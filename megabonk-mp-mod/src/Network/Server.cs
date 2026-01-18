using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MegabonkMP.Core;
using MegabonkMP.Network.Packets;

namespace MegabonkMP.Network
{
    /// <summary>
    /// UDP server with reliability layer for hosting multiplayer sessions.
    /// Manages client connections and packet distribution.
    /// </summary>
    public class Server
    {
        private readonly int _port;
        private readonly int _maxPlayers;
        private UdpClient _socket;
        private Thread _receiveThread;
        private volatile bool _running;
        
        // Client tracking
        private readonly Dictionary<int, ClientConnection> _clients = new();
        private readonly Dictionary<IPEndPoint, int> _endpointToId = new();
        private int _nextClientId = 1; // 0 is reserved for host
        private readonly object _clientLock = new();
        
        // Packet queue for thread-safe processing
        private readonly ConcurrentQueue<(int clientId, byte[] data)> _receiveQueue = new();
        
        // Events
        public event Action<int, string> OnClientConnected;
        public event Action<int> OnClientDisconnected;
        public event Action<int, IPacket> OnPacketReceived;
        
        public Server(int port, int maxPlayers)
        {
            _port = port;
            _maxPlayers = maxPlayers;
        }
        
        public void Start()
        {
            _socket = new UdpClient(_port);
            _socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            _running = true;
            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = "MegabonkMP-Server"
            };
            _receiveThread.Start();
            
            ModLogger.Info($"Server started on port {_port}");
        }
        
        public void Stop()
        {
            _running = false;
            
            // Notify all clients of shutdown
            var disconnectPacket = new DisconnectPacket { Reason = "Server shutting down" };
            Broadcast(disconnectPacket, DeliveryMethod.ReliableOrdered);
            
            Thread.Sleep(100); // Give packets time to send
            
            _socket?.Close();
            _socket = null;
            
            lock (_clientLock)
            {
                _clients.Clear();
                _endpointToId.Clear();
            }
            
            ModLogger.Info("Server stopped");
        }
        
        public void PollEvents()
        {
            while (_receiveQueue.TryDequeue(out var item))
            {
                ProcessReceivedData(item.clientId, item.data);
            }
            
            // Check for timeouts
            CheckClientTimeouts();
        }
        
        public void Broadcast(IPacket packet, DeliveryMethod delivery, int excludeClientId = -1)
        {
            var data = PacketSerializer.Serialize(packet);
            
            lock (_clientLock)
            {
                foreach (var kvp in _clients)
                {
                    if (kvp.Key != excludeClientId)
                    {
                        SendRaw(kvp.Value.EndPoint, data, delivery);
                    }
                }
            }
        }
        
        public void SendTo(int clientId, IPacket packet, DeliveryMethod delivery)
        {
            ClientConnection client;
            lock (_clientLock)
            {
                if (!_clients.TryGetValue(clientId, out client)) return;
            }
            
            var data = PacketSerializer.Serialize(packet);
            SendRaw(client.EndPoint, data, delivery);
        }
        
        private void SendRaw(IPEndPoint endpoint, byte[] data, DeliveryMethod delivery)
        {
            try
            {
                // Wrap with reliability header
                var wrappedData = WrapWithReliability(data, delivery);
                _socket?.Send(wrappedData, wrappedData.Length, endpoint);
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to send to {endpoint}: {ex.Message}");
            }
        }
        
        private byte[] WrapWithReliability(byte[] data, DeliveryMethod delivery)
        {
            // Simple header: [delivery(1)] [sequence(4)] [data...]
            var wrapped = new byte[data.Length + 5];
            wrapped[0] = (byte)delivery;
            // Sequence number would go here for proper implementation
            Array.Copy(data, 0, wrapped, 5, data.Length);
            return wrapped;
        }
        
        private void ReceiveLoop()
        {
            while (_running)
            {
                try
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    var data = _socket.Receive(ref remoteEP);
                    
                    if (data.Length < 5) continue; // Invalid packet
                    
                    // Unwrap reliability header
                    var actualData = new byte[data.Length - 5];
                    Array.Copy(data, 5, actualData, 0, actualData.Length);
                    
                    int clientId;
                    lock (_clientLock)
                    {
                        if (!_endpointToId.TryGetValue(remoteEP, out clientId))
                        {
                            // New connection attempt
                            clientId = HandleNewConnection(remoteEP, actualData);
                            if (clientId < 0) continue;
                        }
                        
                        // Update last seen time
                        if (_clients.TryGetValue(clientId, out var client))
                        {
                            client.LastSeen = DateTime.UtcNow;
                        }
                    }
                    
                    _receiveQueue.Enqueue((clientId, actualData));
                }
                catch (SocketException) when (!_running)
                {
                    // Expected when stopping
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        ModLogger.Error($"Receive error: {ex.Message}");
                    }
                }
            }
        }
        
        private int HandleNewConnection(IPEndPoint endPoint, byte[] data)
        {
            // Check max players
            if (_clients.Count >= _maxPlayers - 1) // -1 for host
            {
                ModLogger.Warning($"Connection rejected from {endPoint}: server full");
                return -1;
            }
            
            // Parse connect request
            var packet = PacketSerializer.Deserialize(data);
            if (packet is not ConnectRequestPacket connectRequest)
            {
                return -1;
            }
            
            int clientId = _nextClientId++;
            var client = new ClientConnection
            {
                ClientId = clientId,
                EndPoint = endPoint,
                PlayerName = connectRequest.PlayerName,
                LastSeen = DateTime.UtcNow
            };
            
            _clients[clientId] = client;
            _endpointToId[endPoint] = clientId;
            
            // Send accept response
            var acceptPacket = new ConnectAcceptPacket { AssignedPlayerId = clientId };
            SendTo(clientId, acceptPacket, DeliveryMethod.ReliableOrdered);
            
            ModLogger.Info($"Client {clientId} ({connectRequest.PlayerName}) connected from {endPoint}");
            OnClientConnected?.Invoke(clientId, connectRequest.PlayerName);
            
            return clientId;
        }
        
        private void ProcessReceivedData(int clientId, byte[] data)
        {
            try
            {
                var packet = PacketSerializer.Deserialize(data);
                if (packet != null)
                {
                    OnPacketReceived?.Invoke(clientId, packet);
                }
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to process packet from client {clientId}: {ex.Message}");
            }
        }
        
        private void CheckClientTimeouts()
        {
            var now = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(10);
            var disconnected = new List<int>();
            
            lock (_clientLock)
            {
                foreach (var kvp in _clients)
                {
                    if (now - kvp.Value.LastSeen > timeout)
                    {
                        disconnected.Add(kvp.Key);
                    }
                }
                
                foreach (var clientId in disconnected)
                {
                    if (_clients.TryGetValue(clientId, out var client))
                    {
                        _endpointToId.Remove(client.EndPoint);
                        _clients.Remove(clientId);
                        ModLogger.Info($"Client {clientId} timed out");
                        OnClientDisconnected?.Invoke(clientId);
                    }
                }
            }
        }
        
        private class ClientConnection
        {
            public int ClientId;
            public IPEndPoint EndPoint;
            public string PlayerName;
            public DateTime LastSeen;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MegabonkMP.Core;
using MegabonkMP.Network.Packets;

namespace MegabonkMP.Network
{
    /// <summary>
    /// UDP client for connecting to multiplayer sessions.
    /// </summary>
    public class Client
    {
        private UdpClient _socket;
        private IPEndPoint _serverEndPoint;
        private Thread _receiveThread;
        private volatile bool _running;
        private string _playerName;
        
        // Packet queue for thread-safe processing
        private readonly ConcurrentQueue<byte[]> _receiveQueue = new();
        
        // Connection tracking
        private DateTime _lastServerContact;
        private float _connectionTimeout = 10f;
        
        // Events
        public event Action<int> OnConnected;
        public event Action OnDisconnected;
        public event Action<IPacket> OnPacketReceived;
        
        public void Connect(string address, int port, string playerName)
        {
            _playerName = playerName;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            
            _socket = new UdpClient();
            _socket.Connect(_serverEndPoint);
            
            _running = true;
            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = "MegabonkMP-Client"
            };
            _receiveThread.Start();
            
            // Send connection request
            var connectPacket = new ConnectRequestPacket { PlayerName = playerName };
            Send(connectPacket, DeliveryMethod.ReliableOrdered);
            
            _lastServerContact = DateTime.UtcNow;
            Logger.Info($"Connection request sent to {address}:{port}");
        }
        
        public void Disconnect()
        {
            if (!_running) return;
            
            _running = false;
            
            try
            {
                var disconnectPacket = new DisconnectPacket { Reason = "Client disconnecting" };
                Send(disconnectPacket, DeliveryMethod.ReliableOrdered);
                Thread.Sleep(50);
            }
            catch { }
            
            _socket?.Close();
            _socket = null;
            
            Logger.Info("Disconnected from server");
        }
        
        public void PollEvents()
        {
            while (_receiveQueue.TryDequeue(out var data))
            {
                ProcessReceivedData(data);
            }
            
            // Check for timeout
            if (_running && (DateTime.UtcNow - _lastServerContact).TotalSeconds > _connectionTimeout)
            {
                Logger.Warning("Connection to server timed out");
                OnDisconnected?.Invoke();
            }
        }
        
        public void Send(IPacket packet, DeliveryMethod delivery)
        {
            if (_socket == null) return;
            
            try
            {
                var data = PacketSerializer.Serialize(packet);
                var wrapped = WrapWithReliability(data, delivery);
                _socket.Send(wrapped, wrapped.Length);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send packet: {ex.Message}");
            }
        }
        
        private byte[] WrapWithReliability(byte[] data, DeliveryMethod delivery)
        {
            var wrapped = new byte[data.Length + 5];
            wrapped[0] = (byte)delivery;
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
                    
                    if (data.Length < 5) continue;
                    
                    var actualData = new byte[data.Length - 5];
                    Array.Copy(data, 5, actualData, 0, actualData.Length);
                    
                    _lastServerContact = DateTime.UtcNow;
                    _receiveQueue.Enqueue(actualData);
                }
                catch (SocketException) when (!_running)
                {
                    // Expected when disconnecting
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Logger.Error($"Receive error: {ex.Message}");
                    }
                }
            }
        }
        
        private void ProcessReceivedData(byte[] data)
        {
            try
            {
                var packet = PacketSerializer.Deserialize(data);
                if (packet == null) return;
                
                // Handle connection accept
                if (packet is ConnectAcceptPacket acceptPacket)
                {
                    Logger.Info($"Connected with player ID {acceptPacket.AssignedPlayerId}");
                    OnConnected?.Invoke(acceptPacket.AssignedPlayerId);
                    return;
                }
                
                // Handle disconnect
                if (packet is DisconnectPacket disconnectPacket)
                {
                    Logger.Info($"Disconnected by server: {disconnectPacket.Reason}");
                    OnDisconnected?.Invoke();
                    return;
                }
                
                OnPacketReceived?.Invoke(packet);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to process packet: {ex.Message}");
            }
        }
    }
}

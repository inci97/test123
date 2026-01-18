using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MegabonkMP.Core;
using MegabonkMP.Network.Packets;

namespace MegabonkMP.Network
{
    /// <summary>
    /// Core networking manager handling client-server architecture.
    /// Uses LiteNetLib-style UDP with reliability layer.
    /// </summary>
    public class NetworkManager
    {
        public static NetworkManager Instance { get; private set; }
        
        // Connection state
        public bool IsConnected => _connectionState == ConnectionState.Connected;
        public bool IsHost { get; private set; }
        public int LocalPlayerId { get; private set; } = -1;
        
        // Network components
        private Server _server;
        private Client _client;
        private ConnectionState _connectionState = ConnectionState.Disconnected;
        
        // Player management
        private readonly Dictionary<int, NetworkPlayer> _players = new();
        private readonly object _playerLock = new();
        
        // Events
        public event Action<NetworkPlayer> OnPlayerConnected;
        public event Action<NetworkPlayer> OnPlayerDisconnected;
        public event Action<ConnectionState> OnConnectionStateChanged;
        public event Action<int, IPacket> OnPacketReceived;
        
        // Configuration
        private int _tickRate = 60;
        private float _tickInterval => 1f / _tickRate;
        private float _tickTimer;
        
        public NetworkManager()
        {
            Instance = this;
            PacketRegistry.Initialize();
            ModLogger.Info("NetworkManager created");
        }
        
        /// <summary>
        /// Host a new multiplayer session.
        /// </summary>
        public void Host(int port, int maxPlayers)
        {
            if (_connectionState != ConnectionState.Disconnected)
            {
                ModLogger.Warning("Already connected, disconnect first");
                return;
            }
            
            try
            {
                IsHost = true;
                _server = new Server(port, maxPlayers);
                _server.OnClientConnected += HandleClientConnected;
                _server.OnClientDisconnected += HandleClientDisconnected;
                _server.OnPacketReceived += HandleServerPacketReceived;
                _server.Start();
                
                // Host also acts as local client
                LocalPlayerId = 0;
                var hostPlayer = new NetworkPlayer(0, "Host", true);
                AddPlayer(hostPlayer);
                
                SetConnectionState(ConnectionState.Connected);
                ModLogger.Info($"Hosting on port {port}, max {maxPlayers} players");
            }
            catch (Exception ex)
            {
                ModLogger.Error("Failed to start host", ex);
                Shutdown();
            }
        }
        
        /// <summary>
        /// Connect to an existing session.
        /// </summary>
        public void Connect(string address, int port, string playerName)
        {
            if (_connectionState != ConnectionState.Disconnected)
            {
                ModLogger.Warning("Already connected, disconnect first");
                return;
            }
            
            try
            {
                IsHost = false;
                _client = new Client();
                _client.OnConnected += HandleClientConnectedToServer;
                _client.OnDisconnected += HandleClientDisconnectedFromServer;
                _client.OnPacketReceived += HandleClientPacketReceived;
                
                SetConnectionState(ConnectionState.Connecting);
                _client.Connect(address, port, playerName);
                ModLogger.Info($"Connecting to {address}:{port}");
            }
            catch (Exception ex)
            {
                ModLogger.Error("Failed to connect", ex);
                Shutdown();
            }
        }
        
        /// <summary>
        /// Disconnect and cleanup.
        /// </summary>
        public void Shutdown()
        {
            ModLogger.Info("Shutting down network...");
            
            _server?.Stop();
            _server = null;
            
            _client?.Disconnect();
            _client = null;
            
            lock (_playerLock)
            {
                _players.Clear();
            }
            
            LocalPlayerId = -1;
            IsHost = false;
            SetConnectionState(ConnectionState.Disconnected);
        }
        
        /// <summary>
        /// Send packet to server (client) or all clients (host).
        /// </summary>
        public void Send(IPacket packet, DeliveryMethod delivery = DeliveryMethod.ReliableOrdered)
        {
            if (!IsConnected) return;
            
            if (IsHost)
            {
                _server?.Broadcast(packet, delivery);
            }
            else
            {
                _client?.Send(packet, delivery);
            }
        }
        
        /// <summary>
        /// Send packet to specific player (host only).
        /// </summary>
        public void SendTo(int playerId, IPacket packet, DeliveryMethod delivery = DeliveryMethod.ReliableOrdered)
        {
            if (!IsHost || _server == null) return;
            _server.SendTo(playerId, packet, delivery);
        }
        
        /// <summary>
        /// Called every frame to process network updates.
        /// </summary>
        public void Update(float deltaTime)
        {
            _server?.PollEvents();
            _client?.PollEvents();
            
            _tickTimer += deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                ProcessTick();
            }
        }
        
        private void ProcessTick()
        {
            // Send position updates, process queued packets, etc.
        }
        
        public NetworkPlayer GetPlayer(int playerId)
        {
            lock (_playerLock)
            {
                return _players.TryGetValue(playerId, out var player) ? player : null;
            }
        }
        
        public IEnumerable<NetworkPlayer> GetAllPlayers()
        {
            lock (_playerLock)
            {
                return new List<NetworkPlayer>(_players.Values);
            }
        }
        
        private void AddPlayer(NetworkPlayer player)
        {
            lock (_playerLock)
            {
                _players[player.PlayerId] = player;
            }
            OnPlayerConnected?.Invoke(player);
        }
        
        private void RemovePlayer(int playerId)
        {
            NetworkPlayer player;
            lock (_playerLock)
            {
                if (_players.TryGetValue(playerId, out player))
                {
                    _players.Remove(playerId);
                }
            }
            if (player != null)
            {
                OnPlayerDisconnected?.Invoke(player);
            }
        }
        
        private void SetConnectionState(ConnectionState state)
        {
            if (_connectionState == state) return;
            _connectionState = state;
            OnConnectionStateChanged?.Invoke(state);
            ModLogger.Info($"Connection state: {state}");
        }
        
        // Server event handlers
        private void HandleClientConnected(int clientId, string playerName)
        {
            var player = new NetworkPlayer(clientId, playerName, false);
            AddPlayer(player);
            
            // Notify other clients
            var joinPacket = new PlayerJoinPacket { PlayerId = clientId, PlayerName = playerName };
            _server.Broadcast(joinPacket, DeliveryMethod.ReliableOrdered, clientId);
            
            // Send existing players to new client
            foreach (var existingPlayer in GetAllPlayers())
            {
                if (existingPlayer.PlayerId != clientId)
                {
                    var existingPacket = new PlayerJoinPacket 
                    { 
                        PlayerId = existingPlayer.PlayerId, 
                        PlayerName = existingPlayer.Name 
                    };
                    _server.SendTo(clientId, existingPacket, DeliveryMethod.ReliableOrdered);
                }
            }
        }
        
        private void HandleClientDisconnected(int clientId)
        {
            RemovePlayer(clientId);
            var leavePacket = new PlayerLeavePacket { PlayerId = clientId };
            _server.Broadcast(leavePacket, DeliveryMethod.ReliableOrdered);
        }
        
        private void HandleServerPacketReceived(int clientId, IPacket packet)
        {
            OnPacketReceived?.Invoke(clientId, packet);
        }
        
        // Client event handlers
        private void HandleClientConnectedToServer(int assignedId)
        {
            LocalPlayerId = assignedId;
            SetConnectionState(ConnectionState.Connected);
        }
        
        private void HandleClientDisconnectedFromServer()
        {
            Shutdown();
        }
        
        private void HandleClientPacketReceived(IPacket packet)
        {
            switch (packet)
            {
                case PlayerJoinPacket joinPacket:
                    var player = new NetworkPlayer(joinPacket.PlayerId, joinPacket.PlayerName, false);
                    AddPlayer(player);
                    break;
                    
                case PlayerLeavePacket leavePacket:
                    RemovePlayer(leavePacket.PlayerId);
                    break;
                    
                default:
                    OnPacketReceived?.Invoke(-1, packet);
                    break;
            }
        }
    }
    
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }
    
    public enum DeliveryMethod
    {
        Unreliable,
        ReliableUnordered,
        ReliableOrdered,
        Sequenced
    }
    
    /// <summary>
    /// Represents a connected player.
    /// </summary>
    public class NetworkPlayer
    {
        public int PlayerId { get; }
        public string Name { get; set; }
        public bool IsHost { get; }
        public bool IsReady { get; set; }
        public float Latency { get; set; }
        
        public NetworkPlayer(int playerId, string name, bool isHost)
        {
            PlayerId = playerId;
            Name = name;
            IsHost = isHost;
        }
    }
}

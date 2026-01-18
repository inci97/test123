using System.IO;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Request to connect to server.
    /// </summary>
    public class ConnectRequestPacket : IPacket
    {
        public PacketType Type => PacketType.ConnectRequest;
        public string PlayerName { get; set; }
        public string ModVersion { get; set; } = Core.PluginInfo.PLUGIN_VERSION;
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerName ?? "Player");
            writer.Write(ModVersion ?? "0.0.0");
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerName = reader.ReadString();
            ModVersion = reader.ReadString();
        }
    }
    
    /// <summary>
    /// Server accepts connection.
    /// </summary>
    public class ConnectAcceptPacket : IPacket
    {
        public PacketType Type => PacketType.ConnectAccept;
        public int AssignedPlayerId { get; set; }
        public int MapSeed { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(AssignedPlayerId);
            writer.Write(MapSeed);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            AssignedPlayerId = reader.ReadInt32();
            MapSeed = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Disconnect notification.
    /// </summary>
    public class DisconnectPacket : IPacket
    {
        public PacketType Type => PacketType.Disconnect;
        public string Reason { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Reason ?? "");
        }
        
        public void Deserialize(BinaryReader reader)
        {
            Reason = reader.ReadString();
        }
    }
    
    /// <summary>
    /// Keep-alive heartbeat.
    /// </summary>
    public class HeartbeatPacket : IPacket
    {
        public PacketType Type => PacketType.Heartbeat;
        public long Timestamp { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Timestamp);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            Timestamp = reader.ReadInt64();
        }
    }
}

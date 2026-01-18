using System.IO;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Player joined the session.
    /// </summary>
    public class PlayerJoinPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerJoin;
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int CharacterId { get; set; }
        public int SkinId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PlayerName ?? "Player");
            writer.Write(CharacterId);
            writer.Write(SkinId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            PlayerName = reader.ReadString();
            CharacterId = reader.ReadInt32();
            SkinId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Player left the session.
    /// </summary>
    public class PlayerLeavePacket : IPacket
    {
        public PacketType Type => PacketType.PlayerLeave;
        public int PlayerId { get; set; }
        public string Reason { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(Reason ?? "");
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            Reason = reader.ReadString();
        }
    }
    
    /// <summary>
    /// Player ready state changed.
    /// </summary>
    public class PlayerReadyPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerReady;
        public int PlayerId { get; set; }
        public bool IsReady { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(IsReady);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            IsReady = reader.ReadBoolean();
        }
    }
    
    /// <summary>
    /// Session is starting (all players ready).
    /// </summary>
    public class SessionStartPacket : IPacket
    {
        public PacketType Type => PacketType.SessionStart;
        public int MapSeed { get; set; }
        public int Difficulty { get; set; }
        public float StartDelay { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(MapSeed);
            writer.Write(Difficulty);
            writer.Write(StartDelay);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            MapSeed = reader.ReadInt32();
            Difficulty = reader.ReadInt32();
            StartDelay = reader.ReadSingle();
        }
    }
}

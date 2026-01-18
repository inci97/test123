using System.IO;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Item spawned in world.
    /// </summary>
    public class ItemSpawnPacket : IPacket
    {
        public PacketType Type => PacketType.ItemSpawn;
        public int ItemNetId { get; set; }
        public int ItemTypeId { get; set; }
        public byte Rarity { get; set; } // 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int SourceEntityId { get; set; } // -1 for chest/world spawn
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ItemNetId);
            writer.Write(ItemTypeId);
            writer.Write(Rarity);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
            writer.Write(SourceEntityId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            ItemNetId = reader.ReadInt32();
            ItemTypeId = reader.ReadInt32();
            Rarity = reader.ReadByte();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
            SourceEntityId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Item picked up by player.
    /// </summary>
    public class ItemPickupPacket : IPacket
    {
        public PacketType Type => PacketType.ItemPickup;
        public int ItemNetId { get; set; }
        public int PlayerId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ItemNetId);
            writer.Write(PlayerId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            ItemNetId = reader.ReadInt32();
            PlayerId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Chest opened by player.
    /// </summary>
    public class ChestOpenPacket : IPacket
    {
        public PacketType Type => PacketType.ChestOpen;
        public int ChestNetId { get; set; }
        public int PlayerId { get; set; }
        public int RoomId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ChestNetId);
            writer.Write(PlayerId);
            writer.Write(RoomId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            ChestNetId = reader.ReadInt32();
            PlayerId = reader.ReadInt32();
            RoomId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Map seed for procedural generation sync.
    /// </summary>
    public class MapSeedPacket : IPacket
    {
        public PacketType Type => PacketType.MapSeed;
        public int Seed { get; set; }
        public int Difficulty { get; set; }
        public int BiomeId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Seed);
            writer.Write(Difficulty);
            writer.Write(BiomeId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            Seed = reader.ReadInt32();
            Difficulty = reader.ReadInt32();
            BiomeId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Player transitioned to new room.
    /// </summary>
    public class RoomTransitionPacket : IPacket
    {
        public PacketType Type => PacketType.RoomTransition;
        public int PlayerId { get; set; }
        public int FromRoomId { get; set; }
        public int ToRoomId { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(FromRoomId);
            writer.Write(ToRoomId);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            FromRoomId = reader.ReadInt32();
            ToRoomId = reader.ReadInt32();
        }
    }
    
    /// <summary>
    /// Chat message.
    /// </summary>
    public class ChatMessagePacket : IPacket
    {
        public PacketType Type => PacketType.ChatMessage;
        public int SenderId { get; set; }
        public string Message { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(SenderId);
            writer.Write(Message ?? "");
        }
        
        public void Deserialize(BinaryReader reader)
        {
            SenderId = reader.ReadInt32();
            Message = reader.ReadString();
        }
    }
    
    /// <summary>
    /// Ping location in world.
    /// </summary>
    public class PingPacket : IPacket
    {
        public PacketType Type => PacketType.Ping;
        public int PlayerId { get; set; }
        public byte PingType { get; set; } // 0=Generic, 1=Enemy, 2=Item, 3=Danger
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PingType);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            PingType = reader.ReadByte();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
        }
    }
}

using System.IO;

namespace MegabonkMP.Network.Packets
{
    /// <summary>
    /// Weapon fired by player.
    /// </summary>
    public class WeaponFirePacket : IPacket
    {
        public PacketType Type => PacketType.WeaponFire;
        public int PlayerId { get; set; }
        public int WeaponId { get; set; }
        public float DirX { get; set; }
        public float DirY { get; set; }
        public float DirZ { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(WeaponId);
            writer.Write(DirX);
            writer.Write(DirY);
            writer.Write(DirZ);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            WeaponId = reader.ReadInt32();
            DirX = reader.ReadSingle();
            DirY = reader.ReadSingle();
            DirZ = reader.ReadSingle();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
        }
    }
    
    /// <summary>
    /// Projectile spawned (for sync across clients).
    /// </summary>
    public class ProjectileSpawnPacket : IPacket
    {
        public PacketType Type => PacketType.ProjectileSpawn;
        public int ProjectileNetId { get; set; }
        public int OwnerPlayerId { get; set; }
        public int ProjectileTypeId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float VelZ { get; set; }
        public float Damage { get; set; }
        public byte ElementType { get; set; } // 1=Poison, 2=Freeze, 4=Burn, 8=Lightning, 32=Charm, 64=Bleeding
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ProjectileNetId);
            writer.Write(OwnerPlayerId);
            writer.Write(ProjectileTypeId);
            writer.Write(PosX);
            writer.Write(PosY);
            writer.Write(PosZ);
            writer.Write(VelX);
            writer.Write(VelY);
            writer.Write(VelZ);
            writer.Write(Damage);
            writer.Write(ElementType);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            ProjectileNetId = reader.ReadInt32();
            OwnerPlayerId = reader.ReadInt32();
            ProjectileTypeId = reader.ReadInt32();
            PosX = reader.ReadSingle();
            PosY = reader.ReadSingle();
            PosZ = reader.ReadSingle();
            VelX = reader.ReadSingle();
            VelY = reader.ReadSingle();
            VelZ = reader.ReadSingle();
            Damage = reader.ReadSingle();
            ElementType = reader.ReadByte();
        }
    }
    
    /// <summary>
    /// Damage dealt to entity.
    /// </summary>
    public class DamageDealtPacket : IPacket
    {
        public PacketType Type => PacketType.DamageDealt;
        public int SourcePlayerId { get; set; }
        public int TargetEntityId { get; set; }
        public bool TargetIsPlayer { get; set; }
        public float Damage { get; set; }
        public bool IsCritical { get; set; }
        public byte ElementType { get; set; }
        public float HitPosX { get; set; }
        public float HitPosY { get; set; }
        public float HitPosZ { get; set; }
        
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(SourcePlayerId);
            writer.Write(TargetEntityId);
            writer.Write(TargetIsPlayer);
            writer.Write(Damage);
            writer.Write(IsCritical);
            writer.Write(ElementType);
            writer.Write(HitPosX);
            writer.Write(HitPosY);
            writer.Write(HitPosZ);
        }
        
        public void Deserialize(BinaryReader reader)
        {
            SourcePlayerId = reader.ReadInt32();
            TargetEntityId = reader.ReadInt32();
            TargetIsPlayer = reader.ReadBoolean();
            Damage = reader.ReadSingle();
            IsCritical = reader.ReadBoolean();
            ElementType = reader.ReadByte();
            HitPosX = reader.ReadSingle();
            HitPosY = reader.ReadSingle();
            HitPosZ = reader.ReadSingle();
        }
    }
}

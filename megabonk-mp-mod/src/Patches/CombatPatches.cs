using HarmonyLib;
using UnityEngine;
using MegabonkMP.Core;
using MegabonkMP.Network;
using MegabonkMP.Network.Packets;
using MegabonkMP.Sync;

namespace MegabonkMP.Patches
{
    /// <summary>
    /// Harmony patches for combat systems.
    /// Handles damage, projectiles, and weapon synchronization.
    /// </summary>
    public static class CombatPatches
    {
        /// <summary>
        /// Hook damage calculation for network sync.
        /// Target: DamageUtility::GetPlayerDamage (0x18043CAE0)
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class DamageUtilityPatch
        {
            // [HarmonyPatch(typeof(Assets.Scripts.Combat.DamageUtility), "GetPlayerDamage")]
            [HarmonyPostfix]
            public static void Postfix(ref float __result, int targetId, bool isCritical, int elementType)
            {
                if (!NetworkManager.Instance?.IsConnected ?? true) return;
                
                // Broadcast damage event
                var packet = new DamageDealtPacket
                {
                    SourcePlayerId = NetworkManager.Instance.LocalPlayerId,
                    TargetEntityId = targetId,
                    TargetIsPlayer = false,
                    Damage = __result,
                    IsCritical = isCritical,
                    ElementType = (byte)elementType
                };
                
                NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableOrdered);
            }
        }
        
        /// <summary>
        /// Hook weapon damage calculation.
        /// Target: WeaponUtility::GetDamage (0x1803FE380)
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class WeaponDamagePatch
        {
            // [HarmonyPatch(typeof(Assets.Scripts.Inventory__Items__Pickups.Weapons.WeaponUtility), "GetDamage")]
            [HarmonyPostfix]
            public static void Postfix(ref float __result)
            {
                // Weapon damage can be modified for balance in multiplayer
                // Currently passthrough, can add scaling here
            }
        }
        
        /// <summary>
        /// Hook weapon fire for projectile sync.
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class WeaponFirePatch
        {
            // [HarmonyPatch(typeof(Assets.Scripts.Combat.Weapon), "Fire")]
            [HarmonyPrefix]
            public static void Prefix(/* Weapon __instance, */ Vector3 direction, Vector3 origin, int weaponId)
            {
                if (!NetworkManager.Instance?.IsConnected ?? true) return;
                
                var packet = new WeaponFirePacket
                {
                    PlayerId = NetworkManager.Instance.LocalPlayerId,
                    WeaponId = weaponId,
                    DirX = direction.x,
                    DirY = direction.y,
                    DirZ = direction.z,
                    PosX = origin.x,
                    PosY = origin.y,
                    PosZ = origin.z
                };
                
                NetworkManager.Instance.Send(packet, DeliveryMethod.ReliableUnordered);
            }
        }
        
        /// <summary>
        /// Hook projectile spawn for network sync.
        /// Note: Object pooling affects this - need to handle pool recycling.
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class ProjectileSpawnPatch
        {
            private static int _projectileNetId = 1;
            
            // [HarmonyPatch(typeof(Assets.Scripts.Combat.ProjectileManager), "SpawnProjectile")]
            [HarmonyPostfix]
            public static void Postfix(GameObject projectile, int ownerId, int projectileType, 
                                        Vector3 position, Vector3 velocity, float damage, int elementType)
            {
                if (!NetworkManager.Instance?.IsConnected ?? true) return;
                if (!NetworkManager.Instance.IsHost) return; // Only host spawns projectiles authoritatively
                
                var packet = new ProjectileSpawnPacket
                {
                    ProjectileNetId = _projectileNetId++,
                    OwnerPlayerId = ownerId,
                    ProjectileTypeId = projectileType,
                    PosX = position.x,
                    PosY = position.y,
                    PosZ = position.z,
                    VelX = velocity.x,
                    VelY = velocity.y,
                    VelZ = velocity.z,
                    Damage = damage,
                    ElementType = (byte)elementType
                };
                
                NetworkManager.Instance.Send(packet, DeliveryMethod.Unreliable);
            }
        }
        
        /// <summary>
        /// Hook enemy damage for multiplayer XP/credit distribution.
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class EnemyDamagePatch
        {
            // [HarmonyPatch(typeof(Assets.Scripts.Enemy.EnemyHealth), "TakeDamage")]
            [HarmonyPostfix]
            public static void Postfix(/* EnemyHealth __instance, */ float damage, int attackerId, 
                                        bool isDead, int enemyNetId)
            {
                if (!NetworkManager.Instance?.IsConnected ?? true) return;
                if (!NetworkManager.Instance.IsHost) return;
                
                if (isDead)
                {
                    // Enemy killed - get XP/credit values (placeholder)
                    int xpReward = 10;
                    int creditReward = 5;
                    
                    EnemySync.NotifyEnemyDeath(enemyNetId, attackerId, xpReward, creditReward);
                }
            }
        }
        
        /// <summary>
        /// Hook player damage (for friendly fire check).
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class PlayerDamagePatch
        {
            // [HarmonyPatch(typeof(Assets.Scripts.Player.PlayerHealth), "TakeDamage")]
            [HarmonyPrefix]
            public static bool Prefix(/* PlayerHealth __instance, */ int attackerId, ref float damage)
            {
                if (!NetworkManager.Instance?.IsConnected ?? true) return true;
                
                // Check if attacker is another player (friendly fire check)
                var attacker = NetworkManager.Instance.GetPlayer(attackerId);
                if (attacker != null)
                {
                    // Check friendly fire config
                    // If disabled, prevent damage from other players
                    // Config would be checked here
                    bool friendlyFireEnabled = false; // Placeholder
                    if (!friendlyFireEnabled)
                    {
                        ModLogger.Debug($"Blocked friendly fire from player {attackerId}");
                        return false; // Skip damage
                    }
                }
                
                return true;
            }
        }
        
        /// <summary>
        /// Hook debuff application for network sync.
        /// Elemental effects: Poison(1), Freeze(2), Burn(4), Lightning(8), Charm(32), Bleeding(64)
        /// TODO: Uncomment [HarmonyPatch] and specify target when game methods are identified
        /// </summary>
        // [HarmonyPatch] - Disabled until target method is identified
        public static class DebuffPatch
        {
            // [HarmonyPatch(typeof(Assets.Scripts.Combat.DebuffManager), "ApplyDebuff")]
            [HarmonyPostfix]
            public static void Postfix(int targetId, int debuffType, float duration, int sourcePlayerId)
            {
                if (!NetworkManager.Instance?.IsConnected ?? true) return;
                
                // Debuff sync would go here
                // Could create a dedicated DebuffPacket if needed
                ModLogger.Debug($"Debuff {debuffType} applied to {targetId} by {sourcePlayerId}");
            }
        }
    }
}

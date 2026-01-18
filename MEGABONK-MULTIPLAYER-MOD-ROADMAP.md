# Megabonk Multiplayer Mod - Development Roadmap

> **Project Status**: Planning Phase  
> **Developer Clearance**: ✅ Confirmed with Ved (Megabonk developers)  
> **Last Updated**: January 18, 2026

---

## 📋 Game Overview

| Property | Details |
|----------|---------|
| **Game** | Megabonk |
| **Genre** | Roguelite Action/Extraction |
| **Developer** | Ved |
| **Engine** | Unity (IL2CPP) |
| **Platform** | PC (Windows x64) |
| **Current Version** | 1.0.49+ |
| **Save Location** | `%AppData%/LocalLow/Ved/Megabonk/` |

### Core Gameplay Features
- Procedurally generated maps
- Fight enemies, collect loot, and extract
- Level-up system with stat bonuses
- Weapons with rarities (Common → Legendary)
- Elemental damage types (Fire, Ice, Lightning, Poison)
- Boss fights, shrines, chests, and challenges
- XP and credits economy
- Multiple playable characters with skins

---

## 🔧 Technical Foundation

### Engine Architecture
- **Backend**: IL2CPP compiled C#
- **Architecture**: x64 Windows
- **Modding Support**: MelonLoader & BepInEx 6 (IL2CPP)

### Core Namespaces
```
Assets.Scripts.Game.MapGeneration.MapEvents
Assets.Scripts.Inventory__Items__Pickups
Assets.Scripts.Inventory__Items__Pickups.Items
Assets.Scripts.Inventory__Items__Pickups.Stats
Assets.Scripts.Inventory__Items__Pickups.Weapons
Assets.Scripts.Menu.Shop
```

### Stat System Architecture
```
StatComponents {
    float _baseValue;           // Base stat value
    float _additiveValue;       // Added bonuses
    float _multiplicativeValue; // Multipliers
}
// Formula: Final = (Add1+Add2) × (Base1+Base2) × (Mult1×Mult2)
```

### Key Systems (60 Total Stats)
| Category | Stats |
|----------|-------|
| Combat | MaxHealth, HealthRegen, Shield, Armor, Evasion, DamageMultiplier, CritChance, CritDamage |
| Special | Overheal, Luck, Difficulty, Execute, Thorns, Lifesteal |
| Movement | MoveSpeedMultiplier, JumpHeight, ExtraJumps |
| Economy | GoldIncreaseMultiplier, XpIncreaseMultiplier, Projectiles |

### Elemental/Debuff System
| ID | Type | Effect |
|----|------|--------|
| 1 | Poison | DoT (stacking) |
| 2 | Freeze | CC |
| 4 | Burn | DoT |
| 8 | Stun/Lightning | CC |
| 32 | Charm | CC |
| 64 | Bleeding | DoT |

---

## 📚 Existing Community Resources

### Existing Multiplayer Mods (Reference)
| Project | Framework | Features | Status |
|---------|-----------|----------|--------|
| [multibonk](https://github.com/guilhermeljs/multibonk) | MelonLoader | TCP-based, port 25565 | Early Dev |
| [megabonk-together](https://github.com/Fcornaire/megabonk-together) | BepInEx 6 | P2P, up to 6 players, matchmaking | Active |

### Useful Repositories
| Repository | Purpose |
|------------|---------|
| [megabonk_research](https://github.com/lukeod/megabonk_research) | Reverse engineering documentation |
| [MEGABONK_SIMPLE_MOD](https://github.com/Oksamies/MEGABONK_SIMPLE_MOD) | Mod template/tutorial |
| [MEGABONK_CustomCharacterMaker](https://github.com/PeterMoras/MEGABONK_CustomCharacterMaker) | Unity tools for custom characters |
| [MegabonkTwitchIntegration](https://github.com/Flowseal/MegabonkTwitchIntegration) | Twitch chat integration |

### Distribution
- **Thunderstore** - Primary mod distribution platform
- **r2modman** - Supported mod manager

---

## 🎯 Development Roadmap

### Phase 1: Foundation (Weeks 1-3)
**Goal**: Set up development environment and core networking

- [ ] **1.1 Environment Setup**
  - [ ] Install Unity (matching game version)
  - [ ] Set up BepInEx 6 Bleeding Edge (IL2CPP) or MelonLoader
  - [ ] Configure Il2CppInterop for runtime injection
  - [ ] Set up development IDE with decompilation tools (dnSpy, ILSpy)

- [ ] **1.2 Game Analysis**
  - [ ] Study existing mods (megabonk-together, multibonk)
  - [ ] Document hook points for player systems
  - [ ] Map memory structures for critical components
  - [ ] Identify state that needs synchronization

- [ ] **1.3 Networking Foundation**
  - [ ] Choose networking library (LiteNetLib, Mirror, or custom)
  - [ ] Implement basic client-server architecture
  - [ ] Design packet protocol for game state
  - [ ] Create connection/disconnection handling

### Phase 2: Core Multiplayer (Weeks 4-8)
**Goal**: Basic multiplayer functionality

- [ ] **2.1 Player Synchronization**
  - [ ] Sync player positions and movement
  - [ ] Sync player animations
  - [ ] Sync player health/shield/overheal
  - [ ] Sync player stats dictionary
  - [ ] Implement interpolation/prediction for smooth movement

- [ ] **2.2 Session Management**
  - [ ] Host/Join functionality
  - [ ] Lobby system with player list
  - [ ] Ready check system
  - [ ] Session persistence during runs

- [ ] **2.3 Map Synchronization**
  - [ ] Sync procedural generation seed
  - [ ] Sync map events and spawns
  - [ ] Sync room transitions
  - [ ] Sync portal/extraction mechanics

### Phase 3: Combat & Items (Weeks 9-14)
**Goal**: Full combat and item synchronization

- [ ] **3.1 Weapon Synchronization**
  - [ ] Sync weapon pickups
  - [ ] Sync weapon upgrades
  - [ ] Sync projectiles (with object pooling awareness)
  - [ ] Sync melee attacks and hitboxes

- [ ] **3.2 Enemy Synchronization**
  - [ ] Sync enemy spawns (host-authoritative)
  - [ ] Sync enemy positions and states
  - [ ] Sync enemy targeting
  - [ ] Sync enemy deaths and drops

- [ ] **3.3 Item & Pickup Synchronization**
  - [ ] Sync chest spawns and openings
  - [ ] Sync item drops
  - [ ] Sync pickup collection
  - [ ] Sync shrine activations and effects

- [ ] **3.4 Damage & Effects**
  - [ ] Sync damage numbers
  - [ ] Sync debuff applications
  - [ ] Sync healing effects
  - [ ] Sync critical hits

### Phase 4: Game Flow (Weeks 15-18)
**Goal**: Complete gameplay loop in multiplayer

- [ ] **4.1 Progression Sync**
  - [ ] Sync XP gain and leveling
  - [ ] Sync credit collection
  - [ ] Sync stat upgrades
  - [ ] Sync character abilities

- [ ] **4.2 Events & Challenges**
  - [ ] Sync boss spawns and fights
  - [ ] Sync challenge room mechanics
  - [ ] Sync timed events
  - [ ] Sync difficulty scaling

- [ ] **4.3 Economy Balancing**
  - [ ] Credit timer scaling per player count
  - [ ] XP multiplier adjustments
  - [ ] Loot distribution system
  - [ ] Boss lamp charge time scaling

### Phase 5: Quality & Polish (Weeks 19-22)
**Goal**: Stable, enjoyable multiplayer experience

- [ ] **5.1 UI/UX**
  - [ ] Multiplayer menu integration
  - [ ] Player nameplates
  - [ ] Shared minimap features
  - [ ] Death/spectator mode
  - [ ] Connection status indicators

- [ ] **5.2 Performance**
  - [ ] Network bandwidth optimization
  - [ ] Tick rate tuning
  - [ ] Client prediction improvements
  - [ ] Lag compensation

- [ ] **5.3 Stability**
  - [ ] Reconnection handling
  - [ ] Host migration (if P2P)
  - [ ] Error recovery
  - [ ] Desync detection and correction

### Phase 6: Advanced Features (Weeks 23-28)
**Goal**: Enhanced multiplayer experience

- [ ] **6.1 Matchmaking**
  - [ ] Online matchmaking server
  - [ ] Private room codes (Friendlies)
  - [ ] Quick match functionality
  - [ ] Region selection

- [ ] **6.2 Social Features**
  - [ ] In-game chat
  - [ ] Ping system
  - [ ] Emotes/reactions
  - [ ] Friend list integration

- [ ] **6.3 Game Modes**
  - [ ] Co-op (standard)
  - [ ] Competitive extraction
  - [ ] Survival mode
  - [ ] Custom game settings

### Phase 7: Release & Maintenance (Ongoing)
**Goal**: Public release and continued support

- [ ] **7.1 Release Preparation**
  - [ ] Comprehensive testing
  - [ ] Documentation
  - [ ] Thunderstore packaging
  - [ ] Trailer/showcase

- [ ] **7.2 Post-Launch**
  - [ ] Bug fixes
  - [ ] Game update compatibility
  - [ ] Community feedback integration
  - [ ] Performance monitoring

---

## ⚖️ Multiplayer Balance Considerations

Based on megabonk-together's research:

| System | Scaling Formula |
|--------|----------------|
| Credit Timer | Adjusted per player count |
| Free Chest Spawns | Increased spawn rate |
| XP Multiplier | 2x for 2-4 players, 3x for 5-6 |
| Boss Lamp Charge | Scaled for faster activation |
| Enemy Cap | 400-600 based on player count |

---

## 🔌 Technical Implementation Notes

### Recommended Mod Framework
**BepInEx 6 Bleeding Edge (IL2CPP)** - More active development, better IL2CPP support
- Package: `BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.752+`

### Key Hook Points
| Function | Address (may vary) | Purpose |
|----------|-------------------|---------|
| `StatComponents::GetFinalValue` | 0x1803f5a70 | Stat calculations |
| `DamageUtility::GetPlayerDamage` | 0x18043CAE0 | Damage processing |
| `PlayerHealth::Tick` | 0x1803E9EB0 | Health updates |
| `WeaponUtility::GetDamage` | 0x1803FE380 | Weapon damage |
| `PlayerMovementValues` | - | Position/movement |

### Network Architecture Options
1. **Client-Server (Recommended)**
   - Dedicated host or player-hosted
   - Authoritative server for anti-cheat
   - Best for competitive modes

2. **P2P with Relay**
   - Direct connection when possible
   - Relay fallback for NAT/firewall issues
   - Used by megabonk-together

### State Synchronization Priority
| Priority | Data | Update Rate |
|----------|------|-------------|
| Critical | Player position, Health | 60Hz |
| High | Enemy positions, Projectiles | 30Hz |
| Medium | Stats, Inventory | On change |
| Low | UI elements, Chat | On change |

---

## 📁 Project Structure (Suggested)

```
megabonk-mp-mod/
├── src/
│   ├── Core/
│   │   ├── Plugin.cs              # BepInEx entry point
│   │   ├── Config.cs              # Mod configuration
│   │   └── Logger.cs              # Logging utilities
│   ├── Network/
│   │   ├── NetworkManager.cs      # Main networking logic
│   │   ├── Packets/               # Packet definitions
│   │   ├── Client.cs              # Client-side networking
│   │   └── Server.cs              # Server-side networking
│   ├── Sync/
│   │   ├── PlayerSync.cs          # Player synchronization
│   │   ├── EnemySync.cs           # Enemy synchronization
│   │   ├── ItemSync.cs            # Item/pickup sync
│   │   └── MapSync.cs             # Map state sync
│   ├── Patches/
│   │   ├── PlayerPatches.cs       # Player system hooks
│   │   ├── CombatPatches.cs       # Combat system hooks
│   │   └── UIPatches.cs           # UI modifications
│   └── UI/
│       ├── LobbyUI.cs             # Lobby interface
│       ├── InGameUI.cs            # Multiplayer HUD
│       └── SettingsUI.cs          # MP settings
├── libs/                          # External dependencies
├── docs/                          # Documentation
├── thunderstore/                  # Thunderstore packaging
│   ├── manifest.json
│   ├── icon.png
│   └── README.md
└── README.md
```

---

## 🛠️ Development Environment Setup

### Prerequisites
1. Unity (version matching Megabonk's build)
2. Visual Studio 2022 or Rider
3. .NET 6.0 SDK
4. BepInEx 6 Bleeding Edge
5. Il2CppInterop
6. Decompilation tools (dnSpy, ILSpy)

### Initial Setup Commands
```bash
# Clone project
git clone <your-repo-url>
cd megabonk-mp-mod

# Install BepInEx to Megabonk directory
# Download from: https://builds.bepinex.dev/projects/bepinex_be

# Generate Il2Cpp assemblies
# Run game once with BepInEx installed

# Reference assemblies from:
# - Megabonk/BepInEx/interop/
# - Megabonk/BepInEx/core/
```

---

## 📞 Resources & Community

- **Megabonk Discord**: Community and mod support
- **Thunderstore**: https://thunderstore.io/ (mod distribution)
- **BepInEx Docs**: https://docs.bepinex.dev/
- **Il2CppInterop**: https://github.com/BepInEx/Il2CppInterop

---

## 📝 Notes

- Disable pause functionality for multiplayer sessions
- Consider save system modifications for online play
- Object pooling affects projectile networking
- Test with various network conditions (latency simulation)
- Keep compatibility with game updates in mind

---

## ✅ Milestones Summary

| Milestone | Target | Status |
|-----------|--------|--------|
| Foundation Complete | Week 3 | ⬜ Not Started |
| Basic MP Working | Week 8 | ⬜ Not Started |
| Combat Synced | Week 14 | ⬜ Not Started |
| Full Game Loop | Week 18 | ⬜ Not Started |
| Polish Complete | Week 22 | ⬜ Not Started |
| Advanced Features | Week 28 | ⬜ Not Started |
| Public Release | Week 30 | ⬜ Not Started |

---

*This roadmap is a living document and will be updated as development progresses.*

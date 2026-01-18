# Megabonk Multiplayer Mod

A multiplayer mod for Megabonk built with BepInEx 6 IL2CPP. Transform the single-player roguelite into a cooperative multiplayer experience!

## ✨ Features

- **In-Game Menu**: Press F9 to open the multiplayer menu
- **Host & Join**: Easy server hosting and client connection
- **Player Sync**: Real-time player position and state synchronization
- **Settings**: Configurable gameplay options (XP multiplier, friendly fire, etc.)
- **Network Stats**: Built-in debugging and performance monitoring
- **Cross-Platform**: Windows client support

## 📦 Installation

### Option 1: Automatic (Recommended)

1. Download `MegabonkMP_Launcher.exe` from [Releases](https://github.com/inci97/test123/releases)
2. Run the launcher
3. Select your Megabonk game folder
4. Click "Install BepInEx" then "Build && Install Mod"
5. Launch Megabonk and press F9 for the multiplayer menu!

### Option 2: Manual Installation

#### Prerequisites
- Megabonk installed
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [BepInEx 6 Bleeding Edge](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.2)

#### Steps
1. Install BepInEx by extracting to your Megabonk folder
2. Run Megabonk once to generate interop assemblies
3. Clone/download this repository
4. Build the mod:
   ```bash
   cd megabonk-mp-mod/src
   dotnet build -c Release
   ```
5. Copy `bin/Release/MegabonkMP.dll` to `Megabonk/BepInEx/plugins/MegabonkMP/`
6. Launch the game and press F9!

## 🎮 Usage

### In-Game Controls
- **F9**: Toggle multiplayer menu
- **F10**: Quick disconnect
- **F3**: Toggle network stats overlay (when enabled in settings)

### Multiplayer Menu
- **Multiplayer Tab**: Host/join games, view player list
- **Settings Tab**: Configure display and gameplay options
- **Debug Tab**: Network statistics and console logs

### Hosting a Game
1. Press F9 to open menu
2. Enter your player name
3. Set port (default: 7777) and max players (2-6)
4. Click "Host Game"
5. Share your IP address with friends

### Joining a Game
1. Press F9 to open menu
2. Enter player name and server IP/port
3. Click "Join Game"

## 🏗️ Project Structure

```
megabonk-mp-mod/
├── src/
│   ├── Core/           # Plugin entry, config, logging
│   ├── Network/        # Client-server networking
│   │   └── Packets/    # Network packet definitions
│   ├── Sync/           # State synchronization
│   ├── Patches/        # Harmony patches for game hooks
│   └── UI/             # Multiplayer UI components
├── launcher/           # Python GUI launcher
├── thunderstore/       # Thunderstore distribution files
└── docs/               # Additional documentation
```

## 🔧 Development

### Prerequisites
- Unity (matching Megabonk version)
- .NET 6.0 SDK
- BepInEx 6 Bleeding Edge
- Il2CppInterop for runtime injection

### Building
```bash
cd megabonk-mp-mod/src
dotnet build -c Release
```

### Key Components

#### Network Architecture
- **NetworkManager**: Core networking logic
- **Server/Client**: UDP-based communication
- **Packet System**: Structured data serialization

#### Synchronization
- **PlayerSync**: Position, animation, health sync
- **EnemySync**: Enemy state synchronization
- **ItemSync**: Loot and pickup coordination
- **MapSync**: Procedural generation seed sharing

#### UI System
- **ModMenu**: IMGUI-based in-game interface
- **SettingsUI**: Configuration management
- **LobbyUI**: Pre-game setup

## 🐛 Known Issues & Limitations

- Patches are currently disabled (target methods need identification)
- Limited enemy synchronization
- No matchmaking system yet
- Windows-only client support

## 📋 Roadmap

See [ROADMAP.md](../MEGABONK-MULTIPLAYER-MOD-ROADMAP.md) for detailed development plans.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

### Development Setup
1. Install prerequisites
2. Clone with submodules: `git clone --recursive`
3. Build and test locally
4. Check BepInEx logs for debugging

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **Ved** - Megabonk developer for modding support
- **BepInEx Team** - IL2CPP modding framework
- **Community** - Research and reference implementations
- **Harmony** - Runtime patching library
- `DamageUtility::GetPlayerDamage` - Damage processing  
- `PlayerHealth::Tick` - Health updates
- `WeaponUtility::GetDamage` - Weapon damage

### Network Protocol

- UDP with reliability layer
- 60Hz position updates (players)
- 30Hz position updates (enemies)
- Reliable ordered for state changes

### Packet Types

See `src/Network/Packets/` for all packet definitions.

## Testing

1. Run two instances of Megabonk
2. Host on one instance
3. Join from the other using 127.0.0.1

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

This project is for educational purposes. Megabonk is property of Ved.

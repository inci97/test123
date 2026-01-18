# Megabonk Multiplayer Mod

Play Megabonk with friends! This mod adds multiplayer support for up to 6 players.

## Features

- **Co-op Gameplay**: Play through runs together with friends
- **Host/Join System**: Easy lobby system with direct connect
- **Synchronized Gameplay**:
  - Player positions and animations
  - Enemy spawns and combat
  - Items, chests, and loot
  - Map generation (shared seed)
  - XP and credits

## Installation

### Using r2modman (Recommended)
1. Install [r2modman](https://thunderstore.io/package/ebkr/r2modman/)
2. Search for "MegabonkMP" in the mod browser
3. Click Install

### Manual Installation
1. Install [BepInEx 6 IL2CPP](https://builds.bepinex.dev/projects/bepinex_be)
2. Download this mod
3. Extract to `Megabonk/BepInEx/plugins/`

## How to Play

### Hosting a Game
1. Launch Megabonk
2. Click "Multiplayer" on the main menu
3. Set your player name and port (default: 7777)
4. Click "Host Game"
5. Share your IP address with friends

### Joining a Game
1. Launch Megabonk
2. Click "Multiplayer" on the main menu
3. Enter the host's IP address and port
4. Enter your player name
5. Click "Join Game"

## Configuration

Edit `BepInEx/config/com.megabonk.multiplayer.cfg`:

| Setting | Default | Description |
|---------|---------|-------------|
| ServerPort | 7777 | Port for hosting |
| MaxPlayers | 4 | Maximum players (2-6) |
| FriendlyFire | false | Allow player damage |
| SharedLoot | true | Share loot drops |
| XpMultiplier | 2.0 | XP scaling for multiplayer |

## Multiplayer Balance

- XP is multiplied (2x for 2-4 players, 3x for 5-6)
- Credit timer is scaled for player count
- Enemy cap scales with players (400-600)
- Boss lamp charge time adjusted

## Troubleshooting

### Can't Connect
- Ensure port forwarding is configured (TCP/UDP 7777)
- Check firewall settings
- Verify both players have same mod version

### Desync Issues
- Host has authority over enemies and items
- Ensure stable network connection
- Report persistent issues on GitHub

## Credits

- Developed with permission from Ved (Megabonk developers)
- Built on BepInEx and Harmony
- Inspired by megabonk-together and multibonk projects

## Links

- [Bug Reports](https://github.com/yourusername/megabonk-mp-mod/issues)
- [Discord](#)
- [Source Code](https://github.com/yourusername/megabonk-mp-mod)

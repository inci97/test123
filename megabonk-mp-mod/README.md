# Megabonk Multiplayer Mod

A multiplayer mod for Megabonk built with BepInEx 6 IL2CPP.

## Project Structure

```
megabonk-mp-mod/
├── src/
│   ├── Core/           # Plugin entry, config, logging
│   ├── Network/        # Client-server networking
│   │   └── Packets/    # Network packet definitions
│   ├── Sync/           # State synchronization
│   ├── Patches/        # Harmony patches for game hooks
│   └── UI/             # Multiplayer UI components
├── thunderstore/       # Thunderstore distribution files
├── libs/               # External dependencies
└── docs/               # Additional documentation
```

## Building

### Prerequisites

1. .NET 6.0 SDK
2. Megabonk installed
3. BepInEx 6 Bleeding Edge installed in Megabonk
4. Run game once to generate interop assemblies

### Build Steps

1. Set environment variable:
   ```
   set MEGABONK_PATH=C:\Path\To\Megabonk
   ```

2. Build the project:
   ```
   cd src
   dotnet build -c Release
   ```

3. The DLL will be copied to `Megabonk/BepInEx/plugins/MegabonkMP/`

## Development

### Hook Points

Key game methods to hook (addresses from research):
- `StatComponents::GetFinalValue` - Stat calculations
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

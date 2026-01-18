# Megabonk MP Mod Launcher

A simple GUI launcher for the Megabonk Multiplayer Mod.

## Download

Get the latest `MegabonkMP_Launcher.exe` from the [Releases](../../releases) page.

No Python installation required - just download and run!

## Building from Source

### Option 1: Use Pre-built EXE
Download `MegabonkMP_Launcher.exe` from releases - no dependencies needed.

### Option 2: Build EXE Yourself
1. Install Python 3.8+ from https://python.org
2. Run `build_exe.bat`
3. Find the EXE in `dist/MegabonkMP_Launcher.exe`

### Option 3: Run Python Script Directly
Requires Python 3.8+ with tkinter installed.

## Features

- **Game Path Selection**: Browse or auto-detect Megabonk installation
- **BepInEx Installation**: One-click download and install of BepInEx 6 IL2CPP
- **Mod Installation**: Install/update the multiplayer mod
- **Server Settings**: Configure connection settings before launch
  - Server address and port
  - Max players
  - Gameplay options (friendly fire, shared loot, XP multiplier)
- **Log Viewer**: View launcher and BepInEx logs
- **Settings Management**: All settings saved automatically

## Requirements

- Python 3.8 or higher
- tkinter (usually included with Python)

### Installing Python

**Windows:**
1. Download from https://www.python.org/downloads/
2. Run installer
3. **Check "Add Python to PATH"**
4. Click Install

**Linux:**
```bash
# Ubuntu/Debian
sudo apt install python3 python3-tk

# Fedora
sudo dnf install python3 python3-tkinter

# Arch
sudo pacman -S python tk
```

**Mac:**
```bash
brew install python3 python-tk
```

## Usage

### Windows
Double-click `launch.bat` or run:
```
python launcher.py
```

### Linux/Mac
```bash
chmod +x launch.sh
./launch.sh
```
Or:
```bash
python3 launcher.py
```

## Tabs

### Launch Tab
- Set game installation path
- View installation status (BepInEx and mod)
- Install BepInEx and mod
- Set player name
- Launch game

### Server Tab
- Server address (IP or hostname)
- Port number (default: 7777)
- Max players (2-6)
- Gameplay options:
  - Friendly Fire
  - Shared Loot
  - XP Multiplier

### Settings Tab
- UI options (nameplates, network stats)
- Debug mode
- Quick access to folders and logs

### Logs Tab
- View launcher logs
- View BepInEx logs
- Copy/save logs for troubleshooting

## Configuration

Settings are saved to `launcher_config.json` in the launcher directory.

Server settings are written to `BepInEx/config/com.megabonk.multiplayer.cfg`.

## Troubleshooting

### Python not found
Make sure Python is installed and added to PATH.

### tkinter not found
Install python-tk package for your system.

### BepInEx download fails
Check your internet connection and firewall settings.

### Game won't launch
Verify the game path is correct and Megabonk.exe exists.

#!/usr/bin/env python3
"""
Megabonk Multiplayer Mod Launcher
A simple GUI launcher for installing BepInEx and configuring the mod.
"""

import os
import sys
import json
import shutil
import zipfile
import logging
import threading
import webbrowser
import urllib.request
import subprocess
from datetime import datetime
from pathlib import Path

try:
    import tkinter as tk
    from tkinter import ttk, filedialog, messagebox, scrolledtext
except ImportError:
    print("Error: tkinter not available. Install python3-tk")
    sys.exit(1)

# Constants
APP_NAME = "Megabonk MP Launcher"
APP_VERSION = "1.2.0"
CONFIG_FILE = "launcher_config.json"
LOG_FILE = "launcher.log"

BEPINEX_URL = "https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip"
BEPINEX_VERSION = "6.0.0-pre.2"

# GitHub source repository
GITHUB_REPO = "inci97/test123"
GITHUB_SOURCE_URL = f"https://github.com/{GITHUB_REPO}/archive/refs/heads/main.zip"
MOD_SOURCE_FOLDER = "megabonk-mp-mod"

DEFAULT_CONFIG = {
    "game_path": "",
    "player_name": "Player",
    "server_address": "127.0.0.1",
    "server_port": 7777,
    "max_players": 4,
    "auto_connect": False,
    "friendly_fire": False,
    "shared_loot": True,
    "xp_multiplier": 2.0,
    "show_nameplates": True,
    "show_network_stats": False,
    "debug_mode": False
}


class LauncherApp:
    def __init__(self, root):
        self.root = root
        self.root.title(f"{APP_NAME} v{APP_VERSION}")
        self.root.geometry("800x600")
        self.root.minsize(700, 500)
        
        # Setup logging
        self.setup_logging()
        
        # Load config
        self.config = self.load_config()
        
        # Variables
        self.game_path_var = tk.StringVar(value=self.config.get("game_path", ""))
        self.player_name_var = tk.StringVar(value=self.config.get("player_name", "Player"))
        self.server_address_var = tk.StringVar(value=self.config.get("server_address", "127.0.0.1"))
        self.server_port_var = tk.IntVar(value=self.config.get("server_port", 7777))
        self.max_players_var = tk.IntVar(value=self.config.get("max_players", 4))
        self.auto_connect_var = tk.BooleanVar(value=self.config.get("auto_connect", False))
        self.friendly_fire_var = tk.BooleanVar(value=self.config.get("friendly_fire", False))
        self.shared_loot_var = tk.BooleanVar(value=self.config.get("shared_loot", True))
        self.xp_multiplier_var = tk.DoubleVar(value=self.config.get("xp_multiplier", 2.0))
        self.show_nameplates_var = tk.BooleanVar(value=self.config.get("show_nameplates", True))
        self.show_network_stats_var = tk.BooleanVar(value=self.config.get("show_network_stats", False))
        self.debug_mode_var = tk.BooleanVar(value=self.config.get("debug_mode", False))
        
        # Status
        self.status_var = tk.StringVar(value="Ready")
        self.bepinex_installed = tk.BooleanVar(value=False)
        self.mod_installed = tk.BooleanVar(value=False)
        
        # Build UI
        self.create_ui()
        
        # Check installation status
        self.check_installation_status()
        
        self.log("Launcher started")
    
    def setup_logging(self):
        """Setup logging to file and memory buffer"""
        self.log_buffer = []
        
        logging.basicConfig(
            level=logging.DEBUG,
            format='%(asctime)s [%(levelname)s] %(message)s',
            handlers=[
                logging.FileHandler(LOG_FILE, encoding='utf-8'),
                logging.StreamHandler(sys.stdout)
            ]
        )
        self.logger = logging.getLogger(__name__)
    
    def log(self, message, level="INFO"):
        """Log message and update log display"""
        timestamp = datetime.now().strftime("%H:%M:%S")
        log_entry = f"[{timestamp}] [{level}] {message}"
        self.log_buffer.append(log_entry)
        
        if level == "ERROR":
            self.logger.error(message)
        elif level == "WARNING":
            self.logger.warning(message)
        else:
            self.logger.info(message)
        
        # Update log display if it exists
        if hasattr(self, 'log_text'):
            self.log_text.config(state=tk.NORMAL)
            self.log_text.insert(tk.END, log_entry + "\n")
            self.log_text.see(tk.END)
            self.log_text.config(state=tk.DISABLED)
    
    def load_config(self):
        """Load configuration from file"""
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, 'r') as f:
                    config = json.load(f)
                    # Merge with defaults for any missing keys
                    return {**DEFAULT_CONFIG, **config}
            except Exception as e:
                print(f"Failed to load config: {e}")
        return DEFAULT_CONFIG.copy()
    
    def save_config(self):
        """Save configuration to file"""
        config = {
            "game_path": self.game_path_var.get(),
            "player_name": self.player_name_var.get(),
            "server_address": self.server_address_var.get(),
            "server_port": self.server_port_var.get(),
            "max_players": self.max_players_var.get(),
            "auto_connect": self.auto_connect_var.get(),
            "friendly_fire": self.friendly_fire_var.get(),
            "shared_loot": self.shared_loot_var.get(),
            "xp_multiplier": self.xp_multiplier_var.get(),
            "show_nameplates": self.show_nameplates_var.get(),
            "show_network_stats": self.show_network_stats_var.get(),
            "debug_mode": self.debug_mode_var.get()
        }
        
        try:
            with open(CONFIG_FILE, 'w') as f:
                json.dump(config, f, indent=2)
            self.log("Configuration saved")
        except Exception as e:
            self.log(f"Failed to save config: {e}", "ERROR")
    
    def create_ui(self):
        """Create the main UI"""
        # Create notebook for tabs
        self.notebook = ttk.Notebook(self.root)
        self.notebook.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        
        # Create tabs
        self.create_main_tab()
        self.create_settings_tab()
        self.create_log_tab()
        
        def create_main_tab(self):
        status_frame = ttk.Frame(self.root)
        status_frame.pack(fill=tk.X, padx=5, pady=2)
        
        ttk.Label(status_frame, textvariable=self.status_var).pack(side=tk.LEFT)
        
        # Version label
        ttk.Label(status_frame, text=f"v{APP_VERSION}").pack(side=tk.RIGHT)
    
    def create_main_tab(self):
        """Create the main/launch tab"""
        main_frame = ttk.Frame(self.notebook, padding=10)
        self.notebook.add(main_frame, text="  Launch  ")
        
        # Game Path Section
        path_frame = ttk.LabelFrame(main_frame, text="Game Installation", padding=10)
        path_frame.pack(fill=tk.X, pady=5)
        
        ttk.Label(path_frame, text="Megabonk Path:").pack(anchor=tk.W)
        
        path_entry_frame = ttk.Frame(path_frame)
        path_entry_frame.pack(fill=tk.X, pady=2)
        
        self.path_entry = ttk.Entry(path_entry_frame, textvariable=self.game_path_var, width=60)
        self.path_entry.pack(side=tk.LEFT, fill=tk.X, expand=True)
        
        ttk.Button(path_entry_frame, text="Browse...", command=self.browse_game_path).pack(side=tk.LEFT, padx=5)
        ttk.Button(path_entry_frame, text="Auto-Detect", command=self.auto_detect_game).pack(side=tk.LEFT)
        
        # Installation Status
        status_frame = ttk.LabelFrame(main_frame, text="Installation Status", padding=10)
        status_frame.pack(fill=tk.X, pady=5)
            self.build_and_install_btn = ttk.Button(install_frame, text="Build && Install Mod", 
                                             command=self.build_and_install_mod)
            self.build_and_install_btn.pack(side=tk.LEFT, padx=5)
        
        self.mod_status = ttk.Label(status_frame, text="⬜ Mod not installed")
        self.mod_status.pack(anchor=tk.W)
            # Removed separate install button
        
        # Install Buttons - Row 1
        install_frame = ttk.Frame(status_frame)
        install_frame.pack(fill=tk.X, pady=5)
        
        self.install_bepinex_btn = ttk.Button(install_frame, text="Install BepInEx", 
                                               command=self.install_bepinex)
        self.install_bepinex_btn.pack(side=tk.LEFT, padx=5)
        
        self.download_source_btn = ttk.Button(install_frame, text="Download Source", 
                                               command=self.download_source)
        self.download_source_btn.pack(side=tk.LEFT, padx=5)
        
        self.build_mod_btn = ttk.Button(install_frame, text="Build Mod", 
                                         command=self.build_mod)

        self.install_mod_btn = ttk.Button(install_frame2, text="Install/Update Mod", 
                                           command=self.install_mod)
        self.install_mod_btn.pack(side=tk.LEFT, padx=5)
        
        ttk.Button(install_frame2, text="Verify Installation", 
                   command=self.check_installation_status).pack(side=tk.LEFT, padx=5)
        
        # Player Settings
        player_frame = ttk.LabelFrame(main_frame, text="Player", padding=10)
        player_frame.pack(fill=tk.X, pady=5)
        
        ttk.Label(player_frame, text="Player Name:").pack(side=tk.LEFT)
        ttk.Entry(player_frame, textvariable=self.player_name_var, width=20).pack(side=tk.LEFT, padx=10)
        
        # Launch Section
        launch_frame = ttk.Frame(main_frame)
        launch_frame.pack(fill=tk.X, pady=20)
        
        self.launch_btn = ttk.Button(launch_frame, text="🎮  Launch Megabonk", 
                                      command=self.launch_game, style='Accent.TButton')
        self.launch_btn.pack(pady=10)
        
        # Quick connect option
        ttk.Checkbutton(launch_frame, text="Auto-connect on launch", 
                        variable=self.auto_connect_var).pack()
    
    def create_server_tab(self):
        """Create server/connection settings tab"""
        server_frame = ttk.Frame(self.notebook, padding=10)
        self.notebook.add(server_frame, text="  Server  ")
        
        # Connection Settings
        conn_frame = ttk.LabelFrame(server_frame, text="Connection Settings", padding=10)
        conn_frame.pack(fill=tk.X, pady=5)
        
        # Address
        addr_frame = ttk.Frame(conn_frame)
        addr_frame.pack(fill=tk.X, pady=2)
        ttk.Label(addr_frame, text="Server Address:", width=15).pack(side=tk.LEFT)
        ttk.Entry(addr_frame, textvariable=self.server_address_var, width=30).pack(side=tk.LEFT)
        ttk.Label(addr_frame, text="(IP or hostname)", foreground="gray").pack(side=tk.LEFT, padx=10)
        
        # Port
        port_frame = ttk.Frame(conn_frame)
        port_frame.pack(fill=tk.X, pady=2)
        ttk.Label(port_frame, text="Port:", width=15).pack(side=tk.LEFT)
        ttk.Spinbox(port_frame, textvariable=self.server_port_var, 
                    from_=1024, to=65535, width=10).pack(side=tk.LEFT)
        ttk.Label(port_frame, text="(1024-65535, default: 7777)", foreground="gray").pack(side=tk.LEFT, padx=10)
        
        # Preset buttons
        preset_frame = ttk.Frame(conn_frame)
        preset_frame.pack(fill=tk.X, pady=10)
        ttk.Label(preset_frame, text="Quick Connect:").pack(side=tk.LEFT)
        ttk.Button(preset_frame, text="Localhost", 
                   command=lambda: self.server_address_var.set("127.0.0.1")).pack(side=tk.LEFT, padx=5)
        ttk.Button(preset_frame, text="LAN", 
                   command=self.detect_lan_ip).pack(side=tk.LEFT, padx=5)
        
        # Host Settings
        host_frame = ttk.LabelFrame(server_frame, text="Host Settings", padding=10)
        host_frame.pack(fill=tk.X, pady=5)
        
        # Max Players
        players_frame = ttk.Frame(host_frame)
        players_frame.pack(fill=tk.X, pady=2)
        ttk.Label(players_frame, text="Max Players:", width=15).pack(side=tk.LEFT)
        ttk.Spinbox(players_frame, textvariable=self.max_players_var, 
                    from_=2, to=6, width=5).pack(side=tk.LEFT)
        
        # Gameplay Options
        gameplay_frame = ttk.LabelFrame(server_frame, text="Gameplay Options (Host)", padding=10)
        gameplay_frame.pack(fill=tk.X, pady=5)
        
        ttk.Checkbutton(gameplay_frame, text="Friendly Fire (allow player damage)", 
                        variable=self.friendly_fire_var).pack(anchor=tk.W)
        ttk.Checkbutton(gameplay_frame, text="Shared Loot (loot visible to all)", 
                        variable=self.shared_loot_var).pack(anchor=tk.W)
        
        # XP Multiplier
        xp_frame = ttk.Frame(gameplay_frame)
        xp_frame.pack(fill=tk.X, pady=5)
        ttk.Label(xp_frame, text="XP Multiplier:").pack(side=tk.LEFT)
        xp_scale = ttk.Scale(xp_frame, from_=1.0, to=5.0, 
                             variable=self.xp_multiplier_var, orient=tk.HORIZONTAL, length=200)
        xp_scale.pack(side=tk.LEFT, padx=10)
        self.xp_label = ttk.Label(xp_frame, text=f"{self.xp_multiplier_var.get():.1f}x")
        self.xp_label.pack(side=tk.LEFT)
        xp_scale.config(command=lambda v: self.xp_label.config(text=f"{float(v):.1f}x"))
        
        # Apply Button
        ttk.Button(server_frame, text="Save & Apply Settings", 
                   command=self.apply_server_settings).pack(pady=20)
    
    def create_settings_tab(self):
        """Create general settings tab"""
        settings_frame = ttk.Frame(self.notebook, padding=10)
        self.notebook.add(settings_frame, text="  Settings  ")
        
        # UI Settings
        ui_frame = ttk.LabelFrame(settings_frame, text="UI Settings", padding=10)
        ui_frame.pack(fill=tk.X, pady=5)
        
        ttk.Checkbutton(ui_frame, text="Show player nameplates", 
                        variable=self.show_nameplates_var).pack(anchor=tk.W)
        ttk.Checkbutton(ui_frame, text="Show network statistics overlay", 
                        variable=self.show_network_stats_var).pack(anchor=tk.W)
        
        # Debug Settings
        debug_frame = ttk.LabelFrame(settings_frame, text="Debug", padding=10)
        debug_frame.pack(fill=tk.X, pady=5)
        
        ttk.Checkbutton(debug_frame, text="Enable debug mode (verbose logging)", 
                        variable=self.debug_mode_var).pack(anchor=tk.W)
        
        # Buttons
        btn_frame = ttk.Frame(debug_frame)
        btn_frame.pack(fill=tk.X, pady=5)
        
        ttk.Button(btn_frame, text="Open Game Folder", 
                   command=self.open_game_folder).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Open Config Folder", 
                   command=self.open_config_folder).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Open BepInEx Logs", 
                   command=self.open_bepinex_logs).pack(side=tk.LEFT, padx=5)
        
        # About
        about_frame = ttk.LabelFrame(settings_frame, text="About", padding=10)
        about_frame.pack(fill=tk.X, pady=5)
        
        ttk.Label(about_frame, text=f"{APP_NAME} v{APP_VERSION}").pack(anchor=tk.W)
        ttk.Label(about_frame, text=f"BepInEx Version: {BEPINEX_VERSION}").pack(anchor=tk.W)
        ttk.Label(about_frame, text="Mod for Megabonk by Ved").pack(anchor=tk.W)
        
        link_frame = ttk.Frame(about_frame)
        link_frame.pack(fill=tk.X, pady=5)
        ttk.Button(link_frame, text="GitHub", 
                   command=lambda: webbrowser.open("https://github.com")).pack(side=tk.LEFT, padx=5)
        ttk.Button(link_frame, text="Discord", 
                   command=lambda: webbrowser.open("https://discord.gg")).pack(side=tk.LEFT, padx=5)
        
        # Save Button
        ttk.Button(settings_frame, text="Save All Settings", 
                   command=self.save_config).pack(pady=20)
    
    def create_log_tab(self):
        """Create log viewer tab"""
        log_frame = ttk.Frame(self.notebook, padding=10)
        self.notebook.add(log_frame, text="  Logs  ")
        
        # Log text area
        self.log_text = scrolledtext.ScrolledText(log_frame, height=20, state=tk.DISABLED,
                                                   font=('Consolas', 9))
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        # Load existing log buffer
        for entry in self.log_buffer:
            self.log_text.config(state=tk.NORMAL)
            self.log_text.insert(tk.END, entry + "\n")
            self.log_text.config(state=tk.DISABLED)
        
        # Buttons
        btn_frame = ttk.Frame(log_frame)
        btn_frame.pack(fill=tk.X, pady=5)
        
        ttk.Button(btn_frame, text="Clear Log", command=self.clear_log).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Copy to Clipboard", command=self.copy_log).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Save Log", command=self.save_log).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Refresh", command=self.refresh_log).pack(side=tk.RIGHT, padx=5)
    
    def browse_game_path(self):
        """Open file browser to select game path"""
        initial_dir = self.game_path_var.get() or os.path.expanduser("~")
        path = filedialog.askdirectory(title="Select Megabonk Installation Folder",
                                        initialdir=initial_dir)
        if path:
            self.game_path_var.set(path)
            self.check_installation_status()
            self.save_config()
    
    def auto_detect_game(self):
        """Try to auto-detect game installation"""
        self.log("Auto-detecting game installation...")
        
        # Common Steam paths
        possible_paths = [
            r"C:\Program Files (x86)\Steam\steamapps\common\Megabonk",
            r"C:\Program Files\Steam\steamapps\common\Megabonk",
            r"D:\Steam\steamapps\common\Megabonk",
            r"D:\SteamLibrary\steamapps\common\Megabonk",
            r"E:\SteamLibrary\steamapps\common\Megabonk",
            os.path.expanduser("~/.steam/steam/steamapps/common/Megabonk"),
            os.path.expanduser("~/Library/Application Support/Steam/steamapps/common/Megabonk"),
        ]
        
        for path in possible_paths:
            if os.path.exists(path) and os.path.exists(os.path.join(path, "Megabonk.exe")):
                self.game_path_var.set(path)
                self.log(f"Found game at: {path}")
                self.check_installation_status()
                self.save_config()
                return
        
        self.log("Could not auto-detect game. Please browse manually.", "WARNING")
        messagebox.showwarning("Not Found", "Could not auto-detect Megabonk installation.\n"
                                            "Please browse to the game folder manually.")
    
    def check_installation_status(self):
        """Check if BepInEx and mod are installed"""
        game_path = self.game_path_var.get()
        
        if not game_path or not os.path.exists(game_path):
            self.bepinex_status.config(text="⬜ Game path not set")
            self.mod_status.config(text="⬜ Game path not set")
            self.bepinex_installed.set(False)
            self.mod_installed.set(False)
        else:
            # Check BepInEx
            bepinex_path = os.path.join(game_path, "BepInEx")
            bepinex_core = os.path.join(bepinex_path, "core", "BepInEx.Core.dll")
            
            if os.path.exists(bepinex_core):
                self.bepinex_status.config(text="✅ BepInEx installed")
                self.bepinex_installed.set(True)
                self.log("BepInEx found")
            else:
                self.bepinex_status.config(text="❌ BepInEx not installed")
                self.bepinex_installed.set(False)
            
            # Check Mod
            mod_path = os.path.join(bepinex_path, "plugins", "MegabonkMP")
            mod_dll = os.path.join(mod_path, "MegabonkMP.dll")
            
            if os.path.exists(mod_dll):
                self.mod_status.config(text="✅ MegabonkMP mod installed")
                self.mod_installed.set(True)
                self.log("MegabonkMP mod found")
            else:
                self.mod_status.config(text="❌ MegabonkMP mod not installed")
                self.mod_installed.set(False)
        
        # Check Source (independent of game path)
        mod_source = self.get_mod_source_dir()
        csproj_path = os.path.join(mod_source, "MegabonkMP.csproj")
                # Check if mod DLL is latest built version
                mod_source = self.get_mod_source_dir()
                built_dll = os.path.join(mod_source, "bin", "Release", "MegabonkMP.dll")
                if os.path.exists(mod_dll):
                    is_latest = False
                    if os.path.exists(built_dll):
                        try:
                            mod_time = os.path.getmtime(mod_dll)
                            built_time = os.path.getmtime(built_dll)
                            is_latest = mod_time >= built_time
                        except Exception:
                            pass
                    if is_latest:
                        self.mod_status.config(text="✅ MegabonkMP mod installed (latest)")
                    else:
                        self.mod_status.config(text="⚠️ MegabonkMP mod installed (outdated)")
                    self.mod_installed.set(True)
                    self.log("MegabonkMP mod found")
                else:
                    self.mod_status.config(text="❌ MegabonkMP mod not installed")
                    self.mod_installed.set(False)
    def install_bepinex(self):
        """Download and install BepInEx"""
        game_path = self.game_path_var.get()
        
        if not game_path or not os.path.exists(game_path):
            messagebox.showerror("Error", "Please set a valid game path first.")
            return
        
        if not messagebox.askyesno("Install BepInEx", 
                                    f"This will install BepInEx {BEPINEX_VERSION} to:\n{game_path}\n\nContinue?"):
            return
        
        self.status_var.set("Downloading BepInEx...")
        self.log(f"Downloading BepInEx from {BEPINEX_URL}")
        
        def download_and_install():
            try:
                # Download
                zip_path = os.path.join(game_path, "bepinex_temp.zip")
                urllib.request.urlretrieve(BEPINEX_URL, zip_path)
                self.log("Download complete, extracting...")
                
                # Extract
                with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                    zip_ref.extractall(game_path)
                
                # Cleanup
                os.remove(zip_path)
                
                self.log("BepInEx installed successfully!")
                self.root.after(0, lambda: self.status_var.set("BepInEx installed!"))
                self.root.after(0, self.check_installation_status)
                self.root.after(0, lambda: messagebox.showinfo("Success", 
                    "BepInEx installed!\n\nRun the game once to generate interop assemblies."))
                
            except Exception as e:
                self.log(f"Failed to install BepInEx: {e}", "ERROR")
                self.root.after(0, lambda: self.status_var.set("Installation failed"))
                self.root.after(0, lambda: messagebox.showerror("Error", f"Failed to install BepInEx:\n{e}"))
        
        threading.Thread(target=download_and_install, daemon=True).start()
    
    def get_mod_source_dir(self):
        """Get the mod source directory, downloading from GitHub if needed"""
        launcher_dir = os.path.dirname(os.path.abspath(__file__))
        
        # Check for local source first (development mode)
        local_source = os.path.join(launcher_dir, "..", "src")
        if os.path.exists(os.path.join(local_source, "MegabonkMP.csproj")):
            return local_source
        
        # Use downloaded source in user's app data
        if sys.platform == "win32":
            app_data = os.environ.get("LOCALAPPDATA", os.path.expanduser("~"))
        else:
            app_data = os.path.expanduser("~/.local/share")
        
        source_dir = os.path.join(app_data, "MegabonkMP", "src")
        return source_dir
    
    def download_source(self):
        """Download latest source files from GitHub"""
        self.status_var.set("Downloading latest source...")
        self.log(f"Downloading source from {GITHUB_SOURCE_URL}")
        
        def do_download():
            try:
                # Setup directories
                if sys.platform == "win32":
                    app_data = os.environ.get("LOCALAPPDATA", os.path.expanduser("~"))
                else:
                    app_data = os.path.expanduser("~/.local/share")
                
                mod_dir = os.path.join(app_data, "MegabonkMP")
                os.makedirs(mod_dir, exist_ok=True)
                
                zip_path = os.path.join(mod_dir, "source_temp.zip")
                
                # Download
                urllib.request.urlretrieve(GITHUB_SOURCE_URL, zip_path)
                self.log("Download complete, extracting...")
                
                # Extract
                with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                    zip_ref.extractall(mod_dir)
                
                # The zip extracts to test123-main/megabonk-mp-mod/src
                # Move to the right location
                extracted_dir = os.path.join(mod_dir, "test123-main", MOD_SOURCE_FOLDER, "src")
                target_dir = os.path.join(mod_dir, "src")
                
                # Remove old source if exists
                if os.path.exists(target_dir):
                    shutil.rmtree(target_dir)
                
                if os.path.exists(extracted_dir):
                    shutil.move(extracted_dir, target_dir)
                    self.log(f"Source files installed to {target_dir}")
                else:
                    raise Exception(f"Source folder not found in downloaded archive")
                
                # Cleanup
                os.remove(zip_path)
                extracted_root = os.path.join(mod_dir, "test123-main")
                if os.path.exists(extracted_root):
                    shutil.rmtree(extracted_root)
                
                self.root.after(0, lambda: self.status_var.set("Source downloaded!"))
                self.root.after(0, lambda: messagebox.showinfo("Success", 
                    f"Latest source files downloaded!\n\nLocation: {target_dir}\n\n"
                    "You can now click 'Build Mod' to compile."))
                return True
                
            except Exception as e:
                self.log(f"Failed to download source: {e}", "ERROR")
                self.root.after(0, lambda: self.status_var.set("Download failed"))
                self.root.after(0, lambda: messagebox.showerror("Error", f"Failed to download source:\n{e}"))
                return False
        
        threading.Thread(target=do_download, daemon=True).start()
    
    def build_mod(self, after_build=None):
        """Build the mod from source. Calls after_build() on success if provided."""
        game_path = self.game_path_var.get()
        
        if not self.bepinex_installed.get():
            messagebox.showerror("Error", "Please install BepInEx first and run the game once\n"
                                          "to generate the required interop assemblies.")
            return
        
        # Check if dotnet is available
        try:
            result = subprocess.run(["dotnet", "--version"], capture_output=True, text=True)
            if result.returncode != 0:
                raise FileNotFoundError()
            self.log(f"Found .NET SDK: {result.stdout.strip()}")
        except FileNotFoundError:
            self.log(".NET SDK not found", "ERROR")
            if messagebox.askyesno("Install .NET SDK", 
                                    ".NET SDK is required to build the mod.\n\n"
                                    "Would you like to open the download page?"):
                webbrowser.open("https://dotnet.microsoft.com/download/dotnet/6.0")
            return
        
        # Get mod source directory
        mod_source = self.get_mod_source_dir()
        csproj_path = os.path.join(mod_source, "MegabonkMP.csproj")
        
        if not os.path.exists(csproj_path):
            self.log(f"Project file not found: {csproj_path}", "WARNING")
            if messagebox.askyesno("Download Source", 
                                    "Mod source files not found.\n\n"
                                    "Would you like to download the latest source from GitHub?"):
                self.download_source()
            return
        
        self.status_var.set("Building mod...")
        self.log("Starting mod build...")
        
        def do_build():
            try:
                # Set environment variable for game path
                env = os.environ.copy()
                env["MEGABONK_PATH"] = game_path
                
                # Run dotnet build
                self.log(f"Running: dotnet build -c Release")
                result = subprocess.run(
                    ["dotnet", "build", "-c", "Release", csproj_path],
                    capture_output=True,
                    text=True,
                    cwd=mod_source,
                    env=env
                )
                
                # Log output
                if result.stdout:
                    for line in result.stdout.strip().split('\n'):
                        if line.strip():
                            self.log(line)
                
                if result.returncode != 0:
                    if result.stderr:
                        for line in result.stderr.strip().split('\n'):
                            if line.strip():
                                self.log(line, "ERROR")
                    raise Exception("Build failed. Check the logs for details.")
                
                # Find the built DLL
                # When MEGABONK_PATH is set, output goes directly to BepInEx plugins
                # When not set, it goes to bin\Release\ (no net6.0 subfolder)
                dll_paths = [
                    os.path.join(game_path, "BepInEx", "plugins", "MegabonkMP", "MegabonkMP.dll"),
                    os.path.join(mod_source, "bin", "Release", "MegabonkMP.dll"),
                    os.path.join(mod_source, "bin", "Release", "net6.0", "MegabonkMP.dll"),
                ]
                
                dll_found = None
                self.log(f"Looking for DLL in: {dll_paths}")
                for dll_path in dll_paths:
                    self.log(f"Checking: {dll_path} - Exists: {os.path.exists(dll_path)}")
                    if os.path.exists(dll_path):
                        dll_found = dll_path
                        break
                
                if dll_found:
                    self.log(f"Build successful! DLL at: {dll_found}")
                    self.root.after(0, lambda: self.status_var.set("Build successful!"))
                    self.root.after(0, lambda: messagebox.showinfo("Success", 
                        f"Mod built successfully!\n\nDLL: {dll_found}\n\n"
                        "Click 'Install/Update Mod' to copy it to BepInEx plugins."))
                    if after_build:
                        self.root.after(0, after_build)
                else:
                    self.log("Build completed but DLL not found", "WARNING")
                    self.root.after(0, lambda: self.status_var.set("Build complete"))
                
            except Exception as e:
                self.log(f"Build failed: {e}", "ERROR")
                self.root.after(0, lambda: self.status_var.set("Build failed"))
                self.root.after(0, lambda: messagebox.showerror("Build Failed", 
                    f"Failed to build mod:\n{e}\n\nCheck the Logs tab for details."))
        
        threading.Thread(target=do_build, daemon=True).start()
    
    def build_and_install_mod(self):
        """Build the mod and install it in one go."""
        def after_build():
            self.install_mod()
        self.build_mod(after_build=after_build)
    
    def build_and_install_mod(self):
        """Build the mod and install it in one go."""
        def after_build():
            self.install_mod()
        self.build_mod(after_build=after_build)

    def build_mod(self, after_build=None):
        """Build the mod from source. Calls after_build() on success if provided."""
        game_path = self.game_path_var.get()
        
        if not self.bepinex_installed.get():
            messagebox.showerror("Error", "Please install BepInEx first.")
            return
        
        def do_build():
        mod_source = self.get_mod_source_dir()
        
        # Mod destination
        mod_dest = os.path.join(game_path, "BepInEx", "plugins", "MegabonkMP")
        
        try:
            os.makedirs(mod_dest, exist_ok=True)
            
                    if after_build:
                        self.root.after(0, after_build)
            # First check if DLL was already built to the destination
            dest_dll = os.path.join(mod_dest, "MegabonkMP.dll")
            if os.path.exists(dest_dll):
                self.log(f"Mod DLL already present at destination: {dest_dll}")
                messagebox.showinfo("Success", f"Mod is already installed!\n\n{dest_dll}")
                self.check_installation_status()
                return
            
            # Look for compiled DLL in build output locations
            dll_paths = [
                os.path.join(mod_source, "bin", "Release", "MegabonkMP.dll"),
                os.path.join(mod_source, "bin", "Release", "net6.0", "MegabonkMP.dll"),
                os.path.join(mod_source, "bin", "Debug", "MegabonkMP.dll"),
                os.path.join(mod_source, "bin", "Debug", "net6.0", "MegabonkMP.dll"),
            ]
            
            dll_found = None
            for dll_path in dll_paths:
                if os.path.exists(dll_path):
                    dll_found = dll_path
                    break
            
            if dll_found:
                dest_dll = os.path.join(mod_dest, "MegabonkMP.dll")
                shutil.copy2(dll_found, dest_dll)
                self.log(f"Mod DLL copied from {dll_found} to {dest_dll}")
                messagebox.showinfo("Success", f"Mod installed successfully!\n\n{dest_dll}")
            else:
                self.log("Mod DLL not found. Please build the mod first.", "WARNING")
                if messagebox.askyesno("Build Required", 
                    "Mod DLL not found.\n\nWould you like to build the mod now?"):
                    self.build_mod()
                return
            
            self.check_installation_status()
            
        except Exception as e:
            self.log(f"Failed to install mod: {e}", "ERROR")
            messagebox.showerror("Error", f"Failed to install mod:\n{e}")
    
    def apply_server_settings(self):
        """Apply and save server settings to mod config"""
        game_path = self.game_path_var.get()
        
        if not game_path:
            messagebox.showerror("Error", "Game path not set.")
            return
        
        # Save launcher config
        self.save_config()
        
        # Write to BepInEx config
        config_path = os.path.join(game_path, "BepInEx", "config", "com.megabonk.multiplayer.cfg")
        
        try:
            os.makedirs(os.path.dirname(config_path), exist_ok=True)
            
            config_content = f"""## Settings file for MegabonkMP
## Generated by {APP_NAME}

[Network]

## IP address to connect to or host on
ServerAddress = {self.server_address_var.get()}

## Port for multiplayer connections
ServerPort = {self.server_port_var.get()}

## Maximum players in a session
MaxPlayers = {self.max_players_var.get()}

[Gameplay]

## Allow players to damage each other
FriendlyFire = {str(self.friendly_fire_var.get()).lower()}

## Share loot drops among all players
SharedLoot = {str(self.shared_loot_var.get()).lower()}

## XP multiplier for multiplayer
XpMultiplier = {self.xp_multiplier_var.get()}

[UI]

## Display nameplates above other players
ShowPlayerNameplates = {str(self.show_nameplates_var.get()).lower()}

## Display network statistics overlay
ShowNetworkStats = {str(self.show_network_stats_var.get()).lower()}

[Debug]

## Enable debug logging
DebugMode = {str(self.debug_mode_var.get()).lower()}
"""
            
            with open(config_path, 'w') as f:
                f.write(config_content)
            
            self.log(f"Config written to {config_path}")
            messagebox.showinfo("Success", "Settings saved!")
            
        except Exception as e:
            self.log(f"Failed to write config: {e}", "ERROR")
            messagebox.showerror("Error", f"Failed to save config:\n{e}")
    
    def launch_game(self):
        """Launch the game"""
        game_path = self.game_path_var.get()
        exe_path = os.path.join(game_path, "Megabonk.exe")
        
        if not os.path.exists(exe_path):
            messagebox.showerror("Error", f"Game executable not found:\n{exe_path}")
            return
        
        # Save settings before launch
        self.save_config()
        self.apply_server_settings()
        
        self.log(f"Launching game: {exe_path}")
        self.status_var.set("Launching game...")
        
        try:
            subprocess.Popen([exe_path], cwd=game_path)
            self.log("Game launched successfully")
            self.status_var.set("Game running")
        except Exception as e:
            self.log(f"Failed to launch game: {e}", "ERROR")
            messagebox.showerror("Error", f"Failed to launch game:\n{e}")
    
    def detect_lan_ip(self):
        """Try to detect LAN IP address"""
        import socket
        try:
            s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            s.connect(("8.8.8.8", 80))
            ip = s.getsockname()[0]
            s.close()
            self.server_address_var.set(ip)
            self.log(f"Detected LAN IP: {ip}")
        except Exception:
            self.log("Could not detect LAN IP", "WARNING")
    
    def open_game_folder(self):
        """Open game folder in file explorer"""
        game_path = self.game_path_var.get()
        if game_path and os.path.exists(game_path):
            webbrowser.open(game_path)
        else:
            messagebox.showerror("Error", "Game path not set or invalid.")
    
    def open_config_folder(self):
        """Open BepInEx config folder"""
        game_path = self.game_path_var.get()
        config_path = os.path.join(game_path, "BepInEx", "config")
        if os.path.exists(config_path):
            webbrowser.open(config_path)
        else:
            messagebox.showerror("Error", "Config folder not found. Run the game once first.")
    
    def open_bepinex_logs(self):
        """Open BepInEx log file"""
        game_path = self.game_path_var.get()
        log_path = os.path.join(game_path, "BepInEx", "LogOutput.log")
        if os.path.exists(log_path):
            webbrowser.open(log_path)
        else:
            messagebox.showerror("Error", "Log file not found. Run the game once first.")
    
    def clear_log(self):
        """Clear the log display"""
        self.log_text.config(state=tk.NORMAL)
        self.log_text.delete(1.0, tk.END)
        self.log_text.config(state=tk.DISABLED)
        self.log_buffer.clear()
        self.log("Log cleared")
    
    def copy_log(self):
        """Copy log to clipboard"""
        self.root.clipboard_clear()
        self.root.clipboard_append(self.log_text.get(1.0, tk.END))
        self.log("Log copied to clipboard")
    
    def save_log(self):
        """Save log to file"""
        file_path = filedialog.asksaveasfilename(
            defaultextension=".txt",
            filetypes=[("Text files", "*.txt"), ("Log files", "*.log")],
            initialfile=f"megabonk_mp_log_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"
        )
        if file_path:
            with open(file_path, 'w') as f:
                f.write(self.log_text.get(1.0, tk.END))
            self.log(f"Log saved to {file_path}")
    
    def refresh_log(self):
        """Refresh log from BepInEx"""
        game_path = self.game_path_var.get()
        log_path = os.path.join(game_path, "BepInEx", "LogOutput.log")
        
        if os.path.exists(log_path):
            try:
                with open(log_path, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                
                self.log_text.config(state=tk.NORMAL)
                self.log_text.delete(1.0, tk.END)
                self.log_text.insert(tk.END, "=== BepInEx Log ===\n\n")
                self.log_text.insert(tk.END, content)
                self.log_text.see(tk.END)
                self.log_text.config(state=tk.DISABLED)
                
            except Exception as e:
                self.log(f"Failed to read log: {e}", "ERROR")


def main():
    root = tk.Tk()
    
    # Try to set theme
    try:
        root.tk.call('source', 'azure.tcl')
        root.tk.call('set_theme', 'dark')
    except:
        pass  # Theme not available, use default
    
    app = LauncherApp(root)
    
    # Handle window close
    def on_closing():
        app.save_config()
        root.destroy()
    
    root.protocol("WM_DELETE_WINDOW", on_closing)
    root.mainloop()


if __name__ == "__main__":
    main()

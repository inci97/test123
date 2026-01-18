#!/bin/bash
# Megabonk MP Launcher - Linux/Mac Shell Script

echo "========================================"
echo "  Megabonk Multiplayer Mod Launcher"
echo "========================================"
echo

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check for Python
if command -v python3 &> /dev/null; then
    echo "Found Python3, launching..."
    python3 "$SCRIPT_DIR/launcher.py"
    exit 0
fi

if command -v python &> /dev/null; then
    # Check Python version
    PYTHON_VERSION=$(python -c 'import sys; print(sys.version_info[0])')
    if [ "$PYTHON_VERSION" -ge 3 ]; then
        echo "Found Python, launching..."
        python "$SCRIPT_DIR/launcher.py"
        exit 0
    fi
fi

echo
echo "ERROR: Python 3 not found!"
echo
echo "Please install Python 3.8+:"
echo "  Ubuntu/Debian: sudo apt install python3 python3-tk"
echo "  Fedora: sudo dnf install python3 python3-tkinter"
echo "  Mac: brew install python3 python-tk"
echo

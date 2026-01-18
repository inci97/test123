@echo off
REM Megabonk MP Launcher - Windows Batch File
REM Checks for Python and launches the GUI

echo ========================================
echo   Megabonk Multiplayer Mod Launcher
echo ========================================
echo.

REM Check if Python is available
where python >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Found Python, launching...
    python "%~dp0launcher.py"
    exit /b
)

where python3 >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Found Python3, launching...
    python3 "%~dp0launcher.py"
    exit /b
)

where py >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Found Py launcher, launching...
    py "%~dp0launcher.py"
    exit /b
)

echo.
echo ERROR: Python not found!
echo.
echo Please install Python 3.8+ from:
echo https://www.python.org/downloads/
echo.
echo Make sure to check "Add Python to PATH" during installation.
echo.
pause

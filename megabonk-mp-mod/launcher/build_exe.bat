@echo off
REM Build script for creating MegabonkMP_Launcher.exe
REM Requires Python 3.8+ with pip

echo ========================================
echo   Building Megabonk MP Launcher EXE
echo ========================================
echo.

REM Check Python
where python >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Python not found!
    echo Please install Python 3.8+ from https://www.python.org/downloads/
    pause
    exit /b 1
)

echo Checking Python version...
python --version

echo.
echo Installing/upgrading PyInstaller...
python -m pip install --upgrade pyinstaller

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to install PyInstaller
    pause
    exit /b 1
)

echo.
echo Building executable...
python -m PyInstaller --clean --noconfirm launcher.spec

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo   Build Complete!
echo ========================================
echo.
echo Executable created at:
echo   dist\MegabonkMP_Launcher.exe
echo.
echo You can distribute this single file.
echo.

REM Copy to release folder
if not exist "..\release" mkdir "..\release"
copy /Y "dist\MegabonkMP_Launcher.exe" "..\release\"
echo Copied to: ..\release\MegabonkMP_Launcher.exe

echo.
pause

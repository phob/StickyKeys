# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StickyKeysAgent is a .NET 9.0 Windows application that programmatically manages Windows Sticky Keys settings and persists them across system reboots. The application runs as a system tray agent with minimal UI.

## Build and Run Commands

### Build
```bash
dotnet build StickyKeysService.sln
```

### Publish (creates single-file executable)
```bash
dotnet publish StickyKeysService.sln
```
Published output goes to `publish/StickyKeysAgent/`

### Debug/Run
```bash
dotnet run --project StickyKeysService/StickyKeysAgent.csproj
```

## Architecture

### Application Structure

The application follows a three-component architecture:

1. **Program.cs** - Entry point and UI management
   - Configures Serilog logging to `Logs/` directory
   - Builds configuration from `config.json`
   - Sets up dependency injection
   - Manages system tray icon and context menu
   - Handles first-run setup dialog
   - Manages autostart registry setting in `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`

2. **Worker.cs** - Background monitoring service
   - Runs continuously with 1-minute polling interval
   - Uses mutex to prevent multiple instances
   - Monitors configuration changes with hot-reload support
   - Compares current Windows Sticky Keys settings against desired configuration
   - Only applies changes when settings differ (using flags mask comparison)
   - Uses P/Invoke to call Windows `SystemParametersInfo` API

3. **ConfigSettings.cs** - Configuration model
   - Maps to `config.json` settings
   - Controls all Sticky Keys flags (StickyKeysOn, HotKeyActive, ConfirmHotKey, etc.)
   - Includes `Autostart` and `FirstRun` properties

### Configuration System

- Primary config: `StickyKeysService/config.json` (copied to output on build)
- Configuration supports hot-reload - changes are detected and applied within 1 minute
- Settings are persisted back to `config.json` when changed via UI
- First run prompts user about autostart preference

### Windows API Integration

The Worker class interacts with Windows accessibility features via P/Invoke:
- `SPI_GETSTICKYKEYS` (0x003A) - Retrieves current settings
- `SPI_SETSTICKYKEYS` (0x003B) - Applies new settings
- Sticky Keys flags defined as constants (SKF_STICKYKEYSON, SKF_HOTKEYACTIVE, etc.)
- Uses `STICKYKEYS` struct matching Windows API requirements

### Key Implementation Details

- Single-file executable configured in `.csproj` with `PublishSingleFile`
- Targets .NET 9.0 with Windows Forms support
- Logs to monthly rolling files via Serilog
- Uses `NotifyIcon` for system tray presence
- Registry modifications require appropriate user permissions

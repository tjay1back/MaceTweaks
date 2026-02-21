# MaceTweaks

My first app! A macro tool for gaming built with C# and WPF.

## Features

- **Stun Slam** - Configurable key sequence with mouse trigger
- **Breach Swap** - Quick weapon swap macro
- **Attribute Swap** - Attribute switching macro
- Customizable hotkeys and delays
- Auto-updater built-in

## Installation

Download the latest `MaceTweaks_Setup.exe` from [Releases](https://github.com/tjay1back/MaceTweaks/releases).

> Windows SmartScreen may warn you because the app is unsigned. Click "More info" > "Run anyway". The source code is open - verify it yourself.

## Building from Source

Requires .NET 8.0 SDK.

```bash
dotnet build
dotnet run
```

To publish:
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Made by

**PeakJayX** - [guns.lol/jay_back](https://guns.lol/jay_back) | [GitHub](https://github.com/tjay1back) | [Discord](https://discord.gg/XPS2bJVyn9)

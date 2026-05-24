# SafeCheck — Install Guide

## Build & Run (Developer Machine)

```
dotnet build SafeCheck.sln
dotnet run --project SafeCheck.App
```

Or open `SafeCheck.sln` in Visual Studio and press **F5**.

## Publish for Another Computer

```
dotnet publish SafeCheck.App -c Release -r win-x64 --self-contained -o ./publish
```

This bundles the .NET runtime so the target PC doesn't need anything installed.

## Deploy to Target PC

1. Copy the entire `publish` folder to the target computer (USB drive, network share, etc.).
2. Double-click `SafeCheck.App.exe` to run.

No installer or .NET SDK needed on the target machine.

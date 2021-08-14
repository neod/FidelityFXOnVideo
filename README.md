# FidelityFXOnVideo
FidelityFX usable on video

This software is write in .NET 5 and usable in CLI

Work only on windows due to Direct3D 11 dependence of FidelityFX-CLI

## Binary

Binary for 64-bit Windows is available here [Releases](https://github.com/neod/FidelityFXOnVideo/releases)

## Dependency

you need to download `.NET 5`, `ffmpeg` and `FidelityFX-CLI` and add ffmpeg to the PATH 

- [.NET 5](https://dotnet.microsoft.com/)
- [ffmpeg](https://www.ffmpeg.org/)
- [FidelityFX-CLI](https://github.com/GPUOpen-Effects/FidelityFX-CLI)

Nuget package
- [FFMpegCore](https://www.nuget.org/packages/FFMpegCore/)

## How to build

```donet build```

## How to use

In the FidelityFXOnVideo sub directory

```dotnet run [FidelityFX_CLI.exe path] [video path] [newWidth] [newHeight]```
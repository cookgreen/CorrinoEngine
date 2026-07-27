# CorrinoEngine 
CorrinoEngine is an open-source, custom game engine written from scratch in C#. Its primary goal and ultimate milestone is to completely recreate and run the classic 2001 Westwood Studios RTS, Emperor: Battle for Dune.
  
The project is named "Corrino"—derived from the Emperor Corrino character in *Emperor: Battle for Dune*—and is thus called "CorrinoEngine." This naming choice was made to avoid the common "Open+XXX" convention, as a project named "OpenDune" already exists.
  
The project is built using pure C# and OpenTK; C# handles the logic layer, while OpenTK manages 3D rendering. It also includes logic for parsing Xbf and RFH/RFD file formats.
  
Why CorrinoEngine? As many of you know, playing Emperor: Battle for Dune on modern operating systems (Windows 10/11) has become incredibly difficult due to outdated DirectX dependencies, deprecated DRM systems, and overall compatibility issues. CorrinoEngine aims to solve this by providing a native, cross-platform capable, modern foundation to play the original game natively—much like what OpenRA has done for the 2D C&C games.
  
<img src="https://user-images.githubusercontent.com/21171007/140613614-cd2c73c0-3caa-4bcb-a744-6b3459d2f74b.png" width="100"/>

## Usage
Go to your Emperor: Battle for Dune and copy all RFD/RFH files under `DATA` into the `bin/net6.0-windows/mods/ebfd/Assets/`
  
`LShift + M` - Model browser
  
`LShift + A` - Asset browser
  
`LShift + I` - Import Maps

## Enviroment  
.Net 6.0  
OpenTK  

## Credit  
ebfd-re - IceReaper (Andre Mohren)  

## License
GNU GPLv3  

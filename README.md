# <img align="center" src="./PackageIcon.png">  Javi.MediaInfo

This [.NET standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) package 
provides a wrapper for the [MediaInfo](https://mediaarea.net/en/MediaInfo) library of functions.<br>
MediaInfo is a convenient unified display of the most relevant technical and tag data for video and audio files.<br>
With this package, using the MediaInfo library from your application and retrieving this data for video and audio files 
is as simple as making a method call and then examining the properties of this class.<br>

- [Features](#features)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Features
- Wraps the MediaInfo.dll 
- Provides properties for almost all information available using the MediaInfo.dll
- Targets .NET Standard 2.0
    
## Getting Started

- Install package using nuget

Install Javi.MediaInfo from NuGet using the Package Manager Console with the following command

    PM> Install-Package Javi.MediaInfo

Alternatively search on [NuGet Javi.MediaInfo](https://www.nuget.org/packages/Javi.MediaInfo)

- Download a copy of MediaInfo.dll

Since this package is only a wrapper for the MediaInfo dll, a copy of the dll must be available. Official releases can
be downloaded using links from the [MediaInfo download site](https://mediaarea.net/en/MediaInfo/Download).<br>
The MediaInfo.dll is available in 32 and 64 bit versions, choose the correct version for your application and operating system.<br>
This package uses dynamic DLL loading, so applications using this package still have the freedom to choose their target as appropriate.

## Usage

Add to usings:

    using Javi.MediaInfo

Instantiate an object of class MediaInfo, providing the full path to the local copy of the MediaInfo.dll.

    MediaInfo MediaInfo = new MediaInfo(MediaInfoDLLFullName);

Call the method that calls into the MediaInfo.dll to update all properties.

    MediaInfo.ReadMediaInformation(@"Sample.mp4");

Retrieve technical and tag data from the video or audio file:

    var containerFormat = MediaInfo.General.Format;

### Demo application

A C# dotnet core console demo application is available which shows the usage of the package. Code from this demo should not be used in production code,
the code is merely to demonstrate the usage of this package.
    
## License

This project is licensed under the [MIT License](https://github.com/jacovis/Javi.MediaInfo/blob/master/LICENSE.md).

## Acknowledgments

<br>
Sample video courtesy of https://sample-videos.com/

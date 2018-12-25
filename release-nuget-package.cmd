rem https://docs.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package-using-visual-studio

rd /s /q .\Javi.MediaInfo\bin\Release
dotnet build -c Release .\Javi.FFmpeg\Javi.MediaInfo.csproj
nuget push .\Javi.MediaInfo\bin\Release\*.nupkg -Source https://api.nuget.org/v3/index.json
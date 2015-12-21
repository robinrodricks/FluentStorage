msbuild src/storage.sln /p:Configuration=Release
nuget pack storage.nuspec
nuget pack storage.net45.azure.nuspec
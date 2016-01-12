msbuild src/storage.sln /p:Configuration=Release
del *.nupkg
nuget pack storage.nuspec
nuget pack storage.net45.azure.nuspec
nuget pack storage.net45.aws.nuspec
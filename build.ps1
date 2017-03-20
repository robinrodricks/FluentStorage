param(
   [switch]
   $Publish,

   [string]
   $NuGetApiKey
)

$Version = "3.5.0-alpha-4"
$SlnPath = "src\storage.sln"
$IconUrl = "http://i.isolineltd.com/nuget/storage.png"
$Copyright = "Copyright (c) 2015-2017 by Ivan Gavryliuk"

function Set-VstsBuildNumber($BuildNumber)
{
   Write-Verbose -Verbose "##vso[build.updatebuildnumber]$BuildNumber"
}

function Update-ProjectVersion([string]$Path, [string]$Version)
{
   $xml = [xml](Get-Content $Path)

   if($xml.Project.PropertyGroup.Count -eq $null)
   {
      $pg = $xml.Project.PropertyGroup
   }
   else
   {
      $pg = $xml.Project.PropertyGroup[0]
   }

   $pg.VersionPrefix = $Version
   $pg.PackageIconUrl = $IconUrl
   $pg.Copyright = $Copyright
   


   $xml.Save($Path)
}

function Exec($Command)
{
   Invoke-Expression $Command
   if($LASTEXITCODE -ne 0)
   {
      Write-Error "command failed (error code: $LASTEXITCODE)"
      exit 1
   }
}

# General validation
if($Publish -and (-not $NuGetApiKey))
{
   Write-Error "Please specify nuget key to publish"
   exit 1
}

# Update versioning information
Get-ChildItem *.csproj -Recurse | Where-Object {-not($_.Name -like "*test*")} | % {
   $path = $_.FullName
   Write-Host "setting version of $path to $Version"
   Update-ProjectVersion $path $Version
}
Set-VstsBuildNumber $Version

# Restore packages
Exec "dotnet restore $SlnPath"

# Build solution
Get-ChildItem *.nupkg -Recurse | Remove-Item
Exec "dotnet build $SlnPath -c release"

# Run the tests
#Exec "dotnet test test\LogMagic.Test\LogMagic.Test.csproj"

# publish the nugets
if($Publish.IsPresent)
{
   Write-Host "publishing nugets..."

   Get-ChildItem *.nupkg -Recurse | % {
      $path = $_.FullName

      Exec "nuget push $path -Source https://www.nuget.org/api/v2/package -ApiKey $NuGetApiKey"
   }
}

Write-Host "build succeeded."
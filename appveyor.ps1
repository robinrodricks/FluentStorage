$BuildNo = $env:APPVEYOR_BUILD_NUMBER
$Major = 5
$Minor = 9
$Patch = 2
$IsPrerelease = $true

# latest released version: 5.9.2

if($BuildNo -eq $null)
{
   $BuildNo = "1"
}

$VDisplay = Get-DisplayVersion
Invoke-Expression "appveyor UpdateBuild -Version $VDisplay"

$vt = @{
   "Storage.Net.Microsoft.ServiceFabric.csproj" = (5, 6, $BuildNo);
}

$Copyright = "Copyright (c) 2015-2017 by Ivan Gavryliuk"
$PackageIconUrl = "http://i.isolineltd.com/nuget/storage.png"
$PackageProjectUrl = "https://github.com/aloneguid/storage"
$RepositoryUrl = "https://github.com/aloneguid/storage"
$Authors = "Ivan Gavryliuk (@aloneguid)"
$PackageLicenseUrl = "https://github.com/aloneguid/storage/blob/master/LICENSE"
$RepositoryType = "GitHub"

$SlnPath = "src\storage.sln"

function Get-DisplayVersion()
{
   $v = "$Major.$Minor.$Patch"
   
   if($IsPrerelease)
   {
      $v = "$v-ci-$BuildNo"
   }

   $v
}

function Update-ProjectVersion($File)
{
   Write-Host "updating $File ..."

   $over = $vt.($File.Name)
   if($over -eq $null) {
      $thisMajor = $Major
      $thisMinor = $Minor
      $thisPatch = $Patch
   } else {
      $thisMajor = $over[0]
      $thisMinor = $over[1]
      $thisPatch = $over[2]
   }

   $xml = [xml](Get-Content $File.FullName)

   if($xml.Project.PropertyGroup.Count -eq $null)
   {
      $pg = $xml.Project.PropertyGroup
   }
   else
   {
      $pg = $xml.Project.PropertyGroup[0]
   }

   if($IsPrerelease) {
      $suffix = "-ci-" + $BuildNo.PadLeft(5, '0')
   } else {
      $suffix = ""
   }

   
   [string] $fv = "{0}.{1}.{2}.{3}" -f $thisMajor, $thisMinor, $thisPatch, $BuildNo
   [string] $av = "{0}.0.0.0" -f $thisMajor
   [string] $pv = "{0}.{1}.{2}{3}" -f $thisMajor, $thisMinor, $thisPatch, $suffix

   $pg.Version = $pv
   $pg.FileVersion = $fv
   $pg.AssemblyVersion = $av

   Write-Host "$($File.Name) => fv: $fv, av: $av, pkg: $pv"

   $pg.Copyright = $Copyright
   $pg.PackageIconUrl = $PackageIconUrl
   $pg.PackageProjectUrl = $PackageProjectUrl
   $pg.RepositoryUrl = $RepositoryUrl
   $pg.Authors = $Authors
   $pg.PackageLicenseUrl = $PackageLicenseUrl
   $pg.RepositoryType = $RepositoryType

   $xml.Save($File.FullName)
}

function Exec($Command, [switch]$ContinueOnError)
{
   Invoke-Expression $Command
   if($LASTEXITCODE -ne 0)
   {
      Write-Error "command failed (error code: $LASTEXITCODE)"

      if(-not $ContinueOnError.IsPresent)
      {
         exit 1
      }
   }
}

# Update versioning information
Get-ChildItem *.csproj -Recurse | Where-Object {-not(($_.Name -like "*test*") -or ($_.Name -like "*Stateful*") -or ($_.Name -like "*Esent*") -or ($_.Name -like "*Monitor*")) } | % {
   Update-ProjectVersion $_
}

# Restore packages
Exec "dotnet restore $SlnPath"
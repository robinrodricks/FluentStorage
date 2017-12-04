$gv = $env:APPVEYOR_BUILD_VERSION
if($gv -eq $null)
{
   $gv = "5.0.0"
}

$bn = $env:APPVEYOR_BUILD_NUMBER
if($bn -eq $null)
{
   $bn = "0"
}

$vt = @{
   "Storage.Net.csproj" = "5.4.0";

   "Storage.Net.Microsoft.ServiceFabric.csproj" = "2.6.204.80";

   "Storage.Net.Amazon.Aws.csproj" = "5.4.0";
   "Storage.Net.ZipFile.csproj" = "5.4.0";
   "Storage.Net.Microsoft.Azure.DataLake.Store.csproj" = "5.4.0";
   "Storage.Net.Microsoft.Azure.EventHub.csproj" = "5.4.0";
   "Storage.Net.Microsoft.Azure.KeyVault.csproj" = "5.4.0";
   "Storage.Net.Microsoft.Azure.ServiceBus.csproj" = "5.4.0";
   "Storage.Net.Microsoft.Azure.Storage.csproj" = "5.4.0";
}

$Copyright = "Copyright (c) 2015-2017 by Ivan Gavryliuk"
$PackageIconUrl = "http://i.isolineltd.com/nuget/storage.png"
$PackageProjectUrl = "https://github.com/aloneguid/storage"
$RepositoryUrl = "https://github.com/aloneguid/storage"
$Authors = "Ivan Gavryliuk (@aloneguid)"
$PackageLicenseUrl = "https://github.com/aloneguid/storage/blob/master/LICENSE"
$RepositoryType = "GitHub"

$SlnPath = "src\storage.sln"

function Update-ProjectVersion($File)
{
   Write-Host "processing $File ..."

   $v = $vt.($File.Name)
   if($v -eq $null) { $v = $gv }

   $xml = [xml](Get-Content $File.FullName)

   if($xml.Project.PropertyGroup.Count -eq $null)
   {
      $pg = $xml.Project.PropertyGroup
   }
   else
   {
      $pg = $xml.Project.PropertyGroup[0]
   }

   $parts = $v -split "\."
   $bv = $parts[2]
   if($bv.Contains("-")) { $bv = $bv.Substring(0, $bv.IndexOf("-"))}
   $fv = "{0}.{1}.{2}.0" -f $parts[0], $parts[1], $bv
   $av = "{0}.0.0.0" -f $parts[0]
   $pv = $v

   #Write-Host "current settings - v: $($pg.Version), fv: $($pg.FileVersion), av: $($pg.AssemblyVersion)"

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
Get-ChildItem *.csproj -Recurse | Where-Object {-not(($_.Name -like "*test*") -or ($_.Name -like "*Stateful*") -or ($_.Name -like "*Esent*")) } | % {
   Update-ProjectVersion $_
}

# Restore packages
Exec "dotnet restore $SlnPath"
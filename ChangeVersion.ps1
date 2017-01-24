param(
   [String]
   $Version
)

Write-Host "version is $Version"

function Update-ProjectJsonVersions([string]$RelPath, [string]$Version, [bool]$UpdatePackageVersion)
{
   $path = "$PSScriptRoot\$RelPath"
   Write-Host "processing $path"
   $json = Get-Content $path | ConvertFrom-Json

   if($UpdatePackageVersion)
   {
      $json.version = $Version

      Write-Host "main package set to $Version"
   }
   
   foreach($other in $args)
   {
      $json.$other = $Version

      Write-Host "set $other to $Version"
   }

   $content = $json | ConvertTo-Json -Depth 100

   Write-Host "result json:"
   Write-Host $content

   $content | Set-Content -Path $path
}

Update-ProjectJsonVersions "src\src\Storage.Net\project.json" $Version $true

<#
$jsonMain = Get-Json "src\Config.Net\project.json"
$jsonAzure = Get-Json "src\Config.Net.Azure\project.json"
$jsonTests = Get-Json "src\Config.Net.Tests\project.json"

$jsonMain.version = $Version

$jsonAzure.version = $Version
$jsonAzure.dependencies."Config.Net" = $Version

$jsonTests.dependencies."Config.Net" = $Version
$jsonTests.dependencies."Config.Net.Azure" = $Version

Set-Json $jsonMain "src\Config.Net\project.json"
Set-Json $jsonAzure "src\Config.Net.Azure\project.json"
Set-Json $jsonTests "src\Config.Net.Tests\project.json"

#>
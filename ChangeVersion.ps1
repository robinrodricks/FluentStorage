param(
   [String]
   $Version
)

Write-Host "version is $Version"

function Update-ProjectJsonVersions([string]$RelPath, [string]$Version, [bool]$UpdatePackageVersion)
{
   $path = "$PSScriptRoot\$RelPath\project.json"
   Write-Host "processing $path"
   $json = Get-Content $path | ConvertFrom-Json

   if($UpdatePackageVersion)
   {
      $json.version = $Version

      Write-Host "main package set to $Version"
   }
   
   foreach($other in $args)
   {
      $json.dependencies.$other = $Version

      Write-Host "set $other to $Version"
   }

   $content = $json | ConvertTo-Json -Depth 100

   Write-Host "result json:"
   Write-Host $content

   $content | Set-Content -Path $path
}

Update-ProjectJsonVersions "src\src\Storage.Net" $Version $true
Update-ProjectJsonVersions "src\src\Storage.Net.Tests" $Version $false "Storage.Net" "Storage.Net.Amazon.Aws" "Storage.Net.Microsoft.Azure" "Storage.Net.Esent"
Update-ProjectJsonVersions "src\src\Storage.Net.Amazon.Aws" $Version $true "Storage.Net"
Update-ProjectJsonVersions "src\src\Storage.Net.Esent" $Version $true "Storage.Net"
Update-ProjectJsonVersions "src\src\Storage.Net.Microsoft.Azure" $Version $true "Storage.Net"
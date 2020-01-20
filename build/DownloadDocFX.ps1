[cmdletbinding()]
param([string]$Version="2.48.1",[string]$Folder)
Add-Type -AssemblyName System.IO.Compression.FileSystem
[Net.ServicePointManager]::SecurityProtocol =[Net.SecurityProtocolType]::Tls12

function DownloadDocFX([string]$version, [string]$folder)
{
   Write-Output "Using folder $folder"
   $path = "$folder\v$version"
   if (!(Test-Path $path)) 
   {
      New-Item -ItemType Directory -Force -Path $path
      Write-Output "Downloading DocFX v$version..."
      Invoke-WebRequest -Uri "https://github.com/dotnet/docfx/releases/download/v$version/docfx.zip" -OutFile "$folder\docfx_v$version.zip"
      [System.IO.Compression.ZipFile]::ExtractToDirectory("$folder\docfx_v$version.zip",$path )
   }
}
DownloadDocFX -version $Version -folder $Folder
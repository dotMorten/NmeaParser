@ECHO OFF

SET DocFXVersion=2.48.1
SET DocFxFolder=%~dp0\..\.tools\docfx

REM Download DocFx
powershell -ExecutionPolicy ByPass -command "%~dp0/DownloadDocFX.ps1" -Version %DocFXVersion% -Folder %DocFxFolder%

REM Merge output (not implemented)
REM %DocFxFolder%\v%DocFXVersion%\docfx.exe merge

REM Generate OMD
dotnet tool install --tool-path .tools/omd dotMorten.OmdGenerator --version 1.2.0
mkdir %~dp0../artifacts/docs/api
.tools\omd\generateomd /source=%~dp0../src/NmeaParser /output=%~dp0../artifacts/docs/api/omd.html /preprocessors=NETSTANDARD1_4;NETSTANDARD

REM Build the output site (HTML) from the generated metadata and input files (uses configuration in docfx.json in this folder)
%DocFxFolder%\v%DocFXVersion%\docfx.exe %~dp0..\docs\docfx.json 

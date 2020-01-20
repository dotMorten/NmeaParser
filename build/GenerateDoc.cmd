@ECHO OFF

SET DocFXVersion=2.48.1
SET DocFxFolder=..\artifacts\toolset

REM Download DocFx
powershell -ExecutionPolicy ByPass -command "./DownloadDocFX.ps1" -Version %DocFXVersion% -Folder %DocFxFolder%

REM Build metadata for all platforms (uses configuration in docfx.json in this folder)
%DocFxFolder%\v%DocFXVersion%\docfx.exe ..\docfx\docfx.json metadata

REM Merge output (not implemented)
REM %DocFxFolder%\v%DocFXVersion%\docfx.exe merge

REM Generate OMD
dotnet tool install --global dotMorten.OmdGenerator
generateomd /source=../src/NmeaParser /output=../artifacts/docs/api/omd.html

REM Build the output site (HTML) from the generated metadata and input files (uses configuration in docfx.json in this folder)
%DocFxFolder%\v%DocFXVersion%\docfx.exe ..\docfx\docfx.json build

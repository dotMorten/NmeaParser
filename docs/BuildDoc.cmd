@ECHO OFF

SET DocFXVersion=2.58
SET DocFxFolder=%~dp0..\.tools\docfx

REM Download DocFx

IF NOT EXIST "%DocFxFolder%\v%DocFXVersion%\docfx.exe" (
   MKDIR "%DocFXFolder%\v%DocFXVersion%"
   powershell -ExecutionPolicy ByPass -command "Invoke-WebRequest -Uri "https://github.com/dotnet/docfx/releases/download/v%DocFXVersion%/docfx.zip" -OutFile '%DocFxFolder%\docfx_v%DocFXVersion%.zip'"
   powershell -ExecutionPolicy ByPass -command "Expand-Archive -LiteralPath '%DocFxFolder%\docfx_v%DocFXVersion%.zip' -DestinationPath '%DocFxFolder%\v%DocFXVersion%'"
   DEL "%DocFxFolder%\docfx_v%DocFXVersion%.zip" /Q
)
IF NOT EXIST "..\.tools\nuget.exe" (
  powershell -ExecutionPolicy ByPass -command "Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile '%~dp0..\.tools\nuget.exe'"
)

REM Generate OMD
dotnet tool install --tool-path %~dp0../.tools/omd dotMorten.OmdGenerator --version 1.2.0
mkdir %~dp0../artifacts/docs/api
%~dp0..\.tools\omd\generateomd /source=%~dp0../src/NmeaParser /output=%~dp0../artifacts/docs/api/omd.html /preprocessors=NETSTANDARD1_4;NETSTANDARD

%~dp0..\.tools\nuget install memberpage -Version 2.58.0 -OutputDirectory %~dp0
REM Build the output site (HTML) from the generated metadata and input files (uses configuration in docfx.json in this folder)
%DocFxFolder%\v%DocFXVersion%\docfx.exe metadata %~dp0\docfx.json

REM Build applies-to version/framework info 
dotnet build AppliesToGenerator\DocFXAppliesToGenerator.csproj
AppliesToGenerator\bin\Debug\netcoreapp3.1\DocFXAppliesToGenerator.exe appliesToList.json

%DocFxFolder%\v%DocFXVersion%\docfx.exe build %~dp0\docfx.json

ECHO Fixing API Reference Links
powershell -ExecutionPolicy ByPass -command "%~dp0FixApiRefLinks.ps1" -Path %~dp0..\artifacts\docs_site\api\
start http://localhost:8080
%DocFxFolder%\v%DocFXVersion%\docfx.exe serve %~dp0..\artifacts\docs_site\

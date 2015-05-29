@echo off

@echo *******************************************
@echo * COPYING BINARIES FOR NUGET              *
@echo *******************************************
REM msbuild ..\..\src\NmeaParser.sln /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.WinDesktop\NmeaParser.WinDesktop.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.WinStore\NmeaParser.WinStore.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.WinPhone\NmeaParser.WinPhone.csproj /t:Rebuild /p:Configuration=Release
xcopy ..\..\src\bin\Release\NmeaParser.WinStore.dll .\NmeaParser\lib\netcore45\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinStore.xml .\NmeaParser\lib\netcore45\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinPhone.dll .\NmeaParser\lib\wpa\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinPhone.xml .\NmeaParser\lib\wpa\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinDesktop.dll .\NmeaParser\lib\net40-client\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinDesktop.xml .\NmeaParser\lib\net40-client\ /Y


@echo *******************************************
@echo * BUILDING NUGET PAKCAGE					*
@echo *******************************************
nuget pack NmeaParser\NmeaParser.nuspec -o .\

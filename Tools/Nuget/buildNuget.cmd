@echo off

@echo *******************************************
@echo * COPYING BINARIES FOR NUGET              *
@echo *******************************************
REM msbuild ..\..\src\NmeaParser.sln /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.WinDesktop\NmeaParser.WinDesktop.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.WinStore\NmeaParser.WinStore.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.WinPhone\NmeaParser.WinPhone.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.UWP\NmeaParser.UWP.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.Android\NmeaParser.Android.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.iOS\NmeaParser.iOS.csproj /t:Rebuild /p:Configuration=Release
msbuild ..\..\src\NmeaParser.NetStandard\NmeaParser.NetStandard.csproj /t:Rebuild /p:Configuration=Release
xcopy ..\..\src\bin\Release\NmeaParser.WinStore.dll .\NmeaParser\lib\netcore45\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinStore.xml .\NmeaParser\lib\netcore45\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinPhone.dll .\NmeaParser\lib\wpa\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinPhone.xml .\NmeaParser\lib\wpa\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinDesktop.dll .\NmeaParser\lib\net40-client\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.WinDesktop.xml .\NmeaParser\lib\net40-client\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.UWP.dll .\NmeaParser\lib\uap10.0\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.UWP.xml .\NmeaParser\lib\uap10.0\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.Android.dll .\NmeaParser\lib\MonoAndroid10\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.Android.xml .\NmeaParser\lib\MonoAndroid10\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.iOS.dll .\NmeaParser\lib\Xamarin.iOS10\ /Y
xcopy ..\..\src\bin\Release\NmeaParser.iOS.xml .\NmeaParser\lib\Xamarin.iOS10\ /Y
xcopy ..\..\src\bin\Release\netstandard1.4\NmeaParser.NetStandard.dll .\NmeaParser\lib\netstandard1.4\ /Y
xcopy ..\..\src\bin\Release\netstandard1.4\NmeaParser.NetStandard.xml .\NmeaParser\lib\netstandard1.4\ /Y


@echo *******************************************
@echo * BUILDING NUGET PAKCAGE					*
@echo *******************************************
nuget pack NmeaParser\NmeaParser.nuspec -o .\

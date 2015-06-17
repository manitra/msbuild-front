@echo off
cd %~dp0\..\src

set msbuild=C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe
set nuget=nuget
set git=git
set version=0.6.1.2
set config=Release

echo Building the package ...
%msbuild% MSBuild.Front.sln /p:Configuration=%config%
if ERRORLEVEL 1 goto error
%nuget% pack MSBuild.Front.Nuget\MSBuild.Front.Nuget.nuspec -Prop Configuration=%config% -Version %version% -OutputDir MSBuild.Front.Nuget\bin
if ERRORLEVEL 1 goto error

echo Pushing the package ...
%git% tag -a nuget-%version% -m "Nuget package version %version%"
if ERRORLEVEL 1 goto error
%nuget% push MSBuild.Front.Nuget\bin\MSBuild.Front.%version%.nupkg
if ERRORLEVEL 1 goto error
%git% push --tags
if ERRORLEVEL 1 goto error

goto end

:error
echo An error occured, deleting the tag in case it has been created
%git% tag -d nuget-%version%

:end
echo Finished

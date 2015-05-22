@echo off
cd %~dp0\..\src

set msbuild=C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe
set nuget=nuget
set version=0.6.1.0
set config=Release

%msbuild% MSBuild.Front.sln /p:Configuration=%config%
%nuget% pack MSBuild.Front.Nuget\MSBuild.Front.Nuget.nuspec -Prop Configuration=%config% -Version %version% -OutputDir MSBuild.Front.Nuget\bin
%nuget% push MSBuild.Front.Nuget\bin\MSBuild.Front.%version%.nupkg
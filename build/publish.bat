@echo off
cd %~dp0\..\src

set msbuild=C:\Windows\Microsoft.Net\Framework64\v4.0.30319\MSBuild.exe
set nuget=nuget
set version=0.5.0.0
set config=Release

%msbuild% MSBuild.Front.sln /p:Configuration=%config%
%nuget% pack MSBuild.Front.Nuget\MSBuild.Front.Nuget.csproj -Prop Configuration=%config% -Version %version% -OutputDir MSBuild.Front.Nuget\bin\%config%
%nuget% push MSBuild.Front.Nuget\bin\%config%\MSBuild.Front.%version%.nupkg
@echo off 
dotnet clean
dotnet build -p:UseAppHost=false
dotnet test .\Minsk.Tests\Minsk.Tests.csproj

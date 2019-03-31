@echo off

dotnet test .\ServForOracle.NetCore.Extensions.Autofac.Tests
coverlet .\ServForOracle.NetCore.Extensions.Autofac.Tests\bin\Debug\netcoreapp2.2\ServForOracle.NetCore.UnitTests.dll --target "dotnet" --targetargs "test .\ServForOracle.NetCore.Extensions.Autofac.Tests --no-build" --include "[ServForOracle.NetCore.Extensions.Autofac]ServForOracle.NetCore.Extensions.Autofac*"
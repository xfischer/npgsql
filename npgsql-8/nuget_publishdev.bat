@echo off
setlocal
:PROMPT
SET /P AREYOUSURE=Are you sure (Y/[N])?
IF /I "%AREYOUSURE%" NEQ "Y" GOTO END

SET version=8.0.5.1
SET efversion=8.0.10.1
SET output_dir=edb_dotnet_nugetpackages

dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.DependencyInjection.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.GeoJSON.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.Json.NET.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.NetTopologySuite.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.NodaTime.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.OpenTelemetry.%version%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.%efversion%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NetTopologySuite.%efversion%.nupkg
dotnet nuget push -s http://localhost:5000/v3/index.json -k NUGET-SERVER-API-KEY %output_dir%/EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NodaTime.%efversion%.nupkg
pause

:END
endlocal

pause

REM CALL "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64
REM CALL "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat"

REM @SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"

cd %SOURCE_PATH%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%

REM echo "-->nuget install VSSDK.Shell.12"
REM nuget install VSSDK.Shell.12
REM nuget install AsyncRewriter -Version 0.6.0 -Output packages
REM nuget install System.Threading.Tasks.Extensions -Version 4.3.0
REM nuget install EntityFramework

echo "nuget restore Npgsql.sln"
cd npgsql-6
REM nuget restore Npgsql.sln

REM echo "msbuild restore"
REM msbuild Npgsql.sln /t:restore /p:Configuration=Release /p:Platform="Any CPU"

REM echo "Mmsbuild build"
REM msbuild Npgsql.sln /t:build /p:Configuration=Release /p:Platform="Any CPU"

echo "dotnet build"
cd %SOURCE_PATH%
cd npgsql-6\src\Npgsql
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1
copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\Microsoft.Bcl.HashCode.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Collections.Immutable.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Text.Encodings.Web.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Collections.Immutable.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Text.Encodings.Web.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error

cd %SOURCE_PATH%
cd npgsql-6\test\Npgsql.Tests
REM nuget restore Npgsql.Tests.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-6\src\Npgsql.OpenTelemetry
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.OpenTelemetry.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\netstandard2.0 || goto :error

REM FuzzyStringMatch and Trigrams are integrated into the main provider: https://github.com/npgsql/efcore.pg/commit/8af92596a77a1b27b8c75693f9b26b98c066d201

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core
cd %SOURCE_PATH%
cd npgsql-6\EF.core\src\EFCore.PG
REM nuget restore EFCore.PG
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net6.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

cd %SOURCE_PATH%
cd npgsql-6\EF.core\src\EFCore.PG.NodaTime
REM nuget restore EFCore.PG.NodaTime
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net6.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

cd %SOURCE_PATH%
cd npgsql-6\EF.core\src\EFCore.PG.NTS
REM nuget restore EFCore.PG.NTS
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net6.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-6\src\Npgsql.GeoJSON
REM nuget restore Npgsql.GeoJSON.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-6\src\Npgsql.Json.NET
REM nuget restore Npgsql.Json.NET.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0 || goto :error

REM LegacyPostgis has been removed. Details: https://www.npgsql.org/doc/release-notes/6.0.html#npgsqllegacypostgis-has-been-removed
REM mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0
REM cd %SOURCE_PATH%
REM cd npgsql-6\src\Npgsql.LegacyPostgis
REM nuget restore Npgsql.LegacyPostgis.csproj
REM dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
REM copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.LegacyPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-6\src\Npgsql.NetTopologySuite
REM nuget restore Npgsql.NetTopologySuite.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-6\src\Npgsql.NodaTime
REM nuget restore Npgsql.NodaTime.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0 || goto :error

:error
echo "Failed with error %errorlevel%."
exit /b %errorlevel%
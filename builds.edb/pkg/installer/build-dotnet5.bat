CALL "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64

REM @SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"

@SET DOTNET_PATH="C:\\Program Files\\dotnet"
REM @SET PATH=%PGBUILD%\bin;%SOURCE_PATH%;%DOTNET_PATH%;%PATH%
@SET MSBUILD_PATH="C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\MSBuild\\Current\\Bin"
@SET VS_2019_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools"
@SET PATH=%DOTNET_PATH%;%SOURCE_PATH%;%MSBUILD_PATH%;%VS_2019_PATH%;%PATH%

echo "****************************"
dotnet --list-sdks || goto :error
echo "pppppppppppppppppppppppppppp"

cd %SOURCE_PATH%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%

nuget install VSSDK.Shell.12
nuget install AsyncRewriter -Version 0.6.0 -Output packages
nuget install System.Threading.Tasks.Extensions -Version 4.3.0
nuget install EntityFramework
nuget restore npgsql-5.0.1.1\Npgsql.sln

cd npgsql-5.0.1.1\src\Npgsql
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\ %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1
copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error

cd %SOURCE_PATH%
cd npgsql-5.0.1.1\test\Npgsql.Tests
nuget restore Npgsql.Tests.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core
cd %SOURCE_PATH%
cd EF.core\src\EFCore.PG
nuget restore EFCore.PG
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.1\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

cd %SOURCE_PATH%
cd EF.core\src\EFCore.PG.FuzzyStringMatch
nuget restore EFCore.PG.FuzzyStringMatch
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.1\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.FuzzyStringMatch.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

cd %SOURCE_PATH%
cd EF.core\src\EFCore.PG.NodaTime
nuget restore EFCore.PG.NodaTime
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.1\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

cd %SOURCE_PATH%
cd EF.core\src\EFCore.PG.NTS
nuget restore EFCore.PG.NTS
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.1\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error

cd %SOURCE_PATH%
cd EF.core\src\EFCore.PG.Trigrams
nuget restore EFCore.PG.Trigrams
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.1\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Trigrams.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core || goto :error


mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-5.0.1.1\src\Npgsql.GeoJSON
nuget restore Npgsql.GeoJSON.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-5.0.1.1\src\Npgsql.Json.NET
nuget restore Npgsql.Json.NET.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-5.0.1.1\src\Npgsql.LegacyPostgis
nuget restore Npgsql.LegacyPostgis.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.LegacyPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-5.0.1.1\src\Npgsql.NetTopologySuite
nuget restore Npgsql.NetTopologySuite.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0
cd %SOURCE_PATH%
cd npgsql-5.0.1.1\src\Npgsql.NodaTime
nuget restore Npgsql.NodaTime.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0 || goto :error

:error
echo "Failed with error %errorlevel%."
exit /b %errorlevel%

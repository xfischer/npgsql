CALL "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64

@SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"

@SET DOTNET_PATH="C:\\Program Files\\dotnet"
<<<<<<< HEAD
@SET PATH=%PGBUILD%\bin;%SOURCE_PATH%;%DOTNET_PATH%;%PATH%
=======
@SET MSBUILD_PATH="C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\MSBuild\\Current\\Bin"
@SET VS_2019_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools"
@SET PATH=%PGBUILD%\bin;%DOTNET_PATH%;%MSBUILD_PATH%;%VS_2019_PATH%;%PATH%
>>>>>>> 6ba4dcf31aecdd1adcd53597073f836665affb1a

cd %SOURCE_PATH%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%

nuget install VSSDK.Shell.12
nuget install AsyncRewriter -Version 0.6.0 -Output packages
nuget install System.Threading.Tasks.Extensions -Version 4.3.0
nuget install EntityFramework
nuget restore Npgsql.sln

cd src\Npgsql
msbuild.exe Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

echo %cd%
echo %RELEASE_CONFIGURATION%
echo %SOURCE_PATH%

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net461
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Runtime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net461 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.0
copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.0 || goto :error

cd %SOURCE_PATH%
cd src\VSIX
nuget restore VSIX.csproj
msbuild.exe VSIX.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net461

copy bin\%RELEASE_CONFIGURATION%\EnterpriseDB.vsix %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net461 || goto :error
copy bin\%RELEASE_CONFIGURATION%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net461 || goto :error
copy SSDLToPgSQL.tt %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net461 || goto :error
copy %SOURCE_PATH%\src\VSIX\Resources\edb_logo.ico %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net461 || goto :error

cd %SOURCE_PATH%
cd test\Npgsql.Tests
nuget restore Npgsql.Tests.csproj
msbuild.exe Npgsql.Tests.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

cd %SOURCE_PATH%
cd src\EF6.PG
nuget restore EntityFramework6.Npgsql.sln
msbuild.exe EntityFramework6.Npgsql.sln /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform="Any CPU" /p:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF\net461
copy EF6.PG\bin\%RELEASE_CONFIGURATION%\net461\EntityFramework6*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF\net461 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net461
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0

cd %SOURCE_PATH%
cd src\Npgsql.GeoJSON
nuget restore Npgsql.GeoJSON.csproj
msbuild.exe Npgsql.GeoJSON.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\Release\net461\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net461 || goto :error
copy bin\Release\net461\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net461 || goto :error
copy bin\Release\net461\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net461 || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net461
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0

cd %SOURCE_PATH%
cd src\Npgsql.Json.NET
nuget restore Npgsql.Json.NET.csproj
msbuild.exe Npgsql.Json.NET.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\Release\net461\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net461 || goto :error
copy bin\Release\net461\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net461 || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0 || goto :error


mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\net461
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0

cd %SOURCE_PATH%
cd src\Npgsql.LegacyPostgis
nuget restore Npgsql.LegacyPostgis.csproj
msbuild.exe Npgsql.LegacyPostgis.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\Release\net461\EnterpriseDB.EDBClient.LegacyPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\net461 || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.LegacyPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0 || goto :error


mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net461
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0

cd %SOURCE_PATH%
cd src\Npgsql.NetTopologySuite
nuget restore Npgsql.NetTopologySuite.csproj
msbuild.exe Npgsql.NetTopologySuite.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\Release\net461\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net461 || goto :error
copy bin\Release\net461\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net461 || goto :error
copy bin\Release\net461\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net461 || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net461
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0

cd %SOURCE_PATH%
cd src\Npgsql.NodaTime
nuget restore Npgsql.NodaTime.csproj
msbuild.exe Npgsql.NodaTime.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\Release\net461\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net461 || goto :error
copy bin\Release\net461\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net461 || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis\net461
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis\netstandard2.0

cd %SOURCE_PATH%
cd src\Npgsql.RawPostgis
nuget restore Npgsql.RawPostgis.csproj
msbuild.exe Npgsql.RawPostgis.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\Release\net461\EnterpriseDB.EDBClient.RawPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis\net461 || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.RawPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis\netstandard2.0 || goto :error

:error
echo "Failed with error %errorlevel%."
exit /b %errorlevel%

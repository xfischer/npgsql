CALL "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64

@SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"
@SET DOTNET_461_SOURCE_PATH="%7"

@SET DOTNET_PATH="C:\\Program Files\\dotnet"
@SET MSBUILD_PATH="C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\MSBuild\\Current\\Bin"
@SET VS_2017_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools"
@SET PATH=%PGBUILD%\bin;%DOTNET_PATH%;%MSBUILD_PATH%;%VS_2017_PATH%;%PATH%

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

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net45
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net45 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net45 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net45 || goto :error 

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net451
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net451 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net451 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net451 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net451 || goto :error
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net451 || goto :error 

cd %SOURCE_PATH%
cd src\VSIX
nuget restore VSIX.csproj
msbuild /t:restore VSIX.csproj
msbuild.exe VSIX.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net45

copy bin\%RELEASE_CONFIGURATION%\EnterpriseDB.vsix %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net45 || goto :error
copy SSDLToPgSQL.tt %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net45 || goto :error
copy %SOURCE_PATH%\src\VSIX\Resources\edb_logo.ico %STAGING_DIR%\%TARGET_FRAMEWORK%\vsix\net45 || goto :error

cd %SOURCE_PATH%
cd test\Npgsql.Tests
nuget restore Npgsql.Tests.csproj
msbuild.exe Npgsql.Tests.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

xcopy /s %SOURCE_PATH%\test\Npgsql.Tests\bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE% %DOTNET_461_SOURCE_PATH%\test\Npgsql.Tests\bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\
xcopy /s %SOURCE_PATH%\test\Npgsql.Tests\bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1 %DOTNET_461_SOURCE_PATH%\test\Npgsql.Tests\bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\

cd %SOURCE_PATH%
cd src\EntityFramework6.Npgsql
msbuild.exe EntityFramework5.Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

cd %SOURCE_PATH%
cd src\EntityFramework6.Npgsql
nuget restore packages.config -PackagesDirectory packages

msbuild.exe EntityFramework6.Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF\net45
copy bin\%RELEASE_CONFIGURATION%\EntityFramework6*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF\\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\EntityFramework5*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF\net45 || goto :error 

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net45

cd %SOURCE_PATH%
cd src\Npgsql.GeoJSON
nuget restore Npgsql.GeoJSON.csproj
msbuild.exe Npgsql.GeoJSON.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\%RELEASE_CONFIGURATION%\net45\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net45 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net45

cd %SOURCE_PATH%
cd src\Npgsql.Json.NET
nuget restore Npgsql.Json.NET.csproj
msbuild.exe Npgsql.Json.NET.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\%RELEASE_CONFIGURATION%\net45\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net45 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\net45

cd %SOURCE_PATH%
cd src\Npgsql.LegacyPostgis
nuget restore Npgsql.LegacyPostgis.csproj
msbuild.exe Npgsql.LegacyPostgis.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\%RELEASE_CONFIGURATION%\net45\EnterpriseDB.EDBClient.LegacyPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\net45 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net45

cd %SOURCE_PATH%
cd src\Npgsql.NetTopologySuite
nuget restore Npgsql.NetTopologySuite.csproj
msbuild.exe Npgsql.NetTopologySuite.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\%RELEASE_CONFIGURATION%\net45\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\GeoAPI.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net45 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net45

cd %SOURCE_PATH%
cd src\Npgsql.NodaTime
nuget restore Npgsql.NodaTime.csproj
msbuild.exe Npgsql.NodaTime.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\%RELEASE_CONFIGURATION%\net45\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net45 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net45\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net45 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis\net45

cd %SOURCE_PATH%
cd src\Npgsql.RawPostgis
nuget restore Npgsql.RawPostgis.csproj
msbuild.exe Npgsql.RawPostgis.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error

copy bin\%RELEASE_CONFIGURATION%\net45\EnterpriseDB.EDBClient.RawPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\RawPostgis\net45 || goto :error

mkdir %STAGING_DIR%\4.0
mkdir %STAGING_DIR%\4.0\net40

cd %SOURCE_PATH%\Net40\EDBDataProvider2.0.2\src
msbuild.exe EDBDataProvider.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:Platform=%TARGET_PLATFORM% /p:SourceLinkCreate=false || goto :error
copy bin\%RELEASE_CONFIGURATION%\EDBDataProvider2.0.2.dll %STAGING_DIR%\4.0\net40
copy bin\%RELEASE_CONFIGURATION%\Mono.Security.dll %STAGING_DIR%\4.0\net40

:error
echo "Failed with error %errorlevel%."
exit /b %errorlevel%

CALL "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64

@SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"

@SET PATH=%PGBUILD%\bin;%PATH%

cd %SOURCE_PATH%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%1
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard1.3
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DDexProvider

nuget install VSSDK.Shell.12
nuget install AsyncRewriter -Version 0.6.0 -Output packages
nuget install System.Threading.Tasks.Extensions -Version 4.3.0
nuget install EntityFramework
nuget restore Npgsql.sln

cd src\Npgsql
msbuild.exe Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% 

echo %cd%
echo %RELEASE_CONFIGURATION%
echo %SOURCE_PATH%

copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%

copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%1
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%1

copy bin\%RELEASE_CONFIGURATION%\netstandard1.3\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard1.3

cd %SOURCE_PATH%
cd src\NpgsqlDdexProvider
nuget restore NpgsqlDdexProvider.sln

msbuild.exe NpgsqlDdexProvider2010.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% 

copy bin\%RELEASE_CONFIGURATION%\EDBDdexProvider.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DDexProvider
copy bin\%RELEASE_CONFIGURATION%\EDBDdexProvider.vsix %STAGING_DIR%\%TARGET_FRAMEWORK%\DDexProvider
copy SSDLToPgSQL.tt %STAGING_DIR%\%TARGET_FRAMEWORK%\DDexProvider

cd %SOURCE_PATH%
cd src\EDBNunit
msbuild.exe EDBNunit.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% 

cd %SOURCE_PATH%
cd src\EntityFramework6.Npgsql
msbuild.exe EntityFramework5.Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% 

cd %SOURCE_PATH%
cd src\EntityFramework6.Npgsql
nuget restore packages.config -PackagesDirectory packages

msbuild.exe EntityFramework6.Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% 

copy bin\%RELEASE_CONFIGURATION%\EntityFramework6*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF
copy bin\%RELEASE_CONFIGURATION%\EntityFramework5*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF

copy %SOURCE_PATH%\src\NpgsqlDdexProvider\Resources\edb_logo.ico %STAGING_DIR%\%TARGET_FRAMEWORK%\DDexProvider

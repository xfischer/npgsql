CALL "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64

@SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"

@SET DOTNET_PATH="C:\\Program Files\\dotnet"
@SET PATH=%PGBUILD%\bin;%DOTNET_PATH%;%PATH%

cd %SOURCE_PATH%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%

nuget install VSSDK.Shell.12
nuget install AsyncRewriter -Version 0.6.0 -Output packages
nuget install System.Threading.Tasks.Extensions -Version 4.3.0
nuget install EntityFramework
nuget restore Npgsql.sln

cd src\Npgsql
msbuild.exe Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% || goto :error 

echo %cd%
echo %RELEASE_CONFIGURATION%
echo %SOURCE_PATH%

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE% || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE% || goto :error 

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%1
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%1 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%1\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\%FRAMEWORK_DEFINE%1 || goto :error 

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error 

cd %SOURCE_PATH%
cd src\VSIX
nuget restore VSIX.csproj
msbuild.exe VSIX.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM%

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\VSIX

copy bin\%RELEASE_CONFIGURATION%\EnterpriseDB.vsix %STAGING_DIR%\%TARGET_FRAMEWORK%\VSIX || goto :error
copy SSDLToPgSQL.tt %STAGING_DIR%\%TARGET_FRAMEWORK%\VSIX
copy %SOURCE_PATH%\src\VSIX\Resources\edb_logo.ico %STAGING_DIR%\%TARGET_FRAMEWORK%\VSIX || goto :error

cd %SOURCE_PATH%
cd test\Npgsql.Tests
nuget restore Npgsql.Tests.csproj
msbuild.exe Npgsql.Tests.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% || goto :error

cd %SOURCE_PATH%
cd src\EntityFramework6.Npgsql
msbuild.exe EntityFramework5.Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% || goto :error 

cd %SOURCE_PATH%
cd src\EntityFramework6.Npgsql
nuget restore packages.config -PackagesDirectory packages

msbuild.exe EntityFramework6.Npgsql.csproj /p:Configuration=%RELEASE_CONFIGURATION% /p:%FRAMEWORK_DEFINE%=1 /p:Platform=%TARGET_PLATFORM% || goto :error 

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF
copy bin\%RELEASE_CONFIGURATION%\EntityFramework6*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF || goto :error
copy bin\%RELEASE_CONFIGURATION%\EntityFramework5*.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF || goto :error 

:error
echo "Failed with error %errorlevel%."
exit /b %errorlevel%


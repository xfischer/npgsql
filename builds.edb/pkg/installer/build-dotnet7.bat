CALL "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat" -arch=amd64
REM CALL "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvars64.bat"

REM @SET PGBUILD=C:\\pgBuild64
@SET SOURCE_PATH="%1"
@SET TARGET_FRAMEWORK="%2"
@SET RELEASE_CONFIGURATION="%3"
@SET TARGET_PLATFORM="%4"
@SET FRAMEWORK_DEFINE="%5"
@SET STAGING_DIR="%6"

@SET DOTNET_PATH="C:\\Program Files\\dotnet"
@SET PATH=%DOTNET_PATH%;%PATH%

cd %SOURCE_PATH%
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%

REM echo "-->nuget install VSSDK.Shell.12"
REM nuget install VSSDK.Shell.12
REM nuget install AsyncRewriter -Version 0.6.0 -Output packages
REM nuget install System.Threading.Tasks.Extensions -Version 4.3.0
REM nuget install EntityFramework

echo "nuget restore Npgsql.sln"
cd npgsql-7.0.2
REM nuget restore Npgsql.sln

REM echo "msbuild restore"
REM msbuild Npgsql.sln /t:restore /p:Configuration=Release /p:Platform="Any CPU"

REM echo "Mmsbuild build"
REM msbuild Npgsql.sln /t:build /p:Configuration=Release /p:Platform="Any CPU"

echo "dotnet build"
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net7.0
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net7.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\%FRAMEWORK_DEFINE%\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net7.0 || goto :error 

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0
copy bin\%RELEASE_CONFIGURATION%\net6.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\net6.0\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0 || goto :error 
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net6.0 || goto :error

REM mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0
REM copy bin\%RELEASE_CONFIGURATION%\net5.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error 
REM copy bin\%RELEASE_CONFIGURATION%\net5.0\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error 
REM copy bin\%RELEASE_CONFIGURATION%\net5.0\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net5.0 || goto :error

REM mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1
REM copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.1\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.1\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.1\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\netcoreapp3.1\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netcoreapp3.1 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
copy bin\%RELEASE_CONFIGURATION%\netstandard2.0\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.0 || goto :error
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
copy bin\%RELEASE_CONFIGURATION%\netstandard2.1\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\netstandard2.1 || goto :error
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

REM mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net471
REM copy bin\%RELEASE_CONFIGURATION%\net471\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\Microsoft.Bcl.HashCode.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Collections.Immutable.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.Text.Encodings.Web.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error
REM copy bin\%RELEASE_CONFIGURATION%\net471\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net471 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net472
copy bin\%RELEASE_CONFIGURATION%\net472\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\Microsoft.Bcl.HashCode.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Collections.Immutable.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.Text.Encodings.Web.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net472\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net472 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net48
copy bin\%RELEASE_CONFIGURATION%\net48\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\Microsoft.Bcl.HashCode.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Collections.Immutable.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.Text.Encodings.Web.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net48\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net48 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\net481
copy bin\%RELEASE_CONFIGURATION%\net481\EnterpriseDB.EDBClient.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\Microsoft.Extensions.Logging.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\Microsoft.Bcl.AsyncInterfaces.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\Microsoft.Bcl.HashCode.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Collections.Immutable.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Diagnostics.DiagnosticSource.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Memory.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Runtime.CompilerServices.Unsafe.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Threading.Tasks.Extensions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Text.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Threading.Channels.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Numerics.Vectors.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Buffers.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.Text.Encodings.Web.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error
copy bin\%RELEASE_CONFIGURATION%\net481\System.ValueTuple.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\net481 || goto :error

cd %SOURCE_PATH%
cd npgsql-7.0.2\test\Npgsql.Tests
REM nuget restore Npgsql.Tests.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\netstandard2.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net471
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net472
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net48
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net481
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql.OpenTelemetry
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.OpenTelemetry.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\OpenTelemetry.Api.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\netstandard2.0 || goto :error

copy bin\Release\net471\EnterpriseDB.EDBClient.OpenTelemetry.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net471 || goto :error
copy bin\Release\net471\OpenTelemetry.Api.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net471 || goto :error

copy bin\Release\net472\EnterpriseDB.EDBClient.OpenTelemetry.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net472 || goto :error
copy bin\Release\net472\OpenTelemetry.Api.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net472 || goto :error

copy bin\Release\net48\EnterpriseDB.EDBClient.OpenTelemetry.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net48 || goto :error
copy bin\Release\net48\OpenTelemetry.Api.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net48 || goto :error

copy bin\Release\net481\EnterpriseDB.EDBClient.OpenTelemetry.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net481 || goto :error
copy bin\Release\net481\OpenTelemetry.Api.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\OpenTelemetry\net481 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net7.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\netstandard2.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net471
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net472
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net48
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net481
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql.DependencyInjection
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net7.0\EnterpriseDB.EDBClient.DependencyInjection.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net7.0 || goto :error
copy bin\Release\net7.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net7.0 || goto :error

copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.DependencyInjection.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\netstandard2.0 || goto :error

copy bin\Release\net471\EnterpriseDB.EDBClient.DependencyInjection.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net471 || goto :error
copy bin\Release\net471\Microsoft.Extensions.DependencyInjection.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net471 || goto :error

copy bin\Release\net472\EnterpriseDB.EDBClient.DependencyInjection.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net472 || goto :error
copy bin\Release\net472\Microsoft.Extensions.DependencyInjection.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net472 || goto :error

copy bin\Release\net48\EnterpriseDB.EDBClient.DependencyInjection.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net48 || goto :error
copy bin\Release\net48\Microsoft.Extensions.DependencyInjection.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net48 || goto :error

copy bin\Release\net481\EnterpriseDB.EDBClient.DependencyInjection.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net481 || goto :error
copy bin\Release\net481\Microsoft.Extensions.DependencyInjection.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\DependencyInjection\net481 || goto :error

REM FuzzyStringMatch and Trigrams are integrated into the main provider: https://github.com/npgsql/efcore.pg/commit/8af92596a77a1b27b8c75693f9b26b98c066d201

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG.NodaTime
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG.NTS
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net7.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net6.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG.NodaTime\net6.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG.NTS\net6.0
cd %SOURCE_PATH%
cd npgsql-7.0.2\EF.core\src\EFCore.PG
REM nuget restore EFCore.PG
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net7.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net7.0 || goto :error
copy bin\Release\net7.0\Microsoft.EntityFrameworkCore.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net7.0 || goto :error
copy bin\Release\net7.0\Microsoft.EntityFrameworkCore.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net7.0 || goto :error
copy bin\Release\net7.0\Microsoft.EntityFrameworkCore.Relational.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net7.0 || goto :error
copy bin\Release\net6.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net6.0 || goto :error
copy bin\Release\net6.0\Microsoft.EntityFrameworkCore.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net6.0 || goto :error
copy bin\Release\net6.0\Microsoft.EntityFrameworkCore.Abstractions.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net6.0 || goto :error
copy bin\Release\net6.0\Microsoft.EntityFrameworkCore.Relational.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG\net6.0 || goto :error

cd %SOURCE_PATH%
cd npgsql-7.0.2\EF.core\src\EFCore.PG.NodaTime
REM nuget restore EFCore.PG.NodaTime
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net6.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG.NodaTime\net6.0 || goto :error

cd %SOURCE_PATH%
cd npgsql-7.0.2\EF.core\src\EFCore.PG.NTS
REM nuget restore EFCore.PG.NTS
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\net6.0\EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\EF.Core\EFCore.PG.NTS\net6.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net471
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net472
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net48
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net481
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql.GeoJSON
REM nuget restore Npgsql.GeoJSON.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\netstandard2.0 || goto :error

copy bin\Release\net471\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net471 || goto :error
copy bin\Release\net471\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net471 || goto :error

copy bin\Release\net472\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net472 || goto :error
copy bin\Release\net472\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net472 || goto :error

copy bin\Release\net48\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net48 || goto :error
copy bin\Release\net48\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net48 || goto :error

copy bin\Release\net481\EnterpriseDB.EDBClient.GeoJSON.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net481 || goto :error
copy bin\Release\net481\GeoJSON.Net.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\GeoJSON\net481 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net481
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net472
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net48
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net471
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql.Json.NET
REM nuget restore Npgsql.Json.NET.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\netstandard2.0 || goto :error

copy bin\Release\net471\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net471 || goto :error
copy bin\Release\net471\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net471 || goto :error

copy bin\Release\net472\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net472 || goto :error
copy bin\Release\net472\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net472 || goto :error

copy bin\Release\net48\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net48 || goto :error
copy bin\Release\net48\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net48 || goto :error

copy bin\Release\net481\EnterpriseDB.EDBClient.Json.NET.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net481 || goto :error
copy bin\Release\net481\Newtonsoft.Json.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\Json.NET\net481 || goto :error

REM LegacyPostgis has been removed. Details: https://www.npgsql.org/doc/release-notes/6.0.html#npgsqllegacypostgis-has-been-removed
REM mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0
REM cd %SOURCE_PATH%
REM cd npgsql-7.0.2\src\Npgsql.LegacyPostgis
REM nuget restore Npgsql.LegacyPostgis.csproj
REM dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
REM copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.LegacyPostgis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\LegacyPostgis\netstandard2.0 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net471
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net472
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net48
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net481
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql.NetTopologySuite
REM nuget restore Npgsql.NetTopologySuite.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\netstandard2.0 || goto :error

copy bin\Release\net471\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net471 || goto :error
copy bin\Release\net471\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net471 || goto :error
copy bin\Release\net471\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net471 || goto :error

copy bin\Release\net472\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net472 || goto :error
copy bin\Release\net472\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net472 || goto :error
copy bin\Release\net472\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net472 || goto :error

copy bin\Release\net48\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net48 || goto :error
copy bin\Release\net48\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net48 || goto :error
copy bin\Release\net48\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net48 || goto :error

copy bin\Release\net481\EnterpriseDB.EDBClient.NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net481 || goto :error
copy bin\Release\net481\NetTopologySuite.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net481 || goto :error
copy bin\Release\net481\NetTopologySuite.IO.PostGis.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NetTopologySuite\net481 || goto :error

mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net471
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net472
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net48
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net481
mkdir %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net6.0
cd %SOURCE_PATH%
cd npgsql-7.0.2\src\Npgsql.NodaTime
REM nuget restore Npgsql.NodaTime.csproj
dotnet build -property:Configuration=Release -property:SourceLinkCreate=false || goto :error
copy bin\Release\netstandard2.0\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0 || goto :error
copy bin\Release\netstandard2.0\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\netstandard2.0 || goto :error

copy bin\Release\net471\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net471 || goto :error
copy bin\Release\net471\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net471 || goto :error

copy bin\Release\net472\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net472 || goto :error
copy bin\Release\net472\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net472 || goto :error

copy bin\Release\net48\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net48 || goto :error
copy bin\Release\net48\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net48 || goto :error

copy bin\Release\net481\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net481 || goto :error
copy bin\Release\net481\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net481 || goto :error

copy bin\Release\net6.0\EnterpriseDB.EDBClient.NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net6.0 || goto :error
copy bin\Release\net6.0\NodaTime.dll %STAGING_DIR%\%TARGET_FRAMEWORK%\plugins\NodaTime\net6.0 || goto :error

:error
echo "Failed with error %errorlevel%."
exit /b %errorlevel%
@echo off

set SCRIPTDIR=%~dp0

set PSQL="C:\Program Files\edb\as16\bin\edb-psql"

set NPGSQL_HOST=127.0.0.1

set NPGSQL_PORT=%1

set NPGSQL_UID=enterprisedb

set NPGSQL_PWD=edb

set NPGSQL_template_DB=template1

set NPGSQL_DB=test

set NPGSQL_TESTS_LOG=%SCRIPTDIR%tests.log

set PGPASSWORD=%NPGSQL_PWD%

set PGUSER=%NPGSQL_UID%

echo Deleting database %NPGSQL_DB%...

%PSQL% -U %NPGSQL_UID% -h %NPGSQL_HOST% -p %NPGSQL_PORT% -d %NPGSQL_template_DB% -c "drop database %NPGSQL_DB% ;" >> %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)

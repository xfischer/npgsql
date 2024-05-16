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

set NPGSQL_TEST_STRING=-U %NPGSQL_UID% -h %NPGSQL_HOST% -d %NPGSQL_DB% -p %NPGSQL_PORT%


echo Creating test database '%NPGSQL_DB%'...


%PSQL% -U %NPGSQL_UID% -h %NPGSQL_HOST% -p %NPGSQL_PORT% -d %NPGSQL_template_DB% -c "create database %NPGSQL_DB% ;" > %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)



echo Adding test tables...

%PSQL% %NPGSQL_TEST_STRING% -f %SCRIPTDIR%add_tables.sql >> %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)



echo Adding test functions...

%PSQL% %NPGSQL_TEST_STRING% -f %SCRIPTDIR%add_functions.sql >> %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)



echo Adding test views...

%PSQL% %NPGSQL_TEST_STRING% -f %SCRIPTDIR%add_views.sql >> %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)



echo Adding test data...

%PSQL% %NPGSQL_TEST_STRING% -f %SCRIPTDIR%add_data.sql >> %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)


echo Adding sample...

%PSQL% %NPGSQL_TEST_STRING% -f %SCRIPTDIR%edb-sample.sql >> %NPGSQL_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)

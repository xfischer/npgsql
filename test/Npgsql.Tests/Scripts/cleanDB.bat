@echo off

set SCRIPTDIR=%~dp0

set PSQL=edb-psql

set EDB_HOST=127.0.0.1

set EDB_PORT=5444

set EDB_UID=enterprisedb

set EDB_PWD=edb

set EDB_template_DB=template1

set EDB_DB=test

set EDB_TESTS_LOG=%SCRIPTDIR%tests.log

set PGPASSWORD=%EDB_PWD%

set PGUSER=%EDB_UID%

echo Deleting database %EDB_DB%...

%PSQL% -U %EDB_UID% -h %EDB_HOST% -p %EDB_PORT% -d %EDB_template_DB% -c "drop database %EDB_DB% ;" >> %EDB_TESTS_LOG% 2>&1

if not errorlevel 1 (echo OK) else (echo FAILED && exit /b 1)

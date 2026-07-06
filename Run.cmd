@echo off
setlocal
cd /d "%~dp0"

echo === Building backend ===
dotnet build "Backend\TaskManagement.Api\TaskManagement.Api.csproj" -c Debug
if errorlevel 1 goto :fail

echo.
echo === Launching backend on http://localhost:5000 ===
start "TaskManagement API" cmd /k dotnet run --project "Backend\TaskManagement.Api" --no-build

echo.
echo === Building frontend ===
pushd "Frontend\task-manager"
if not exist node_modules (
    echo Installing npm packages...
    call npm install
    if errorlevel 1 goto :failpop
)
call npx ng build --configuration development
if errorlevel 1 goto :failpop
popd

echo.
echo === Launching frontend on http://localhost:4200 ===
start "TaskManagement UI" cmd /k "cd /d "%~dp0Frontend\task-manager" && npx ng serve --open"

echo.
echo ============================================================
echo  Backend API:  http://localhost:5000
echo  Frontend UI:  http://localhost:4200  (opens automatically)
echo  Demo login:   demo@taskmanagement.local / Demo123!
echo  Each app runs in its own window; close them to stop.
echo ============================================================
goto :eof

:failpop
popd
:fail
echo.
echo Build failed - nothing was launched.
exit /b 1

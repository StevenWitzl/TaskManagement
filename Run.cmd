@echo off
setlocal
cd /d "%~dp0"

echo === Building backend ===
dotnet build "Backend\TaskManagement.Api\TaskManagement.Api.csproj" -c Debug
if errorlevel 1 goto :fail

echo.
echo === Launching backend on http://localhost:5000 ===
REM --no-launch-profile ignores launchSettings.json; ASPNETCORE_URLS pins the port
REM so the backend always binds http://localhost:5000 (matches CORS + the frontend).
start "TaskManagement API" cmd /k "cd /d "%~dp0Backend\TaskManagement.Api" && set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://localhost:5000&& dotnet run --no-build --no-launch-profile"

echo.
echo === Waiting for the backend to respond on http://localhost:5000 ... ===
for /l %%i in (1,1,40) do (
    curl -s -o nul http://localhost:5000/api/auth/login >nul 2>&1 && goto :backend_ready
    timeout /t 1 /nobreak >nul
)
echo WARNING: the backend did not respond within 40s.
echo          Check the "TaskManagement API" window for errors before using the app.
:backend_ready

echo.
echo === Building frontend ===
pushd "Frontend\task-manager"
if not exist node_modules (
    echo Installing npm packages...
    call npm install
    if errorlevel 1 goto :failpop
)
call npm run build
if errorlevel 1 goto :failpop
popd

echo.
echo === Launching frontend on http://localhost:4200 ===
start "TaskManagement UI" cmd /k "cd /d "%~dp0Frontend\task-manager" && npm run dev -- --open"

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

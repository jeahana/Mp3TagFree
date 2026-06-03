@echo off
title Smart Finder Build Tool
cls

:MENU
cls
echo =======================================================================
echo          Smart Finder - Modern Build and Distribution Management Tool
echo =======================================================================
echo.
echo  [1] Debug Build
echo  [2] Run in Debug Mode
echo  [3] Publish Standalone Single-File Release
echo  [4] Clean Build Artifacts
echo  [5] Exit
echo.
echo =======================================================================
set /p choice="select number and Enter(1-5): "

if "%choice%"=="1" goto BUILD
if "%choice%"=="2" goto RUN
if "%choice%"=="3" goto PUBLISH
if "%choice%"=="4" goto CLEAN
if "%choice%"=="5" goto EXIT
echo.
echo [!] Wrong Number, Select Number.
timeout /t 2 > nul
goto MENU

:BUILD
echo.
echo [+] Starting to build Project...
dotnet build
if %errorlevel% neq 0 (
    echo.
    echo [!] Failed Build. check Error log.
    echo.
    pause
    goto MENU
)
echo.
echo [+] Success to Build.
echo.
pause
goto MENU

:RUN
echo.
echo [+] run to smart finder...
dotnet run
goto MENU

:PUBLISH
echo.
echo [+] Starting to build to release.
echo.
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
if %errorlevel% neq 0 (
    echo.
    echo [!] Failed Build.
    echo.
    pause
    goto MENU
)
echo.
echo =======================================================================
echo     Completed to Release. (Standalone Single-File Generated)
echo =======================================================================
echo  * File Path : bin\Release\net10.0-windows\win-x64\publish\smartFinder.exe
echo =======================================================================
echo.
echo [+] to open Release Folder.
explorer.exe "bin\Release\net10.0-windows\win-x64\publish"
echo.
pause
goto MENU

:CLEAN
echo.
echo [+] deleting to project file and temp directories.
dotnet clean
if exist obj (
    echo [+] 'obj' deleting cache folder.
    rd /s /q obj
)
if exist bin (
    echo [+] 'bin' deleting output folder.
    rd /s /q bin
)
echo [+] completed to clean work.
echo.
pause
goto MENU

:EXIT
echo.
echo [+] close to Build Tools.
timeout /t 2 > nul
exit /b

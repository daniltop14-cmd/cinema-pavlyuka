@echo off
setlocal
chcp 65001 >nul
cd /d "%~dp0"

rem Use the portable SDK from this workspace, or the installed .NET SDK.
if exist "%~dp0..\..\work\sdk\dotnet.exe" (
    for %%I in ("%~dp0..\..\work\sdk") do set "DOTNET_ROOT=%%~fI"
    set "CINEMA_DOTNET=%~dp0..\..\work\sdk\dotnet.exe"
) else (
    where dotnet >nul 2>&1
    if errorlevel 1 (
        echo Не найден .NET SDK. Установите .NET 8 SDK или откройте проект в Visual Studio.
        pause
        exit /b 1
    )
    set "CINEMA_DOTNET=dotnet"
)

set "ASPNETCORE_ENVIRONMENT=Development"
echo Кинотеатр им. Павлюка
echo После запуска откройте http://localhost:5157
echo SQLite создаётся автоматически. Для остановки нажмите Ctrl+C.
echo.
"%CINEMA_DOTNET%" run --project "%~dp0Cinema.csproj" --no-launch-profile -- --urls http://localhost:5157
set "CINEMA_EXIT_CODE=%ERRORLEVEL%"
if not "%CINEMA_EXIT_CODE%"=="0" pause
exit /b %CINEMA_EXIT_CODE%

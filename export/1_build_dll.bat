@echo off
setlocal EnableExtensions

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build_tools\Export-ChaosHeidemarie.ps1" -Mode Dll
set "EXITCODE=%ERRORLEVEL%"

if not "%EXITCODE%"=="0" (
    echo.
    echo DLL export failed with exit code %EXITCODE%.
    pause
)

exit /b %EXITCODE%

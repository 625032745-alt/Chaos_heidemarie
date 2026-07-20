@echo off
setlocal EnableExtensions

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build_tools\Export-ChaosHeidemarie.ps1" -Mode Pck
set "EXITCODE=%ERRORLEVEL%"

if not "%EXITCODE%"=="0" (
    echo.
    echo PCK export failed with exit code %EXITCODE%.
    pause
)

exit /b %EXITCODE%

@echo off
echo ========================================
echo   Support Copilot Agent
echo ========================================
echo.
echo Starting application...
echo Once started, your browser will open automatically.
echo.
echo The app will be available at:
echo http://localhost:5097
echo.
echo Press Ctrl+C to stop the application.
echo.
echo ========================================
echo.

timeout /t 2 /nobreak >nul

start http://localhost:5097

support-agent.exe

pause

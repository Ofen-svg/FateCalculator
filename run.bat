@echo off
chcp 65001 >nul
title Fate Calculator - запуск
echo ============================================
echo   Fate Calculator - сборка и запуск
echo ============================================
echo.

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ОШИБКА] .NET SDK не найден.
    echo Скачайте и установите .NET 8 SDK: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

cd /d "%~dp0"

echo Восстановление зависимостей (dotnet restore)...
dotnet restore
if %errorlevel% neq 0 (
    echo [ОШИБКА] Не удалось восстановить зависимости.
    pause
    exit /b 1
)

echo.
echo Сборка проекта (dotnet build)...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo [ОШИБКА] Сборка завершилась с ошибкой.
    pause
    exit /b 1
)

echo.
echo Запуск приложения...
dotnet run -c Release --no-build

pause

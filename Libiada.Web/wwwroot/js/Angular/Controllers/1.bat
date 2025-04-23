@echo off
echo Компиляция TypeScript...
tsc -p ../tsconfig.json
if %errorlevel% neq 0 (
    echo Ошибка при компиляции!
    pause
    exit /b %errorlevel%
)
echo Компиляция завершена успешно.
pause
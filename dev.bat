@echo off
echo Iniciando Colegio ERP (Backend + Frontend)...

:: Iniciar Backend en una nueva ventana
start "Colegio Backend" cmd /k "dotnet run --project src/Colegio.Api"

:: Esperar un par de segundos
timeout /t 3 /nobreak > NUL

:: Iniciar Frontend en una nueva ventana
start "Colegio Frontend" cmd /k "cd web && npm run dev"

echo Servicios iniciados en ventanas separadas.
pause

# Script to run Colegio ERP (Backend + Frontend) simultaneously

Write-Host "Iniciando Colegio ERP..." -ForegroundColor Cyan

# 1. Start Backend
Write-Host "Iniciando Backend en puerto 8080..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project src/Colegio.Api"

# 2. Wait a bit for backend to initialize
Start-Sleep -Seconds 2

# 3. Start Frontend
Write-Host "Iniciando Frontend en puerto 5173..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd web; npm run dev"

Write-Host "¡Todo listo! Se han abierto dos ventanas nuevas con los servicios." -ForegroundColor Green
Write-Host "Backend: http://localhost:8080"
Write-Host "Frontend: http://localhost:5173"

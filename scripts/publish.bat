@echo off
echo ========================================================
echo   TasaPlus - Script Oficial de Compilacion Comercial C#
echo ========================================================

set OUTPUT_DIR=%~dp0..\landing\downloads

if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
)

echo [1/3] Compilando TasaPlus.Shared y Backend API...
dotnet build "%~dp0..\backend\DolarMonitorAPI.csproj" -c Release

echo [2/3] Compilando Asistente de Instalacion de Windows (TasaPlus-Setup.exe)...
dotnet publish "%~dp0..\installer\TasaPlusSetup\TasaPlusSetup.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%OUTPUT_DIR%"

echo [3/3] Compilando Artefacto APK Android (TasaPlus.apk)...
dotnet publish "%~dp0..\TasaPlus.App\TasaPlus.App.csproj" -c Release -f net10.0-android -p:AndroidSdkDirectory="C:\Android" -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.20.101-hotspot" -p:AndroidPackageFormat=apk -o "%OUTPUT_DIR%"

if exist "%OUTPUT_DIR%\com.tasaplus.app-Signed.apk" (
    copy /Y "%OUTPUT_DIR%\com.tasaplus.app-Signed.apk" "%OUTPUT_DIR%\TasaPlus.apk" >nul 2>&1
) else if exist "%OUTPUT_DIR%\com.tasaplus.app-signed.apk" (
    copy /Y "%OUTPUT_DIR%\com.tasaplus.app-signed.apk" "%OUTPUT_DIR%\TasaPlus.apk" >nul 2>&1
) else if exist "%OUTPUT_DIR%\com.tasaplus.app.apk" (
    copy /Y "%OUTPUT_DIR%\com.tasaplus.app.apk" "%OUTPUT_DIR%\TasaPlus.apk" >nul 2>&1
)

copy /Y "%OUTPUT_DIR%\TasaPlus.apk" "%~dp0..\frontend\src\landing\downloads\TasaPlus.apk" >nul 2>&1
copy /Y "%OUTPUT_DIR%\TasaPlus-Setup.exe" "%~dp0..\frontend\src\landing\downloads\TasaPlus-Setup.exe" >nul 2>&1

echo ========================================================
echo   Verificacion de Distribuibles en Commercial Downloads
echo ========================================================
if exist "%OUTPUT_DIR%\TasaPlus-Setup.exe" (
    echo [OK] TasaPlus-Setup.exe listo en %OUTPUT_DIR%\TasaPlus-Setup.exe
)
if exist "%OUTPUT_DIR%\TasaPlus.apk" (
    echo [OK] TasaPlus.apk listo en %OUTPUT_DIR%\TasaPlus.apk
)

echo ========================================================
echo   Compilacion y Despliegue Comercial Completado Con Exito!
echo ========================================================

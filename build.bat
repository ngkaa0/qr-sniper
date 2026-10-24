@echo off
chcp 65001 >nul
echo ========================================================
echo   Building QR Sniper v0.9.8 (.NET Framework 4.8)
echo ========================================================

taskkill /f /im QRSniper.exe >nul 2>&1

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo [ERROR] C# compiler not found: %CSC%
    exit /b 1
)

echo Compiling lightweight standalone QRSniper.exe...

set ICON_FLAG=
if exist "app.ico" set ICON_FLAG=/win32icon:app.ico

set RES_FLAGS=/resource:lib\ZXingCpp.dll,QRSniper.Resources.ZXingCpp.dll /resource:lib\ZXing.dll,QRSniper.Resources.ZXing.dll
if exist "app.ico" set RES_FLAGS=%RES_FLAGS% /resource:app.ico,QRSniper.Resources.app.ico

"%CSC%" /target:winexe /optimize+ /unsafe /nologo %ICON_FLAG% /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,lib\ZXingCpp.dll %RES_FLAGS% /win32manifest:app.manifest /out:QRSniper.exe Program.cs Models\*.cs Services\*.cs UI\*.cs

if %ERRORLEVEL% equ 0 (
    if not exist "versions\v0.9.8" mkdir "versions\v0.9.8"
    copy /y "QRSniper.exe" "versions\v0.9.8\QRSniper.exe" >nul
    echo.
    echo [SUCCESS] Standalone QRSniper.exe successfully created!
    echo Embedded native icon and ZXing-C++ library.
    echo Version v0.9.8 archived to versions\v0.9.8
    echo Size: 3.12 MB
) else (
    echo.
    echo [ERROR] Build failed!
)
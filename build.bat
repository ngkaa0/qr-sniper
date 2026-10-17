@echo off
chcp 65001 >nul
echo ========================================================
echo   Building QR Sniper v0.9.6 (.NET Framework 4.8)
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

set RES_FLAGS=/resource:lib\zxing.dll,QRSniper.Resources.zxing.dll
if exist "app.ico" set RES_FLAGS=%RES_FLAGS% /resource:app.ico,QRSniper.Resources.app.ico

"%CSC%" /target:winexe /optimize+ /nologo %ICON_FLAG% /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,lib\zxing.dll %RES_FLAGS% /win32manifest:app.manifest /out:QRSniper.exe Program.cs Models\*.cs Services\*.cs UI\*.cs

if %ERRORLEVEL% equ 0 (
    echo.
    echo [SUCCESS] Standalone QRSniper.exe successfully created!
    echo Embedded native icon and ZXing library.
    echo Size: ~635 KB
) else (
    echo.
    echo [ERROR] Build failed!
)
@echo off
setlocal

echo Cleaning publish folder...
if exist "publish" (
    rmdir /s /q "publish"
    if errorlevel 1 (
        echo Failed to clean publish folder
        exit /b 1
    )
)

echo Publishing project...
dotnet publish StickyKeysService.sln -c Release
if errorlevel 1 (
    echo Publish failed
    exit /b 1
)

echo Build successful, compiling installer...
"C:\Users\pho\AppData\Local\Programs\Inno Setup 6\ISCC.exe" "StickyKeysService.iss"
if errorlevel 1 (
    echo Installer compilation failed
    exit /b 1
)

echo Done!
exit /b 0

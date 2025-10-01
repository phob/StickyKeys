echo Getting version from git...
for /f "delims=" %%i in ('git describe --tags --abbrev^=0') do set GIT_VERSION=%%i
if "%GIT_VERSION%"=="" set GIT_VERSION=1.0.0
echo Version: %GIT_VERSION%

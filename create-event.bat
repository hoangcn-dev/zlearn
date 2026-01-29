@echo off
setlocal enabledelayedexpansion

:: Get Name from param
if "%~1" == "" (
    set /p Name=Type module name: 
) else (
    set Name=%~1
)

:: Make container and subfiles
set "templateDir=D:\projects\ZLearn\module_templates"
mkdir "%Name%"
cd "%Name%"
call :CreateFile "%templateDir%\Event.tem" "%Name%Event.cs" "%Name%"
call :CreateFile "%templateDir%\EventHandler.tem" "%Name%EventHandler.cs" "%Name%"
echo The event "%Name%" was created successfully!
exit /b

:: Subfunction for replace params in template
:CreateFile
set "src=%~1"
set "dest=%~2"
set "name=%~3"
(for /f "usebackq delims=" %%A in ("%src%") do (
    set "line=%%A"
    set "line=!line:$Name$=%name%!"
    echo !line!
)) > "%dest%"
exit /b
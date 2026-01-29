@echo off
setlocal enabledelayedexpansion

:: Get Name and ReturnType from param
if "%~1" == "" (
    set /p Name=Type module name: 
) else (
    set Name=%~1
)
if "%~2" == "" (
    set /p ReturnType=Type return type: 
) else (
    set ReturnType=%~2
)
:: Make container and subfiles
set "templateDir=D:\projects\ZLearn\module_templates"
mkdir "%Name%"
cd "%Name%"
call :CreateFile "%templateDir%\Query.tem" "%Name%Query.cs" "%Name%" "%ReturnType%"
call :CreateFile "%templateDir%\QueryHandler.tem" "%Name%QueryHandler.cs" "%Name%" "%ReturnType%"
echo The query "%Name%" was created successfully!
exit /b

:: Subfunction for replace params in template
:CreateFile
set "src=%~1"
set "dest=%~2"
set "name=%~3"
set "type=%~4"
(for /f "usebackq delims=" %%A in ("%src%") do (
    set "line=%%A"
    set "line=!line:$Name$=%name%!"
    set "line=!line:$ReturnType$=%type%!"
    echo !line!
)) > "%dest%"
exit /b
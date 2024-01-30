rem echo test 0.0
@echo off
rem echo test 0.1
setlocal enabledelayedexpansion
rem echo test 0.2

REM Verkrijg de paden van de bron- en doelmap
rem echo test 1
set "beginPath=%1"
rem echo test 2
set "destinationPath=%2"
rem echo test 3
set "starterProgram=%3"
rem echo  .
rem echo %starterProgram%
rem echo  .
rem echo test 4
set "hasExited=%4"
rem echo test 5
rem echo %0 %1 %2 %3 %4

if "%hasExited%"=="true" (
    GOTO :CONTINUE
) else if "%hasExited%"=="false" (
    echo Starting new instance of the update program...
    rem start "Helpapp" "%0" %beginPath% %destinationPath% %starterProgram% true
    call "%0" %beginPath% %destinationPath% %starterProgram% true
    exit /b 0
) else (
    echo Ongeldig argument. Gebruik "true" of "false".
    pause
    exit /b
)

rem echo test 6

:CONTINUE

REM echo      .
REM echo      Waiting for the main program to exit...
REM :WAIT_LOOP
REM TIMEOUT /T 1 /NOBREAK >NUL
REM WMIC PROCESS WHERE "Name='%starterProgram%.exe'" GET Name | FIND /I "%starterProgram%.exe" >NUL
REM IF ERRORLEVEL 1 GOTO :WAIT_LOOP
REM echo      Main program exited
REM echo      .
REM pause

rem echo Waiting 2 seconds for the main program to exit...
TIMEOUT /T 2 /NOBREAK >NUL
rem pause

rem echo test 7

REM Voer hier de logica uit om bestanden bij te werken

REM Verwijder de tijdelijke map
rmdir /s /q "%tempDir%"
rem echo test 8

REM Maak een tijdelijke map om circulaire koppelingen te vermijden
set "tempDir=%TEMP%\tempCopyDir"
md "%tempDir%"

rem echo test 9

REM Verplaats alle bestanden van de bronmap naar de tijdelijke map
xcopy /s /y /i "%beginPath%\*" "%tempDir%"

rem echo test 10

del "%destinationPath%\*.json" "%destinationPath%\*.dll" "%destinationPath%\*.exe" "%destinationPath%\*.pdb"

REM Verplaats alle bestanden van de tijdelijke map naar de doelmap
xcopy /s /y /i "%tempDir%\*" "%destinationPath%"

REM Verwijder de tijdelijke map
REM rmdir /s /q "%tempDir%"

REM Start het programma dat de hulpapplicatie heeft gestart
start "" "%starterProgram%"
rem %starterProgram%.exe


rem echo     beginPath        %beginPath%
rem echo     destinationPath  %destinationPath%
rem echo     starterProgram   %starterProgram%
rem echo     tempDir          %tempDir%


REM Sluit de hulpapplicatie
exit 0
exit 0

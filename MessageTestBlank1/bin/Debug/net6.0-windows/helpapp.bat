@echo off
setlocal enabledelayedexpansion

REM Verkrijg de paden van de bron- en doelmap
set "beginPath=%1"
set "destinationPath=%2"
set "starterProgram=%3"
set "hasExited=%4"

if "%hasExited%"=="true" (
    GOTO :CONTINUE
) else if "%hasExited%"=="false" (
    echo Starting new instance of helpapp.bat...
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
rem TIMEOUT /T 2 /NOBREAK >NUL
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

del "%destinationPath%\*.dll" "%destinationPath%\*.exe" "%destinationPath%\*.pdb"

REM Verplaats alle bestanden van de tijdelijke map naar de doelmap
xcopy /s /y /i "%tempDir%\*" "%destinationPath%"

REM Read the temp data file
rem for /f "tokens=1,2 delims==" %%a in (%tempDataFilePath%) do (
rem     set "%%a=%%b"
rem )

REM Start the program with the same arguments
echo Starting starterprogram back on...
rem start "" "%starterProgram%" %forceinstall% %debug%
start "" "%starterProgram%" --readStartFile


REM Sluit de hulpapplicatie
echo Exiting helpapp.bat ...
exit /b 0
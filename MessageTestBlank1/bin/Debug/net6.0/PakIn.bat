@echo off
setlocal enabledelayedexpansion

REM Stel het basismap-pad in
set "basePath=C:\Users\MichielsMoriaan\OneDrive - KOGEKA\Documents\Programmeren\source\repos\MessageTestBlank1\MessageTestBlank1\Backup"

REM Vraag de gebruiker om een submapnaam
set /p "subFolder=Voer de submapnaam in: "

REM Controleer of de submapnaam niet leeg is
if not defined subFolder (
    echo Fout: Submapnaam mag niet leeg zijn.
    pause
    exit /b 1
)

REM Volledig pad voor de bestemmingsmap instellen
set "destinationPath=%basePath%\%subFolder%"

REM Maak de bestemmingsmap als deze nog niet bestaat
if not exist "%destinationPath%" md "%destinationPath%"

REM Controleer of de bronmap bestaat
set "sourcePath=C:\Users\MichielsMoriaan\OneDrive - KOGEKA\Documents\Programmeren\source\repos\MessageTestBlank1\MessageTestBlank1\bin\Debug\net6.0"
if not exist "%sourcePath%" (
    echo Fout: De bronmap bestaat niet.
    pause
    exit /b 1
)

REM Controleer of 7-Zip beschikbaar is
where 7z >nul 2>nul
if errorlevel 1 (
    echo Fout: 7-Zip is niet gevonden op dit systeem. Installeer 7-Zip en voeg het toe aan het systeemomgevingspad.
    pause
    exit /b 1
)

REM Stel de bestandsextensies in die moeten worden ingepakt
set "extensions=.dll .exe .pdb .json"

REM Maak een dynamische tijdelijke map voor het inpakken
set "tempDir=%TEMP%\tempZipFolder_%RANDOM%"
md "%tempDir%" 2>nul

REM Kopieer de bestanden naar de tijdelijke map
for %%i in (%extensions%) do (
    copy /Y "%sourcePath%\*%%i" "%tempDir%" 2>nul
)
copy /Y "%sourcePath%\helpapp.bat" "%tempDir%" 2>nul

REM Verpak de bestanden in een ZIP met 7-Zip
7z a -tzip "%destinationPath%\update.zip" "%tempDir%\*"

REM Verwijder de tijdelijke map
rmdir /Q /S "%tempDir%"

echo "Update.zip is gemaakt in %destinationPath%"
pause

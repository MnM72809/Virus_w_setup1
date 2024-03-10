@echo off
setlocal

REM Definieer de map naar het programma zonder de bestandsnaam
set "programFolder=%LOCALAPPDATA%\desk-assistant\assistant\files\program\program"

REM Zoek naar alle .exe-bestanden in de opgegeven map
for %%f in ("%programFolder%\*.exe") do (
    set "programPath=%%f"
    goto :CreateShortcut
)

echo Geen uitvoerbaar bestand (.exe) gevonden in de map: %programFolder%
exit /b 1

:CreateShortcut
REM Definieer de naam van de snelkoppeling en de opstartmap
set "shortcutName=assistant.lnk"
set "startupFolder=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"

REM Maak de snelkoppeling aan
set "targetPath=%programPath%"
set "shortcutPath=%startupFolder%\%shortcutName%"

REM Check if the shortcut file already exists
if exist "%shortcutPath%" (
    echo Shortcut already exists at %shortcutPath%
    rem You can choose to delete the existing shortcut and create a new one
    del "%shortcutPath%"
    rem Create the new shortcut here
)
echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%shortcutPath%" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.TargetPath = "%targetPath%" >> CreateShortcut.vbs
echo oLink.WindowStyle = 7 >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript /nologo CreateShortcut.vbs
del CreateShortcut.vbs


rem attrib +h "%shortcutPath%"

echo Snelkoppeling is aangemaakt op %shortcutPath%

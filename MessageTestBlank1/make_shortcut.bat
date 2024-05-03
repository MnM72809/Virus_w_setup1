@echo off
setlocal

REM Define the program folder without the filename
set "programFolder=%LOCALAPPDATA%\desk-assistant\assistant\files\program\program"

REM Search for all .exe files in the specified folder
for %%f in ("%programFolder%\*.exe") do (
    REM Check if the .exe file is "MessageTestBlank1.exe"
    if /I "%%~nxf"=="MessageTestBlank1.exe" (
        set "programPath=%%f"
        goto :CreateShortcut
    )
)

echo No executable file (.exe) named "MessageTestBlank1.exe" found in the folder: %programFolder%
exit /b 1

:CreateShortcut
REM Define the name of the shortcut and the startup folder
set "shortcutName=assistant.lnk"
set "startupFolder=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"

REM Create the shortcut
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
echo oLink.Arguments = "--lowresources" >> CreateShortcut.vbs
echo oLink.WindowStyle = 7 >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript /nologo CreateShortcut.vbs
del CreateShortcut.vbs

attrib -h "%shortcutPath%"

echo Shortcut has been created at %shortcutPath%
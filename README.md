# MTB1.exe

Welcome to the MTB1.exe project! This started as a fun experiment to develop a prankware application, but has since evolved into a fully functional program. MTB1.exe, short for MessageTestBlank1.exe, was initially created to simulate Windows errors using MessageBox for testing purposes. Over time, it has grown to include a variety of features and functionalities.

The application operates by executing a series of steps upon launch, including updating itself, checking for arguments, and initiating a main loop to execute commands from a database. It also places a shortcut in the startup folder to ensure it runs whenever the machine starts up.

This document provides a comprehensive guide on how to use MTB1.exe, including a list of arguments you can use and a detailed explanation of how the program works. Whether you're here for fun or for testing, we hope you find this guide helpful!

## Q&A

### What is MTB1.exe?

For fun, I tried developing a "virus"; prankware actually. Now, I'm working on it for several months, and this is the result.

### What does MTB1.exe stand for?

MTB1.exe is short for MessageTestBlank1.exe. This name was first chosen for testing purposes, to test a C# application with faked windows errors; using MessageBox. I kept developing the program, and now it fully works. I never changed the name, so I keep it this way.

## How it works

#### Updating

Upon initial execution, the program swiftly terminates and relaunches itself without displaying a window, resulting in a brief appearance of a black screen (console window). Subsequently, it verifies its location within the appdata folder (bypassing the program files directory, which necessitates admin privileges). If located within the appdata folder, it proceeds with execution; otherwise, it initiates the "updating" process. During this phase, the program retrieves the latest version from the server and stores it in the designated folder. Due to a server-imposed file size limit, the update undergoes compression into a .zip file, followed by splitting into multiple parts. The program then combines these parts, extracts the update, and saves it locally. Upon completion of the update process, normal execution resumes.

#### Extra steps

The program places a shortcut in the startup folder, so when the victim's machine starts up, the program does too. The next thing the program does is checking for arguments (see [section Usage](#usage)). Now, the program checks if it is the newest version. If not, it installs the newest update like explained [above](#updating). Finally, it initiates the main loop.

#### Main loop

In a while-loop at an interval of 30 seconds, it checks the database for commands. When found, they are executed. If an error occurs, the program waits 10 seconds before trying again. After 5 seconds, it stops trying and goes back to the main loop.

## Usage

This is a list of the arguments you can use:

- forceupdate
- debug
- enableui
- lowrecources
- setid
- version
- help

Use help ("--help") for more info.

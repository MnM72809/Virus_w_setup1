# MTB1.exe
## Q&A
### What is MTB1.exe?
For fun, I tried developing a "virus"; prankware actually. Now, I'm working on it for several months, and this is the result.
### What does MTB1.exe stand for?
MTB1.exe is short for MessageTestBlank1.exe. This name was first chosen for testing purposes, to test a C# application with faked windows errors; using MessageBox. I kept developing the program, and now it fully works. I never changed the name, so I keep it this way.
## How it works
#### Updating
When you first execute the program, it starts by directly terminating and then starting itself up without a window, so the only thing you're going to see is a flash of a black screen (console window). Then, it continues by checking if it's in the appdata folder (not program files, because admin rights are needed for that). If it is, it continues executing. If it is not, it starts "updating": The program downloads the latest version and saves it in its destination folder. Because there's a file size limit on the server, the update is first compressed to .zip, and then splitted. So, the program combines the splitted files, extracts the update and saves it. After the update is done, it continues executing.
#### Extra steps
The program places a shortcut in the startup folder, so when the victim's machine starts up, the program does too. The next thing the program does is checking for arguments (see [section Arguments](#arguments)). Now, the program checks if it is the newest version. If not, it installs the newest update like explained [above](#updating). Finally, it initiates the main loop.
#### Main loop
In a while-loop at an interval of 30 seconds, it checks the database for commands. When found, they are executed. If an error occurs, the program waits 10 seconds before trying again. After 5 seconds, it stops trying and goes back to the main loop.

 
## Arguments
This is a list of the arguments you can use:
 - forceupdate
 - debug
 - enableui
 - lowrecources
 - setid
 - version
 - help
 
Use help ("--help") for more info.

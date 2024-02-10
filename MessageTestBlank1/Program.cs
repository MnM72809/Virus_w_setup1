using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;
using System.Reflection;
using static DEBUGGER;

class Program
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static bool canClose = true;
    static async Task Main()
    {
        DebugLogMode();

        bool firstArg = true;
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            if (!firstArg)
            {
                switch (arg)
                {
                    case "enableUI":
                        // UPDATE - NOT DONE YET!!!
                        Console.WriteLine("-------------------- ENABLED CONSOLE UI --------------------");
                        break;
                    case "test":
                        // Delete or update this piece of code or just let it in as easter egg
                        Console.WriteLine("test mode (but nothing actually changes rn)");
                        break;
                }
            }
            else { firstArg = false; }
        }



        //_ = await CheckUpdatesAsync(); // Werkt eindelijk

        _ = ThrowError(message: "Test 0.1.0 19:37 10/02/2024");



        while (!canClose)
        {
            // Wacht totdat canClose true wordt
            await Task.Delay(50);
        }
    }


    private static int ThrowError(string message = "Something went wrong.", string title = "Error", uint buttons = 16)
    {
        return MessageBox(IntPtr.Zero, message, title, buttons);
    }

    static async Task<bool> CheckUpdatesAsync()
    {
        // Check for updates
        string currentVersion = "0.1.0";
        string versionUrl = "https://site-mm.000webhostapp.com/v/";

        // Create an instance of the Version class
        Version versionChecker = new();

        // Call the non-static method CheckForUpdates on the instance and asynchronously wait for its completion
        return await Version.CheckForUpdates(currentVersion, versionUrl);
    }
}








class Version
{
    public static async Task<bool> CheckForUpdates(string currentVersion, string versionUrl)
    {
        Program.canClose = false;
        try
        {
            using HttpClient client = new();
            // Haal het versienummer van de nieuwste versie op van de externe bron
            string latestVersion = await client.GetStringAsync(versionUrl + "version.txt");

            // Vergelijk de versienummers
            if (IsUpdateAvailable(currentVersion, latestVersion))
            {
                Console.WriteLine($"A new version is available: {latestVersion}");
                Console.WriteLine("Do you want to install it? (Y/N)");

                // Voer hier logica uit om de update toe te passen indien gewenst
                string response = Console.ReadLine() ?? "N";

                if (response != null && response.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("The update is being applied...");
                    // Download de update
                    // Backup van de huidige bestanden

                    // Hier zou je de logica moeten toevoegen om de updatebestanden te downloaden en de oude bestanden te vervangen.
                    _ = await DownloadUpdateAsync(versionUrl, latestVersion);

                    Environment.Exit(0);
                    // Restart the application
                    Console.WriteLine("The application is restarting...\n\n\n");
                    Thread.Sleep(1000); // Wait a moment
                    // Start a new process to replace the current process
                    Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    // Exit the current process
                    Environment.Exit(0);
                    return true;
                }
                else
                {
                    Console.WriteLine("De update is geannuleerd.");
                }
            }
            else
            {
                Console.WriteLine("You have the newest version. Do you want to install it anyways? (Y/N)");

                // Voer hier logica uit om de update toe te passen indien gewenst
                string response = Console.ReadLine() ?? "N";

                if (response != null && response.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("The update is being applied...");
                    // Download de update
                    // Backup van de huidige bestanden

                    // Hier zou je de logica moeten toevoegen om de updatebestanden te downloaden en de oude bestanden te vervangen.
                    _ = await DownloadUpdateAsync(versionUrl, latestVersion);

                    Environment.Exit(0);
                    // Restart the application
                    Console.WriteLine("The application is restarting...\n\n\n");
                    Thread.Sleep(1000); // Wait a moment
                    // Start a new process to replace the current process
                    Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    // Exit the current process
                    Environment.Exit(0);
                    return true;
                }
                else
                {
                    Console.WriteLine("De update is geannuleerd.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij het controleren op updates: {ex.Message}");
        }
        finally
        {
            Program.canClose = true;
        }
        return false;
    }

    static bool IsUpdateAvailable(string currentVersion, string latestVersion)
    {
        // Voer hier logica uit om te bepalen of een update beschikbaar is
        // Dit kan bijvoorbeeld een vergelijking van versienummers zijn.

        // Hier is een eenvoudige vergelijking, maar het kan complexer zijn
        return string.Compare(currentVersion, latestVersion) < 0;
    }

    static async Task<bool> DownloadUpdateAsync(string versionUrl, string latestVersion)
    {
        try
        {
            using HttpClient client = new();

            // DEBUG
            LogDebug("Function DownloadUpdateAsync before installdir");

            string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "desk-assistant", "assistant", "files", "program");
            DEBUGGER.LogDebug("installDir: " + installDir);
            //Console.WriteLine($"Proceed? (Y/N) installDir: {installDir}");
            string response = "Y"; //Console.ReadLine() ?? "N";
            if (!(response != null && response.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Exiting program...");
                System.Environment.Exit(0);
            }
            if (!EnsureDirExists(installDir))
            {
                LogDebug("installDir doesn't exist yet");
            }









            LogDebug("Function DownloadUpdateAsync after installdir, before downloading");

            // Download het updatebestand
            byte[] updateBytes = await client.GetByteArrayAsync(versionUrl + "data/newest/update.zip");

            LogDebug("Function DownloadUpdateAsync after downloading, before saving");                                                            //------------------------------------------------------------------------------


            // Sla het updatebestand op
            string updateFileName = "update.zip"; // Geef een bestandsnaam op
            string updateFilePath = Path.Combine(installDir, "updates", "version", latestVersion, updateFileName);
            #pragma warning disable CS8604 // Possible null reference argument.
            EnsureDirExists(Path.GetDirectoryName(updateFilePath)); // Zorg ervoor dat de map bestaat
            #pragma warning restore CS8604 // Possible null reference argument.

            // Gebruik FileStream om het bestand te schrijven
            using (FileStream fileStream = new(updateFilePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(updateBytes, 0, updateBytes.Length);
                await fileStream.FlushAsync();
            }


            LogDebug("Function DownloadUpdateAsync after saving, before exrtacting");

            // Uitpakken van het zip-bestand
            string extractpath = Path.Combine(installDir, "temp", "update");
            Console.WriteLine("");
            LogDebug("TEST After extractPath");
            if (Directory.Exists(extractpath))
            {
                Directory.Delete(extractpath, true);
            }
            LogDebug("TEST After deleting temp");
            EnsureDirExists(extractpath);
            LogDebug("TEST After EnsureDirExists");
            ExtractZip(updateFilePath, extractpath);
            LogDebug("TEST After Extracting");
            Console.WriteLine("");

            LogDebug("Function DownloadUpdateAsync after extracting");

            string destinationPath = Path.Combine(installDir, "program");

            LogDebug("Installing helpapp.bat...");
            File.Copy(Path.Combine(extractpath, "helpapp.bat"), Path.Combine(destinationPath, "helpapp.bat"), true);

            // Logica om de oude bestanden te vervangen
            Console.WriteLine("Update is downloaded and extracted. Applying update...");
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

    static void StartHelpApp(string beginPath, string destinationPath, string installDir)
    {
        string helperAppPath = Path.Combine(installDir, "program", "helpapp.bat");
        string starterProgram = Environment.ProcessPath;
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
                    DEBUGGER.LogDebug("Test 1");
                    // Maak een nieuwe DirectorySecurity-object om de machtigingen te beheren
                    DirectorySecurity directorySecurity = new();

                    DEBUGGER.LogDebug("Test 2");

                    // Krijg de SID voor de "Everyone" groep
                    SecurityIdentifier everyoneSid = new(WellKnownSidType.WorldSid, null);

                    DEBUGGER.LogDebug("Test 3");

                    // Voeg de gewenste machtigingen toe (bijv. schrijfmachtigingen voor iedereen)
                    directorySecurity.AddAccessRule(new FileSystemAccessRule(everyoneSid, FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

                    DEBUGGER.LogDebug("Test 4");

                    // Krijg toegang tot de directory en pas de nieuwe machtigingen toe
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);

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
    private static LogMode logMode = LogMode.Auto;

    public enum LogMode
    {
        Skip = 0,
        Log = 1,
        Auto = 2
    }
    public static void LogDebug(string message, bool forceLog = false)
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
            Console.WriteLine($"DEBUG; {message} at line {lineNumber}");
        } else if (logMode == LogMode.Auto)
        {
            #if DEBUG
            Console.WriteLine($"DEBUG; {message} at line {lineNumber}");
            #endif
        }
    }

    public static void DebugLogMode(int mode = 2)
    {
        logMode = (LogMode)mode;
    }
}
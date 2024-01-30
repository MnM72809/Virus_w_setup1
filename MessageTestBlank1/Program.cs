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

class Program
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static bool canClose = true;
    static async Task Main()
    {
        _ = await CheckUpdatesAsync(); // Werkt eindelijk

        ThrowError(message: "Test 0.0.2 17:48 30/01/2024");

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
        string currentVersion = "0.0.2";
        string versionUrl = "https://site-mm.000webhostapp.com/v/";

        // Create an instance of the Version class
        Version versionChecker = new();

        // Call the non-static method CheckForUpdates on the instance and asynchronously wait for its completion
        return await versionChecker.CheckForUpdates(currentVersion, versionUrl);
    }
}








class Version
{
    public async Task<bool> CheckForUpdates(string currentVersion, string versionUrl)
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
                Console.WriteLine("Do you want to install it? (J/N)");

                // Voer hier logica uit om de update toe te passen indien gewenst
                string response = Console.ReadLine() ?? "N";

                if (response != null && response.Trim().Equals("J", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("The update is being applied...");
                    // Download de update
                    // Backup van de huidige bestanden

                    // Hier zou je de logica moeten toevoegen om de updatebestanden te downloaden en de oude bestanden te vervangen.
                    _ = await DownloadUpdateAsync(versionUrl, latestVersion);

                    System.Environment.Exit(0);
                    // Restart the application
                    Console.WriteLine("The application is restarting...\n\n\n");
                    Thread.Sleep(1000); // Wait a moment
                    // Start a new process to replace the current process
                    System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    // Exit the current process
                    System.Environment.Exit(0);
                    return true;
                }
                else
                {
                    Console.WriteLine("De update is geannuleerd.");
                }
            }
            else
            {
                Console.WriteLine("Je hebt de nieuwste versie.");
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
            DEBUGGER.LogDebug("Function DownloadUpdateAsync before installdir");

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
                DEBUGGER.LogDebug("installDir doesn't exist yet");
            }









            DEBUGGER.LogDebug("Function DownloadUpdateAsync after installdir, before downloading");

            // Download het updatebestand
            byte[] updateBytes = await client.GetByteArrayAsync(versionUrl + "data/" + latestVersion + "/update.zip");

            DEBUGGER.LogDebug("Function DownloadUpdateAsync after downloading, before saving");                                                            //------------------------------------------------------------------------------


            // Sla het updatebestand op
            string updateFileName = "update.zip"; // Geef een bestandsnaam op
            string updateFilePath = Path.Combine(installDir, "updates", "version", latestVersion, updateFileName);
            EnsureDirExists(Path.GetDirectoryName(updateFilePath)); // Zorg ervoor dat de map bestaat

            // Gebruik FileStream om het bestand te schrijven
            using (FileStream fileStream = new(updateFilePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(updateBytes, 0, updateBytes.Length);
                await fileStream.FlushAsync();
            }


            DEBUGGER.LogDebug("Function DownloadUpdateAsync after saving, before exrtacting");

            // Uitpakken van het zip-bestand
            string extractpath = Path.Combine(installDir, "temp", "update");
            Console.WriteLine("");
            DEBUGGER.LogDebug("TEST After extractPath");
            if (Directory.Exists(extractpath))
            {
                Directory.Delete(extractpath, true);
            }
            DEBUGGER.LogDebug("TEST After deleting temp");
            EnsureDirExists(extractpath);
            DEBUGGER.LogDebug("TEST After EnsureDirExists");
            ExtractZip(updateFilePath, extractpath);
            DEBUGGER.LogDebug("TEST After Extracting");
            Console.WriteLine("");

            DEBUGGER.LogDebug("Function DownloadUpdateAsync after extracting");

            string destinationPath = Path.Combine(installDir, "program");

            DEBUGGER.LogDebug("Installing helpapp.bat...");
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

                DEBUGGER.LogDebug("Test 5");

                directoryInfo.SetAccessControl(directorySecurity);

                DEBUGGER.LogDebug("Test 6");

                Console.WriteLine("Toestemming verleend. Je kunt naar de opgegeven locatie schrijven.");

                // Voer hier je schrijflogica uit
                // Bijvoorbeeld: File.WriteAllText(Path.Combine(targetPath, "bestand.txt"), "Hallo, wereld!");

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Toestemming geweigerd: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
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
    static public void LogDebug(string message)
    {
        StackTrace stackTrace = new(true);
        StackFrame frame = stackTrace.GetFrame(1); // 0 is the current method, 1 is the caller
        int lineNumber = frame.GetFileLineNumber();
#if DEBUG
        Console.WriteLine($"DEBUG; {message} at line {lineNumber}");
#endif
    }
}
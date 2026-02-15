namespace ConsoleFixer;

using BepInEx;
using BepInEx.Configuration;
using ConsoleFixer.Listeners;
using HarmonyLib;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BepInExLogger = BepInEx.Logging.Logger;

/// <summary> General Plugin class used for adding the fixed pdb's whenever you don't have them. </summary>
[BepInPlugin("Bryan_-000-.ConsoleFixer", "ConsoleFixer", "1.6.0")]
public class Plugin : BaseUnityPlugin
{
    /// <summary> "It's called harmony because it harms your mental health." - Doomah 24/12/2025 </summary>
    public Harmony harmony = new Harmony("Bryan_-000-.ConsoleFixer");

    /// <summary> Mods to filter from BepInEx->PLog redirection. </summary>
    public static ConfigEntry<string> FilterBepInEx;

    /// <summary> Loads all the patches and stuff. </summary>
    public void Awake()
    {
        AddFixedPdbs();
        LoadBinds();

        harmony.PatchAll(typeof(Patches));
    }

    /// <summary> Handles/Loads all the binds and settings for the mod. </summary>
    public void LoadBinds()
    {
        // debug build setting
        if (Config.Bind("Settings", "Debug build", true, "Whether we should confuse the game into believeing this is a debug build. (requires reload)").Value)
            harmony.PatchAll(typeof(DebugBuildPatch));

        // Bep filter setting
        FilterBepInEx = Config.Bind("Settings", "BepPLogFilter", "Unity Log, UltraEditor", "Mods to filter out from the BepInEx to PLog redirector. (Split by ',')");
        Logger.LogInfo(string.Join("|", FilterBepInEx.Value.Split(", ")));

        // redirecting bep logs to plog setting
        if (Config.Bind("Settings", "Log Bep to PLog", true, "Whether to log BepInEx logs to PLog/F8 aswell. (requires reload)").Value)
            BepInExLogger.Listeners.Add(new BepLogger());
    }

    /// <summary> Deletes all the old .pd_ files and adds the new .pdb files. </summary>
    public void AddFixedPdbs()
    {
        string[] Files = Directory.GetFiles(Paths.ManagedPath, "*.pd_");

        if (Files == Enumerable.Empty<string>()) return;

        foreach (string pb_File in Files)
            File.Delete(pb_File);

        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ConsoleFixer.pdbs.zip");

        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);

        string tempZipPath = Path.Combine(Application.temporaryCachePath, "pdbs.zip");
        File.WriteAllBytes(tempZipPath, data);

        ZipFile.ExtractToDirectory(tempZipPath, Paths.ManagedPath);

        File.Delete(tempZipPath);
    }
}
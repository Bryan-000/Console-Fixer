namespace ConsoleFixer;

using BepInEx;
using ConsoleFixer.Listeners;
using HarmonyLib;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary> General Plugin class used for adding the fixed pdb's whenever you don't have them. </summary>
[BepInPlugin("Bryan_-000-.ConsoleFixer", "ConsoleFixer", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    /// <summary> "It's called harmony because it harms your mental health." - Doomah 24/12/2025 </summary>
    public Harmony harmony;

    /// <summary> Loads all the patches and stuff. </summary>
    public void Awake()
    {
        AddFixedPdbs();
        harmony = new Harmony("Bryan_-000-.ConsoleFixer");
        harmony.PatchAll(typeof(Patches));
    }

    /// <summary> Handles/Loads all the binds and settings for the mod. </summary>
    public void Start()
    {
        bool redirectBep = Config.Bind("Settings", "Log Bep to PLog", true, "Whether to log BepInEx logs to PLog/F8 aswell.").Value;
        if (redirectBep) BepInEx.Logging.Logger.Listeners.Add(new BepLogger());
        
        bool redirectUnity = Config.Bind("Settings", "Debug build", true, "Whether we should confuse the game into believeing this is a debug build.").Value;
        if (redirectUnity) harmony.PatchAll(typeof(DebugBuildPatch));
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
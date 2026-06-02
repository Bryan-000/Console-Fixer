global using BepInExLogger = BepInEx.Logging.Logger;
global using PLogger = plog.Logger;

namespace ConsoleFixer;

using BepInEx;
using ConsoleFixer.Listeners;
using HarmonyLib;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using UnityEngine;
using static BepInEx.BepInDependency;

/// <summary> General Plugin class used for adding the fixed pdb's whenever you don't have them. </summary>
[BepInDependency("com.eternalUnion.pluginConfigurator", DependencyFlags.SoftDependency)] // softdependency so pluginconfig loads first if you have it,
[BepInPlugin(Information.GUID, Information.Name, "1.7.0")]                                          // but still loads console fixer if you dont
public class Plugin : BaseUnityPlugin
{
    public static class Information
    {
        public const string GUID = "Bryan_-000-.ConsoleFixer";
        public const string Name = "ConsoleFixer";
        public const string Version = "1.7.0";

        public static readonly Assembly Assembly = typeof(Information).Assembly;
    }

    /// <summary> "It's called harmony because it harms your mental health." - Doomah 24/12/2025 </summary>
    public static Harmony harmony = new(Information.GUID);

    /// <summary> Logger used for redirecting BepInEx logs to PLog as well. </summary>
    public static BepLogger Bep2PLogger = new();

    /// <summary> Loads all the patches and stuff. </summary>
    public void Awake()
    {
        AddFixedPdbs();
        Settings.Load(Config);

        harmony.PatchAll(typeof(Patches));
    }

    /// <summary> I have to call this for config settings to update. </summary>
    public void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus)
            Config.Reload();
    }

    /// <summary> Deletes all the old .pd_ files and adds the new .pdb files. </summary>
    public void AddFixedPdbs()
    {
        try
        {
            string tempZipPath = Path.Combine(Application.temporaryCachePath, "pdbs.zip");
            using (FileStream writeStream = File.Create(tempZipPath))
            {
                using Stream stream = Information.Assembly.GetManifestResourceStream("pdbs.zip");
                stream.CopyTo(writeStream);
            }

            ZipFile.ExtractToDirectory(tempZipPath, Paths.ManagedPath, true);
            File.Delete(tempZipPath);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex.Message + "\n" + ex.StackTrace);
        }
    }
}
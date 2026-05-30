global using BepInExLogger = BepInEx.Logging.Logger;
global using PLogger = plog.Logger;

using BepInEx;
using BepInEx.Configuration;
using ConsoleFixer.Listeners;
using HarmonyLib;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ConsoleFixer;

/// <summary> General Plugin class used for adding the fixed pdb's whenever you don't have them. </summary>
[BepInPlugin("Bryan_-000-.ConsoleFixer", "ConsoleFixer", "1.6.0")]
public class Plugin : BaseUnityPlugin
{
    /// <summary> "It's called harmony because it harms your mental health." - Doomah 24/12/2025 </summary>
    public static Harmony harmony = new("Bryan_-000-.ConsoleFixer");

    /// <summary> Mods to filter from BepInEx->PLog redirection. </summary>
    public static ConfigEntry<string> ConfigFilterBepInEx;

    /// <summary> Whether to redirect BepInEx logs to PLog/F8 aswell. </summary>
    public static ConfigEntry<bool> ConfigBep2PLog;

    /// <summary> Whether to confuse the game into believing it's running in a Debug build. </summary>
    public static ConfigEntry<bool> ConfigDebugBuild;

    /// <summary> Logger used for redirecting BepInEx logs to PLog aswell. </summary>
    public static BepLogger Bep2PLogger = new();

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
        ConfigDebugBuild = Config.Bind("Settings", "Debug build", true, "Whether we should confuse the game into believeing this is a debug build.");
        if (ConfigDebugBuild.Value) harmony.PatchAll(typeof(DebugBuildPatch));

        ConfigDebugBuild.SettingChanged += (_, _) =>
        {
            Logger.LogInfo("ConfigDebugBuild.SettingChanged: " + ConfigDebugBuild.Value);
            harmony.UnpatchSelf();

            harmony.PatchAll(typeof(Patches));
            if (ConfigDebugBuild.Value)
            harmony.PatchAll(typeof(DebugBuildPatch));
        };

        // redirecting bep logs to plog setting
        ConfigBep2PLog = Config.Bind("Settings", "Log Bep to PLog", true, "Whether to log BepInEx logs to PLog aka F8 aswell.");
        if (ConfigBep2PLog.Value) BepInExLogger.Listeners.Add(Bep2PLogger);

        ConfigBep2PLog.SettingChanged += (_, _) =>
        {
            Logger.LogInfo("ConfigBep2PLog.SettingChanged: " + ConfigBep2PLog.Value);
            if (ConfigBep2PLog.Value)
            {
                if (!BepInExLogger.Listeners.Contains(Bep2PLogger))
                    BepInExLogger.Listeners.Add(Bep2PLogger);
            }
            else
            {
                BepInExLogger.Listeners.Remove(Bep2PLogger);
            }
        };

        };

        // Bep filter setting
        ConfigFilterBepInEx = Config.Bind("Settings", "BepPLogFilter", "Unity Log, UltraEditor", "Mods to filter out from the BepInEx to PLog aka F8 redirector.");
        Logger.LogInfo("filter: " + string.Join(", ", ConfigFilterBepInEx.Value.Split([",", ", "], StringSplitOptions.RemoveEmptyEntries)));
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
            string[] Files = Directory.GetFiles(Paths.ManagedPath, "*.pd_");
            if (Files != Enumerable.Empty<string>())
            {
                foreach (string pb_File in Files)
                    File.Delete(pb_File);
            }

            string tempZipPath = Path.Combine(Application.temporaryCachePath, "pdbs.zip");

            FileStream writeStream = File.Create(tempZipPath);
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("pdbs.zip");

            stream.CopyTo(writeStream);
            writeStream.Close();
            stream.Close();

            ZipFile.ExtractToDirectory(tempZipPath, Paths.ManagedPath, true);
            File.Delete(tempZipPath);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex.Message + "\n" + ex.StackTrace);
        }
    }
}
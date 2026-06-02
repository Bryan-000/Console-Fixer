namespace ConsoleFixer;

using BepInEx.Configuration;
using System;
using UnityEngine;

/// <summary> Static class for handling and creating settings. </summary>
public static class Settings
{
    /// <summary> Mod config file handler (must be set before loading). </summary>
    public static ConfigFile Config;

    /// <summary> Logger for debugging :3 </summary>
    public static PLogger Log = new("ConsoleFixer.Settings");

    /// <summary> Whether to confuse the game into believing it's running in a Debug build. </summary>
    public static ConfigEntry<bool> DebugBuild;

    /// <summary> Whether to redirect BepInEx logs to PLog/F8 as well. </summary>
    public static ConfigEntry<bool> Bep2PLog;

    /// <summary> What to set the unityLogger filter log mode to. </summary>
    public static ConfigEntry<LogType> LogFilterMode;

    /// <summary> Mods to filter from BepInEx -> PLog redirection. </summary>
    public static ConfigEntry<string> FilterBepInEx;
    public static string[] FilterBepInEx_Value;

    /// <summary> Handles/Loads all the settings for the mod. </summary>
    public static void Load(ConfigFile config)
    {
        Config ??= config;

        // Debug build setting
        DebugBuild = Config.Bind("Settings", "Debug Build", true, "Whether we should confuse the game into believing this is a debug build.");
        if (DebugBuild.Value) Plugin.harmony.PatchAll(typeof(DebugBuildPatch));

        DebugBuild.SettingChanged += (_, _) =>
        {
            Log.Info("DebugBuild.SettingChanged: " + DebugBuild.Value);
            Plugin.harmony.UnpatchSelf();

            Plugin.harmony.PatchAll(typeof(Patches));
            if (DebugBuild.Value)
                Plugin.harmony.PatchAll(typeof(DebugBuildPatch));
        };



        // Redirecting Bep logs to PLog setting
        Bep2PLog = Config.Bind("Settings", "Log Bep to PLog", true, "Whether to log BepInEx logs to PLog aka F8 as well.");
        if (Bep2PLog.Value) BepInExLogger.Listeners.Add(Plugin.Bep2PLogger);

        Bep2PLog.SettingChanged += (_, _) =>
        {
            Log.Info("Bep2PLog.SettingChanged: " + Bep2PLog.Value);
            if (Bep2PLog.Value)
            {
                if (!BepInExLogger.Listeners.Contains(Plugin.Bep2PLogger))
                    BepInExLogger.Listeners.Add(Plugin.Bep2PLogger);
            }
            else
            {
                BepInExLogger.Listeners.Remove(Plugin.Bep2PLogger);
            }
        };



        // Log filter mode
        LogFilterMode = Config.Bind("Settings", "Log Filter Mode", LogType.Log, "What unity log levels should be logged.");
        Debug.unityLogger.filterLogType = LogFilterMode.Value;

        LogFilterMode.SettingChanged += (_, _) =>
        {
            Log.Info("LogFilterMode.SettingChanged: " + LogFilterMode.Value);
            Debug.unityLogger.filterLogType = LogFilterMode.Value;
        };



        // Bep filter setting
        FilterBepInEx = Config.Bind("Settings", "Bep to PLog Filter", "Unity Log, UltraEditor", "Mods to filter out from the BepInEx to PLog aka F8 redirector.");
        FilterBepInEx_Value = FilterBepInEx.Value.Split([",", ", "], StringSplitOptions.RemoveEmptyEntries);

        FilterBepInEx.SettingChanged += (_, _) =>
        {
            Log.Info("FilterBepInEx.SettingChanged: " + FilterBepInEx.Value);
            FilterBepInEx_Value = FilterBepInEx.Value.Split([",", ", "], StringSplitOptions.RemoveEmptyEntries);
        };

        // trytrytrysafeload (im just being safe okay?? 3:)
        try
        {
            TrySafeLoadingPluginConfigUI();
        }
        catch { }
    }

    /// <summary> Just being extra sure cuz .net likes to scream about type load exceptions... </summary>
    public static void TrySafeLoadingPluginConfigUI()
    {
        try
        {
            PluginConfigGUI.SafeLoad();
        }
        catch { }
    }
}
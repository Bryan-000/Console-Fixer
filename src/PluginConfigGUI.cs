namespace ConsoleFixer;

using BepInEx.Bootstrap;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.IO;
using UnityEngine;
using static ConsoleFixer.Plugin;
using static System.Net.Mime.MediaTypeNames;

/// <summary> Static class for creating and intializing GUI for PluginConfigurator. </summary>
public static class PluginConfigGUI
{
    /// <summary> Whether Plugin Configurator is installed in the current context. </summary>
    public static bool IsPluginConfigInstalled => Chainloader.PluginInfos.ContainsKey("com.eternalUnion.pluginConfigurator");

    /// <summary> Checks if PluginConfigurator is installed b4 calling <see cref="LoadConfig"/>. </summary>
    public static void SafeLoad()
    {
        try
        {
            if (IsPluginConfigInstalled)
                LoadConfig();
        }
        catch { }
    }

    /// <summary> PluginConfig Configurator instance for this GUI :3 </summary>
    public static PluginConfigurator config;

    /// <summary> Whether to confuse the game into believing it's running in a Debug build. </summary>
    public static BoolField DebugBuild;

    /// <summary> Whether to redirect BepInEx logs to PLog/F8 as well. </summary>
    public static BoolField Bep2PLog;

    /// <summary> What to set the unityLogger filter log mode to. </summary>
    public static EnumField<LogType> LogFilterMode;

    /// <summary> Mods to filter from BepInEx -> PLog redirection. </summary>
    public static StringField FilterBepInEx;

    /// <summary> Loads the GUI for Plugin Config. </summary>
    public static void LoadConfig()
    {
        config = PluginConfigurator.Create(Information.Name, Information.GUID);
        config.presetButtonHidden = true; // fuck u
        config.image = GrabIcon();

        Bep2PLog = new(config.rootPanel, "Log Bep to PLog", "log_bep_to_plog", Settings.Bep2PLog.Value, saveToConfig: false);
        DebugBuild = new(config.rootPanel, "Debug Build", "tricked_debug_build", Settings.DebugBuild.Value, saveToConfig: false);
        LogFilterMode = new(config.rootPanel, "Log Filter Mode", "log_filter_mode", Settings.LogFilterMode.Value, saveToConfig: false);
        FilterBepInEx = new(config.rootPanel, "Bep to PLog Filter", "bep_to_plog_filter", Settings.FilterBepInEx.Value, false, saveToConfig: false);

        Bep2PLog.value = Settings.Bep2PLog.Value;
        DebugBuild.value = Settings.DebugBuild.Value;
        LogFilterMode.value = Settings.LogFilterMode.Value;
        FilterBepInEx.value = Settings.FilterBepInEx.Value;

        Bep2PLog.onValueChange += data => Settings.Bep2PLog.Value = data.value;
        DebugBuild.onValueChange += data => Settings.DebugBuild.Value = data.value;
        LogFilterMode.onValueChange += data => Settings.LogFilterMode.Value = data.value;
        FilterBepInEx.onValueChange += data => Settings.FilterBepInEx.Value = data.value;
    }

    /// <summary> Grabs the mod icon. </summary>
    public static Sprite GrabIcon()
    {
        using Stream stream = Information.Assembly.GetManifestResourceStream("icon.png");
        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);

        Texture2D icon_tex = new(0, 0);
        bool loadSuccess = icon_tex.LoadImage(data);

        if (loadSuccess)
        {
            Sprite icon = Sprite.Create(icon_tex, new(0, 0, icon_tex.width, icon_tex.height), new(0.5f, 0.5f));
            return icon;
        }
        else return null;
    }
}
namespace ConsoleFixer;

using BepInEx;
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
    /// <summary> Loads all the patches and stuff. </summary>
    public void Awake()
    {
        bool redirectBep = Config.Bind("Settings", "RedirectBep", true, "Whether to redirect BepInEx logs to PLog/F8.").Value;
        if (redirectBep) BepInEx.Logging.Logger.Listeners.Add(new BepRedirector());

        AddFixedPdbs();
        new Harmony("Bryan_-000-.ConsoleFixer").PatchAll();
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
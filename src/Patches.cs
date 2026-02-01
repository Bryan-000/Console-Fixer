namespace ConsoleFixer;

using GameConsole;
using HarmonyLib;
using plog;
using plog.Models;
using plog.unity.Extensions;
using plog.unity.Handlers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

/// <summary> General Patches class for any and all patches. </summary>
[HarmonyPatch]
public class Patches
{
    /// <summary> Transpiler which makes it so stack traces are formatted nicely with all their newlines since for some reason pitr decided he hates newlines. </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(LogLine), "PopulateLine")]
    public static IEnumerable<CodeInstruction> PopulateLineTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo ReplaceString = AccessTools.Method(typeof(string), "Replace", [typeof(string), typeof(string)]);

        List<CodeInstruction> CompletedInstructions = [];

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Callvirt && instruction.operand != null && (MethodInfo)instruction.operand == ReplaceString)
            {
                CompletedInstructions.RemoveAt(CompletedInstructions.Count - 1);
                CompletedInstructions.RemoveAt(CompletedInstructions.Count - 1); // i feel like i could make this better but every other way felt worse or had 2 pops after stuff
            }
            else CompletedInstructions.Add(instruction);
        }

        return CompletedInstructions;
    }

    /// <summary> All of our cached loggers for UnityLogPrefix. </summary>
    public static Dictionary<string, Logger> CachedLoggers = [];

    /// <summary> Patches PLog's unity logger to make it look nicer. </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UnityProxy), "LogMessageReceived")]
    public static bool UnityLogPrefix(string message, string stacktrace, UnityEngine.LogType type)
    {
        string name = GetName();
        Logger Logger;
        if (CachedLoggers.TryGetValue(name, out var tryResult))
            Logger = tryResult;
        else
        {
            Logger = new(name);
            CachedLoggers.Add(name, Logger);
        }
         
        Logger.Record(message, type.ToPLogLevel(), stackTrace: UnityEngine.StackTraceUtility.ExtractFormattedStackTrace(new(4, true)));
        return false;
    }

    /// <summary> Gets the name of a class via stackTrace's for UnityLogPrefix. </summary>
    public static string GetName()
    {
        var frameType = new StackTrace().GetFrame(9).GetMethod().DeclaringType;
        string name = frameType.Name;

        // if the class name is "Plugin" then use the namespace cuz "Plugin" doesnt tell you much
        if (name == "Plugin")
            name = frameType.Namespace;

        return name;
    }
}

/// <summary> Patches the game/unity to make it think this is a debug build. </summary>
[HarmonyPatch]
public class DebugBuildPatch
{
    /// <summary> Tells the game that this is def a debug build to log all the logs. </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Debug), "get_isDebugBuild")]
    public static void DebugBuildPostfix(ref bool __result) =>
        __result = true;

    /// <summary> This causes a log loop if you have Log Bep to PLog and Debug build enabled. </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UnityProxy), "HandleRecord")]
    public static bool FuckYou(ref Log __result, Log log)
    {
        __result = log;
        return false;
    }
}
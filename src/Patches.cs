namespace ConsoleFixer;

using GameConsole;
using HarmonyLib;
using plog;
using plog.Models;
using plog.unity.Extensions;
using plog.unity.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

/// <summary> General Patches class for any and all patches. </summary>
[HarmonyPatch]
public class Patches
{
    /// <summary> Transpiler which makes it so stack traces are formatted nicely with all their newlines since for some reason pitr decided he hates newlines. </summary>
    [HarmonyTranspiler] [HarmonyPatch(typeof(LogLine), "PopulateLine")]
    public static IEnumerable<CodeInstruction> PopulateLineTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo ReplaceString = AccessTools.Method(typeof(string), "Replace", [typeof(string), typeof(string)]);

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(ReplaceString))
            {
                yield return new(OpCodes.Pop);
                yield return new(OpCodes.Pop);
            }
            else 
                yield return instruction;
        }
    }

    /// <summary> Basically in Unity StackTrace's replace like `NewMovement` with `Assembly-CSharp.NewMovement`. </summary>
    [HarmonyTranspiler] [HarmonyPatch(typeof(StackTraceUtility), "ExtractFormattedStackTrace")]
    public static IEnumerable<CodeInstruction> AddAssemblyNameToGlobalNamespaces(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        MethodInfo get_Namespace = AccessTools.PropertyGetter(typeof(Type), "Namespace");
        MethodInfo get_Assembly = AccessTools.PropertyGetter(typeof(Type), "Assembly");
        MethodInfo GetName = AccessTools.Method(typeof(Assembly), "GetName");
        MethodInfo get_Name = AccessTools.PropertyGetter(typeof(AssemblyName), "Name");
        CodeInstruction[] instructionsArr = [.. instructions];

        for (int i = 0; i < instructionsArr.Length; i++)
        {
            CodeInstruction instruction = instructionsArr[i];

            yield return instruction;
            if (instruction.Calls(get_Namespace))
            {
                Label nextInstructionLabel = il.DefineLabel();
                CodeInstruction nextinstruction = instructionsArr[++i];
                nextinstruction.labels.Add(nextInstructionLabel);

                yield return new(OpCodes.Dup); // dupes the namespace to check if its null
                yield return new(OpCodes.Brtrue, nextInstructionLabel); // skip if not null

                // if (namespace == null)
                    yield return new(OpCodes.Pop); // remove null namespace reference
                    yield return instructionsArr[i-2].Clone(); // do whatever got us the type to call get_Namespace on
                    yield return new(OpCodes.Callvirt, get_Assembly); // Type.Assembly
                    yield return new(OpCodes.Callvirt, GetName); // Type.Assembly.GetName()
                    yield return new(OpCodes.Callvirt, get_Name); // Type.Assembly.GetName().Name

                yield return new(nextinstruction);
            }
        }
    }

    /// <summary> All of our cached loggers for UnityLogPrefix. </summary>
    public static Dictionary<string, PLogger> CachedLoggers = [];

    /// <summary> Patches PLog's unity logger to make it look nicer. </summary>
    [HarmonyPrefix] [HarmonyPatch(typeof(UnityProxy), "LogMessageReceived")]
    public static bool UnityLogPrefix(string message, string stacktrace, LogType type)
    {
        string name = GetName();
        PLogger Logger;
        if (CachedLoggers.TryGetValue(name, out var tryResult))
            Logger = tryResult;
        else
        {
            Logger = new(name);
            CachedLoggers.Add(name, Logger);
        }
         
        Logger.Record(message, type.ToPLogLevel(), stackTrace: StackTraceUtility.ExtractFormattedStackTrace(new(4, true)));
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
    [HarmonyPostfix] [HarmonyPatch(typeof(UnityEngine.Debug), "get_isDebugBuild")]
    public static void DebugBuildPostfix(ref bool __result) =>
        __result = true;

    /// <summary> This causes a log loop if you have Log Bep to PLog and Debug build enabled. </summary>
    [HarmonyPrefix] [HarmonyPatch(typeof(UnityProxy), "HandleRecord")]
    public static bool FuckYou(ref Log __result, Log log)
    {
        __result = log;
        return false;
    }
}
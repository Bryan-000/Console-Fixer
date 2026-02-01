namespace ConsoleFixer;

using GameConsole;
using HarmonyLib;
using plog.Models;
using plog.unity.Handlers;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

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
}

/// <summary> Patches the game/unity to make it think this is a debug build. </summary>
[HarmonyPatch]
public class DebugBuildPatch
{
    /// <summary> Tells the game that this is def a debug build to log all the logs. </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Debug), "get_isDebugBuild")]
    public static void DebugBuild(ref bool __result) =>
        __result = true;

    /// <summary> This causes a log loop if you have Log Bep to PLog and Debug build enabled. </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UnityProxy), "HandleRecord")]
    public static bool FuckYou(ref Log __result, Log log)
    {
        __result = log;
        return true;
    }
}
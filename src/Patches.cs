namespace ConsoleFixer;

using GameConsole;
using HarmonyLib;
using System.Collections.Generic;
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
}
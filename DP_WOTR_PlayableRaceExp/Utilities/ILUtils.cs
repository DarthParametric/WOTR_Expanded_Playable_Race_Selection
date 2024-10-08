﻿using HarmonyLib;
using System.Reflection.Emit;
using DP_WOTR_PlayableRaceExp.ModLogic;

namespace DP_WOTR_PlayableRaceExp.Utilities {
    public static class ILUtils {
        public static void LogIL(ModContextBase context, List<CodeInstruction> codes) {
            context.Logger.LogVerbose("");
            for (int i = 0; i < codes.Count; i++) {
                object operand = codes[i].operand;
                if (operand is Label) {
                    context.Logger.Log($"{i} - {codes[i].labels.Aggregate("", (s, label) => $"{s}[{label.GetHashCode()}]")} - {codes[i].opcode} - {operand.GetHashCode()}");
                } else {
                    context.Logger.Log($"{i} - {codes[i].labels.Aggregate("", (s, label) => $"{s}[{label.GetHashCode()}]")} - {codes[i].opcode} - {codes[i].operand}");
                }
            }
        }
    }
}

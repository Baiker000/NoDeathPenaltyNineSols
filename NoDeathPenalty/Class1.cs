using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace NoDeathPenalty
{

    [BepInPlugin("neko.NoDeathPenalty", "No Death Penalty", "1.0.0")]
    [BepInProcess("NineSols.exe")]
    public class NoDeathPenalty : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("neko.NoDeathPenalty");


        void Awake()
        {
            harmony.PatchAll();
            Debug.Log("NoDeathPenalty plugin loaded");
        }


        //[HarmonyPatch(typeof(PlayerGamePlayData), nameof(PlayerGamePlayData.PlayerDeathPenalty))]
        [HarmonyPatch]
        public class PlayerDeathPenaltyPatch
        {

            static MethodBase TargetMethod()
            {
                var type = AccessTools.FirstInner(typeof(PlayerGamePlayData), t => AccessTools.GetDeclaredMethods(t).Any(m => m.Name == "MoveNext"));
                return AccessTools.Method(type, "MoveNext");
            }


            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_1 && codes[i + 1].opcode == OpCodes.Ldc_I4_0 && codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand.ToString().Contains("set_CurrentGold"))
                    {
                        codes[i].opcode = OpCodes.Nop;
                        codes[i + 1].opcode = OpCodes.Nop;
                        codes[i + 2].opcode = OpCodes.Nop;
                    }
                    if (codes[i].opcode == OpCodes.Ldloc_1 && codes[i + 1].opcode == OpCodes.Ldc_I4_0 && codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand.ToString().Contains("set_CurrentExp"))
                    {
                        codes[i].opcode = OpCodes.Nop;
                        codes[i + 1].opcode = OpCodes.Nop;
                        codes[i + 2].opcode = OpCodes.Nop;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(PlayerDeadRecord))]
        [HarmonyPatch("StorePlayerDataBeforeDead")]
        public static class PlayerDeadRecord_StorePlayerDataBeforeDead_Patch
        {
            static void Postfix(PlayerDeadRecord __instance)
            {
                __instance.ContainEXP.CurrentValue = 0;
                __instance.ContainMoney.CurrentValue = 0;
            }
        }
    }
}

﻿using HarmonyLib;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;

namespace DP_WOTR_PlayableRaceExp.NewEvents {
    public interface ISpontaneousConversionHandler : IUnitSubscriber {
        void HandleGetConversions(AbilityData ability, ref IEnumerable<AbilityData> conversions);

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetConversions))]
        static class AbilityData_GetConversions_SpellSpecializationGreater_Patch {
            static void Postfix(AbilityData __instance, ref IEnumerable<AbilityData> __result) {
                var conversions = __result;
                EventBus.RaiseEvent<ISpontaneousConversionHandler>(__instance.Caster, h => h.HandleGetConversions(__instance, ref conversions));
                __result = conversions;
            }
        }
    }
}

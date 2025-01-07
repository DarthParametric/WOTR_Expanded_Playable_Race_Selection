using Kingmaker.Blueprints.Classes;
using Kingmaker.Visual.Mounts;
using static DP_WOTR_PlayableRaceExp.Main;

namespace DP_WOTR_PlayableRaceExp.Utilities
{
	// Modified after https://github.com/EdgarEbonfowl/EbonsContentMod/blob/main/Utilities/RaceMountFixerizer.cs
	internal class FixRaceMountOffsets
	{
		internal static readonly Dictionary<BlueprintRace, BlueprintRace> RaceOriginals = new();

		public static void AddRaceToMountFixes(BlueprintRace newrace, BlueprintRace copiedrace)
		{
			RaceExpContext.Logger.LogDebug($"FixRaceMountOffsets.AddRaceToMountFixes: Adding {newrace.name} based on {copiedrace.name} to offset fix dictionary");
			RaceOriginals[newrace] = copiedrace;
		}

		[HarmonyPatch]
		static class MountPatches
		{
			[HarmonyPatch(typeof(MountOffsets), nameof(MountOffsets.GetMountOffsets))]
			[HarmonyPostfix]
			static void MountOffsetsFix(BlueprintRace race, MountOffsets __instance, ref RaceMountOffsetsConfig.MountOffsetData __result)
			{
				if (__result == null && RaceOriginals.TryGetValue(race, out var originalRace))
				{
					try
					{
						RaceExpContext.Logger.LogDebug($"FixRaceMountOffsets.MountPatches.MountOffsetsFix: Running Harmony patch on MountOffsetData, new race \"{race.name}\", orig race \"{originalRace.name}\"");
						__result = __instance.GetMountOffsets(originalRace);
					}
					catch (Exception e)
					{
						RaceExpContext.Logger.Log($"FixRaceMountOffsets.MountPatches.MountOffsetsFix: Caught exception trying to patch MountOffsetData!\n{e}");
					}
				}
			}
		}
	}
}

using DP_WOTR_PlayableRaceExp.Config;
using DP_WOTR_PlayableRaceExp.ModLogic;
using DP_WOTR_PlayableRaceExp.Races;
using DP_WOTR_PlayableRaceExp.Utilities;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.BundlesLoading;
using Kingmaker.Modding;
using Kingmaker.ResourceLinks;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace DP_WOTR_PlayableRaceExp;

internal static class Main {
	public static ModContextPlayableRaceExp RaceExpContext;
	public static Dictionary<string, string> AssetsInBundles = new();
	public static HashSet<string> Bundles = new();
	
	static bool Load(UnityModManager.ModEntry modEntry)
	{
		var harmony = new Harmony(modEntry.Info.Id);
		RaceExpContext = new ModContextPlayableRaceExp(modEntry);
		RaceExpContext.LoadAllSettings();
		CreateAssetLinks.LoadAllSettings();

		harmony.PatchAll();
		PostPatchInitializer.Initialize(RaceExpContext);

		return true;
	}

	[HarmonyPatch]
	public static class AssetHandler
	{
		[HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.TryLoadBundle)), HarmonyPrefix]
		public static bool TryLoadBundle(string bundleName, ref AssetBundle __result)
		{
			if (CreateAssetLinks.Bundles.Contains(bundleName))
			{
				RaceExpContext.Logger.LogDebug($"Loading bundle: {bundleName}");

				__result = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Bundles", $"{bundleName}"));

				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.GetBundleNameForAsset)), HarmonyPrefix]
		public static bool GetBundleNameForAsset(string guid, ref string __result)
		{
			if (CreateAssetLinks.AssetsInBundles.TryGetValue(guid, out var bundle))
			{
				RaceExpContext.Logger.LogDebug($"Redirecting asset with GUID: {guid} to AssetBundle: {bundle}");
				__result = bundle;
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.GetDependenciesForBundle)), HarmonyPrefix]
		public static bool GetDependenciesForBundle(string bundleName, ref DependencyData __result)
		{
			if (CreateAssetLinks.Bundles.Contains(bundleName))
			{
				__result = null;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(AssetBundle))]
	public static class AssetPatcher
	{
		public static Dictionary<string, Action<UnityEngine.Object>> LoadActions = new();

		[HarmonyPatch(nameof(AssetBundle.LoadAsset), typeof(string), typeof(Type)), HarmonyPostfix]
		public static void LoadAsset(string name, ref UnityEngine.Object __result)
		{
			if (LoadActions.TryGetValue(name, out var action))
			{
				RaceExpContext.Logger.LogDebug($"Patching asset on load: {name}");
				action(__result);
			}
		}
	}

	[HarmonyPatch(typeof(BlueprintsCache))]
	public static class BlueprintsCaches_Patch
	{
		private static bool Initialized = false;

		[HarmonyPriority(Priority.First)]
		[HarmonyPatch(nameof(BlueprintsCache.Init)), HarmonyPostfix]
		public static void Init_Postfix()
		{
			try
			{
				if (Initialized)
				{
					RaceExpContext.Logger.LogDebug("Already initialised blueprints cache.");
					return;
				}

				Initialized = true;

				RaceExpContext.Logger.Log("Installing race/s.");
				DPDrow.Install();
			}
			catch (Exception e)
			{
				RaceExpContext.Logger.LogException(e, "Mod initialisation failed.");
			}
		}
	}
}
using DP_WOTR_PlayableRaceExp.ModLogic;
using DP_WOTR_PlayableRaceExp.Races;
using DP_WOTR_PlayableRaceExp.Utilities;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.BundlesLoading;
using Kingmaker.Modding;
using Kingmaker.ResourceLinks;
using Kingmaker.SharedTypes;
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
		private static Dictionary<string, Shader> shadersByName;
		private static Dictionary<string, Material> materialsByName;

		// Search the Bundles sub-folder in the mod's install folder for the bundle/s.
		[HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.TryLoadBundle)), HarmonyPrefix]
		public static bool TryLoadBundle(string bundleName, ref AssetBundle __result)
		{
			if (CreateAssetLinks.Bundles.Contains(bundleName))
			{
				RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: Loading bundle {bundleName}");

				__result = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Bundles", $"{bundleName}"));

				// Since imported shaders are broken, swap the shaders in the bundle with a donor vanilla one.
				EquipmentEntityLink DonorHead = new EquipmentEntityLink { AssetId = EE_Names_IDs.Get_EE_ID("ee_body01_m_de") };
				RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: Harvesting vanilla donor head ee_body01_m_de, asset ID {DonorHead.AssetId}");

				if (materialsByName == null)
				{
					materialsByName = new();
					materialsByName["Character_Diffuse_Cutout"] = DonorHead.Load(false).BodyParts[0].Material;
				}

				if (shadersByName == null)
				{
					shadersByName = new();
					shadersByName["Owlcat/Lit"] = DonorHead.Load(false).BodyParts[0].Material.shader;
				}

				RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: Donor material = {DonorHead.Load(false).BodyParts[0].Material.name}, shader = {DonorHead.Load(false).BodyParts[0].Material.shader.name}");

				var materialCollection = __result.LoadAllAssets<OwlcatModificationMaterialsInBundleAsset>();
				RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: Loading bundle MaterialsInBundle list {materialCollection}");

				try
				{
					if (materialCollection != null)
					{
						RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: MaterialsInBundle length = {materialCollection.Length}");
						foreach (var entry in materialCollection)
						{
							for (int i = 0; i < entry.Materials.Length; i++)
							{
								var material = entry.Materials[i];
								RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: Fixing material {i}, {material.name}");

								if (material == null)
								{
									RaceExpContext.Logger.LogDebug("Main.TryLoadBundle: Null material, probably stale asset, skipping");
									continue;
								}

								//RaceExpContext.Logger.LogDebug($"Main.TryLoadBundle: Attempting to replace bundle {material.name} with donor's...");
								//materialsByName.TryGetValue(material.name, out var replacementMat);
								//material = replacementMat;
								//RaceExpContext.Logger.LogDebug("Main.TryLoadBundle: If we got this far, success?");
						
 								if (material.shader != null && shadersByName.TryGetValue(material.shader.name, out var replacement))
								{
									RaceExpContext.Logger.LogDebug("Main.TryLoadBundle: Attempting to replace shader with donor's...");
									material.shader = replacement;
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					RaceExpContext.Logger.LogDebug($"Caught exception trying to replace material:\n{e}");
				}

				return false;
			}
			return true;
		}

		// Map each asset to a bundle.
		[HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.GetBundleNameForAsset)), HarmonyPrefix]
		public static bool GetBundleNameForAsset(string guid, ref string __result)
		{
			if (CreateAssetLinks.AssetsInBundles.TryGetValue(guid, out var bundle))
			{
				RaceExpContext.Logger.LogDebug($"Main.GetBundleNameForAsset: Redirecting asset with GUID {guid} to AssetBundle {bundle}");
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
				RaceExpContext.Logger.LogDebug($"Main.LoadAsset: Patching asset {name} on load");
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
					RaceExpContext.Logger.LogDebug("Already initialised blueprints cache patch.");
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
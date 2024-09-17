using DP_WOTR_PlayableRaceExp.Config;

namespace DP_WOTR_PlayableRaceExp.Utilities
{
	internal static class CreateAssetLinks
	{
		public static void LoadAllSettings()
		{
			try
			{
				LoadAssetLinks();
			}
			catch (Exception e)
			{
				Main.RaceExpContext.Logger.LogException(e, "Error loading asset links!");
			}
			
		}

		public static Dictionary<string, string> AssetsInBundles = new();
		public static HashSet<string> Bundles = new();

		private static void LoadAssetLinks()
		{
			foreach (var item in BundlesAssets.Bundles_Dict)
			{
				var guid = item.Key;
				var bundle = item.Value;

				if (!Bundles.Contains(bundle)) { Main.RaceExpContext.Logger.LogDebug($"Added bundle {bundle} to list."); }
				if (!AssetsInBundles.ContainsKey(guid)) { Main.RaceExpContext.Logger.LogDebug($"Linked asset {guid} to bundle {bundle}."); }

				Bundles.Add(bundle);
				AssetsInBundles[guid] = bundle;
			}

			Main.RaceExpContext.Logger.LogDebug($"Found {AssetsInBundles.Count} asset links in {Bundles.Count} bundles");
		}
	}
}

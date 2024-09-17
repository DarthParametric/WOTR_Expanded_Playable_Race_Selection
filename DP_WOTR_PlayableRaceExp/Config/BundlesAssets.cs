
namespace DP_WOTR_PlayableRaceExp.Config
{
	internal class BundlesAssets
	{
		internal static Dictionary<string, string> Bundles_Dict = new Dictionary<string, string>
		{
			{"acafa75bba164494d84dc032b4a9aa17", "DPPlayableRaceExp_assets"},
			{"870d5d1642a40df459ea61ff6e130d09", "DPPlayableRaceExp_assets"},
			{"6e0870bb006ff8c48b2718c730d50229", "DPPlayableRaceExp_assets"},
			{"b416f98f4e35268489e03b4e309e7da9", "DPPlayableRaceExp_assets"},
			{"c14daab90dc43004ca1a0dae48515479", "DPPlayableRaceExp_assets"},
			{"c45dcb72726b7a649bf35a9ceae40700", "DPPlayableRaceExp_assets"}
		};

		internal static string Get_Bundle_Name(string key)
		{
			if (Bundles_Dict.TryGetValue(key, out var bundle))
			{
				return bundle;
			}
			else
			{
				Main.RaceExpContext.Logger.LogDebug($"Bundle not found for {key}!");
				return "";
			}
		}
	}
}

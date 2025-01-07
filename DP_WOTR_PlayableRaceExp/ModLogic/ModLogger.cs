using Kingmaker.Blueprints.JsonSystem;
using Owlcat.Runtime.Core.Logging;
using static UnityModManagerNet.UnityModManager;

namespace DP_WOTR_PlayableRaceExp.ModLogic {
	public class ModLogger {

		private readonly ModEntry ModEntry;
		private readonly LogChannel ModChannel;
		internal string LogTimestamp = DateTimeOffset.Now.ToString("HH:mm:ss:ffff");

		public ModLogger(ModEntry ModEntry) {
			this.ModEntry = ModEntry;
			ModChannel = LogChannelFactory.GetOrCreate(ModEntry.Info.Id);
		}

		public void Log(string message) {
			ModEntry.Logger.Log($"[{LogTimestamp}] {message}");
		}

		public void LogNotNull(string v, object obj)
		{
			ModEntry.Logger.Log($"{v} not-null: {obj != null}");
		}

		public void LogVerbose(string message)
		{
			ModChannel.Verbose(message);
		}

		public void LogWarning(string message)
		{
			ModEntry.Logger.Log($"WARNING: {message}");
		}

		public void LogPatch(IScriptableObjectWithAssetId bp)
		{
			LogPatch("Patched", bp);
		}

		public void LogPatch(string action, IScriptableObjectWithAssetId bp)
		{
			Log($"{action}: {bp.AssetGuid} - {bp.name}");
		}

		public void LogHeader(string message)
		{
			ModEntry.Logger.Log($"--{message.ToUpper()}--");
		}

		public void LogException(Exception e, string message) {

			ModChannel.Error(message);
			ModEntry.Logger.Log($"[{LogTimestamp}] ERROR: {message}");
			ModEntry.Logger.Log(e.ToString());
		}

		public void LogError(string message) {
			ModChannel.Error(message);
			ModEntry.Logger.Log($"[{LogTimestamp}] ERROR: {message}");
		}

		public void LogDebug(string message) {
#if DEBUG
			ModEntry.Logger.Log($"[{LogTimestamp}] DEBUG: {message}");
#endif
		}
	}
}

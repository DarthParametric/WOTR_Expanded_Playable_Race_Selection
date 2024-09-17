using static DP_WOTR_PlayableRaceExp.Main;
using static UnityModManagerNet.UnityModManager;

namespace DP_WOTR_PlayableRaceExp.ModLogic {
    internal class ModContextPlayableRaceExp : ModContextBase {

        public ModContextPlayableRaceExp(ModEntry ModEntry) : base(ModEntry) {
        }
        public override void LoadAllSettings() {
            LoadBlueprints("DP_WOTR_PlayableRaceExp.Config", RaceExpContext);
            LoadLocalization("DP_WOTR_PlayableRaceExp.Localization");
        }
        public override void AfterBlueprintCachePatches() {
            base.AfterBlueprintCachePatches();
            if (Debug) {
                ModLocalizationPack.RemoveUnused();
                SaveLocalization(ModLocalizationPack);
            }
        }
    }
}

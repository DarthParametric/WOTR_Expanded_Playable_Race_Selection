namespace DP_WOTR_PlayableRaceExp.Config {
    public interface IUpdatableSettings {
        void OverrideSettings(IUpdatableSettings userSettings);
        void Init();
    }
}

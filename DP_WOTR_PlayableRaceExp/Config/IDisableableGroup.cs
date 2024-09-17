namespace DP_WOTR_PlayableRaceExp.Config {
    public interface IDisableableGroup : ICollapseableGroup {
        bool GroupIsDisabled();
        void SetGroupDisabled(bool value);
    }
}

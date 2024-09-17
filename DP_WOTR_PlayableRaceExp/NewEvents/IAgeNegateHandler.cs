using Kingmaker.PubSubSystem;

namespace DP_WOTR_PlayableRaceExp.NewEvents {
    public interface IAgeNegateHandler : IUnitSubscriber {
        void OnAgeNegateChanged();
    }
}

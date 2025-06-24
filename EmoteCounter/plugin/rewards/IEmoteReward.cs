using Dalamud.Game.ClientState.Objects.SubKinds;

namespace EmoteCounter
{
    public interface IEmoteReward
    {
        void OnCounterChanged(EmoteCounter counterOb, IPlayerCharacter instigator, out bool stopProcessing);
    }
}

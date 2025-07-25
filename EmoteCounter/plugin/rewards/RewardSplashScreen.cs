﻿using Dalamud.Game.ClientState.Objects.SubKinds;

namespace EmoteCounter
{
    internal class RewardSplashScreen : IEmoteReward
    {
        public void OnCounterChanged(EmoteCounter counterOb, IPlayerCharacter instigator, out bool stopProcessing)
        {
            var isSpecial = (counterOb.Value < 25) ? (counterOb.Value == 5 || counterOb.Value == 15) : ((counterOb.Value % 25) == 0);
            var canShow = isSpecial && Service.pluginConfig.showSpecialPats;

            if (canShow)
            {
                Service.splashScreen.Show();
            }

            stopProcessing = false;
        }
    }
}

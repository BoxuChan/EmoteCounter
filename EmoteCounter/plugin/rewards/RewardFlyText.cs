﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.FlyText;

namespace EmoteCounter
{
    internal class RewardFlyText : IEmoteReward
    {
        public void OnCounterChanged(EmoteCounter counterOb, IPlayerCharacter instigator, out bool stopProcessing)
        {
            if (Service.pluginConfig.showFlyText)
            {
                var useDesc = counterOb.descSingular.ToUpper();
                var useSubDesc = Service.pluginConfig.showFlyTextNames && instigator != null ? instigator.Name : " ";

                Service.flyTextGui?.AddFlyText(FlyTextKind.DamageCritDh, 0, counterOb.Value, 0, useDesc, useSubDesc, 0xff00ff00, 0, 0);
            }

            stopProcessing = false;
        }
    }
}

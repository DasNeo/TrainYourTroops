// Decompiled with JetBrains decompiler
// Type: TrainYourTroops.SubModule
// Assembly: TrainYourTroops, Version=0.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 84C41D81-ABF0-404B-8A1F-077159C3B98C
// Assembly location: C:\Users\andre\Downloads\TrainYourTroops.dll
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TrainYourTroops
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad() => base.OnSubModuleLoad();

        protected override void OnSubModuleUnloaded() => base.OnSubModuleUnloaded();

        protected override void OnBeforeInitialModuleScreenSetAsRoot() => base.OnBeforeInitialModuleScreenSetAsRoot();

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (!(game.GameType is Campaign))
                return;

            ((CampaignGameStarter)gameStarter).AddBehavior((CampaignBehaviorBase)new TroopTrainingBehaviour());
        }
    }
}

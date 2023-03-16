// Decompiled with JetBrains decompiler
// Type: TrainYourTroops.TroopTrainingBehaviour
// Assembly: TrainYourTroops, Version=0.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 84C41D81-ABF0-404B-8A1F-077159C3B98C
// Assembly location: C:\Users\andre\Downloads\TrainYourTroops.dll

using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;


namespace TrainYourTroops
{
    internal class TroopTrainingBehaviour : CampaignBehaviorBase
    {
        private bool isTraining;
        private bool menuCreated;
        private int hoursTrained;
        private int totalXpForSession;
        private TroopTrainingOptions options = new TroopTrainingOptions();

        public bool IsTraining
        {
            get => this.isTraining;
            set => this.isTraining = value;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, new Action(this.HourlyTickEventAction));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<bool>("TroopTrainingBehaviour_isTrainingTroops", ref this.isTraining);
            dataStore.SyncData<bool>("TroopTrainingBehaviour_trainTroopsMenuCreated", ref this.menuCreated);
            dataStore.SyncData<int>("TroopTrainingBehaviour_totalXpForSession", ref this.totalXpForSession);
        }

        public void OnAfterNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            this.options = ModOptions.LoadOptions();
            this.AddTrainingMenus(campaignGameStarter);
        }

        public void HourlyTickEventAction()
        {
            if (!this.IsTraining)
                return;
            ++this.hoursTrained;
            this.TrainMainPartyTroops();
        }

        public void TrainMainPartyTroops()
        {
            CampaignTime now = CampaignTime.Now;
            TroopRoster memberRoster = MobileParty.MainParty.MemberRoster;
            int num = 0;
            int numberOfMen = MBRandom.RandomInt((memberRoster.TotalRegulars - memberRoster.TotalWoundedRegulars) * TroopTrainingOptions.Instance.ChanceToInjure / 100);
            bool flag = now.IsDayTime || TroopTrainingOptions.Instance.CanTrainAtNight;
            if (!flag)
                this.StopTraining(true);
            if (!flag || this.hoursTrained % 2 != 0)
                return;
            bool allTroopsUpgraded = true;
            foreach (TroopRosterElement troopRosterElement in memberRoster.GetTroopRoster())
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(TroopTrainingOptions.Instance.XPperTick);
                if (!((BasicCharacterObject)troopRosterElement.Character).IsHero && ((MBObjectBase)troopRosterElement.Character).IsReady)
                {
                    if (troopRosterElement.Xp / troopRosterElement.Number < troopRosterElement.Character.GetUpgradeXpCost(MobileParty.MainParty.Party, 0))
                    {
                        allTroopsUpgraded = false;
                        if (troopRosterElement.WoundedNumber == troopRosterElement.Number)
                            continue;

                        int xpAmount = MathF.Round(explainedNumber.ResultNumber * (float)(troopRosterElement.Number - troopRosterElement.WoundedNumber));
                        memberRoster.AddXpToTroop(xpAmount, troopRosterElement.Character);
                        num += xpAmount;
                        
                    }
                }
            }
            if (allTroopsUpgraded)
            {
                StopTraining(false);
                return;
            }
            if(numberOfMen <= memberRoster.TotalRegulars - memberRoster.TotalWoundedRegulars)
                memberRoster.WoundNumberOfTroopsRandomly(numberOfMen);
            
            InformationManager.DisplayMessage(new InformationMessage(string.Format("Trained for one hour and your troops gained {0} XP and {1} troops where wounded during training", (object)num, (object)numberOfMen)));
            this.totalXpForSession += num;
        }

        private void PayTrainingCost()
        {
            this.hoursTrained = 0;
            MobileParty.MainParty.Owner.Gold -= MobileParty.MainParty.TotalWage;
        }

        private void AddTrainingMenus(CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddGameMenuOption("town_arena", "town_arena_train_troops", "Train troops {TRAINING_COST}{GOLD_ICON}", game_menu_wait_here_on_condition, x => GameMenu.SwitchToMenu("town_arena_train_troops_wait_menus"), index: 1);
            campaignStarter.AddWaitGameMenu("town_arena_train_troops_wait_menus", "Your troops are training.", game_menu_arena_train_troops_wait_on_init, game_menu_arena_train_troops_wait_on_condition, null, null, GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, TroopTrainingOptions.Instance.CanTrainAtNight ? 0.0f : 22f - CampaignTime.Now.CurrentHourInDay);
            campaignStarter.AddGameMenuOption("town_arena_train_troops_wait_menus", "training_stop", "Stop Training", back_on_condition, game_menu_stop_training, true);
            this.menuCreated = true;
        }

        private void game_menu_stop_training(MenuCallbackArgs args) => this.StopTraining(false);

        private static bool CanMainHeroPartyTrain(
          Settlement settlement,
          SettlementAccessModel.SettlementAction settlementAction,
          out bool disableOption,
          out TextObject disabledText)
        {
            int num = !settlement.Town.HasTournament ? 0 : (Campaign.Current.IsDay ? 1 : 0);
            disableOption = false;
            disabledText = TextObject.Empty;
            if (!Campaign.Current.IsDay)
                return false;
            if (!Campaign.Current.IsMainHeroDisguised)
                return true;
            disableOption = true;
            disabledText = new TextObject("You cannot train your troops while disguised.", (Dictionary<string, object>)null);
            return false;
        }

        private void StopTraining(bool isNight)
        {
            this.IsTraining = false;
            PlayerEncounter.Current.IsPlayerWaiting = false;
            if (TroopTrainingOptions.Instance.MoralePenalty > 0)
                MobileParty.MainParty.RecentEventsMorale -= TroopTrainingOptions.Instance.MoralePenalty;
            GameMenu.SwitchToMenu("town_arena");
            string str = string.Format("You spent {0} hours training your troops and they gained a total of {1} XP", (object)this.hoursTrained, (object)this.totalXpForSession);
            if (isNight)
                str = string.Format("The arena is closed at night. You gained {1} XP in {0} hours of training.", (object)this.hoursTrained, (object)this.totalXpForSession);
            MBInformationManager.AddQuickInformation(new TextObject(str, (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, "");
        }

        private static bool game_menu_wait_here_on_condition(MenuCallbackArgs args)
        {
            bool disableOption;
            TextObject disabledText;
            bool canPlayerDo = TroopTrainingBehaviour.CanMainHeroPartyTrain(Settlement.CurrentSettlement, SettlementAccessModel.SettlementAction.WaitInSettlement, out disableOption, out disabledText);
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            MBTextManager.SetTextVariable("TRAINING_COST", MobileParty.MainParty.TotalWage);
            return MenuHelper.SetOptionProperties(args, canPlayerDo, disableOption, disabledText);
        }

        private void game_menu_arena_train_troops_wait_on_init(MenuCallbackArgs args)
        {
            if (!IsTraining)
            {
                this.totalXpForSession = 0;
                Campaign.Current.GameMenuManager.MenuLocations.Add((Settlement.CurrentSettlement == null ? MobileParty.MainParty.CurrentSettlement : Settlement.CurrentSettlement).LocationComplex.GetLocationWithId("arena"));
                if (PlayerEncounter.Current == null)
                    return;
                this.PayTrainingCost();
                this.IsTraining = true;
                PlayerEncounter.Current.IsPlayerWaiting = true;
            }
        }

        private static bool game_menu_arena_train_troops_wait_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
            return true;
        }

        private static bool back_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }
    }
}

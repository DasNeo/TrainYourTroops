// Decompiled with JetBrains decompiler
// Type: TrainYourTroops.TroopTrainingOptions
// Assembly: TrainYourTroops, Version=0.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 84C41D81-ABF0-404B-8A1F-077159C3B98C
// Assembly location: C:\Users\andre\Downloads\TrainYourTroops.dll

using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace TrainYourTroops
{
    public class TroopTrainingOptions : AttributeGlobalSettings<TroopTrainingOptions>
    {
        public override string Id => "TrainYourTroops";
        public override string DisplayName => "TrainYourTroops";
        public override string FormatType => "json";

        private int _xpPerTick = 10;
        private int _chanceToInjure = 8;
        private int _moralePenalty = 5;
        private bool _canTrainAtNight;

        [SettingPropertyInteger("XP per Tick", 1, 1000, RequireRestart = false)]
        public int XPperTick
        {
            get => this._xpPerTick;
            set
            {
                if (value <= 0)
                    return;
                this._xpPerTick = value;
            }
        }

        [SettingPropertyInteger("Chance to Injure", 0, 100, RequireRestart = false)]
        public int ChanceToInjure
        {
            get => this._chanceToInjure;
            set
            {
                if (value <= 0 || value >= 101)
                    return;
                this._chanceToInjure = value;
            }
        }

        [SettingPropertyInteger("Morale Penalty", 0, 50, RequireRestart = false)]
        public int MoralePenalty
        {
            get => this._moralePenalty;
            set
            {
                if (value <= 0 || value > 50)
                    return;
                this._moralePenalty = value;
            }
        }

        [SettingPropertyBool("Can train at night", RequireRestart = false)]
        public bool CanTrainAtNight
        {
            get => this._canTrainAtNight;
            set
            {
                if (value && !value)
                    return;
                this._canTrainAtNight = value;
            }
        }
    }
}

// Decompiled with JetBrains decompiler
// Type: TrainYourTroops.ModOptions
// Assembly: TrainYourTroops, Version=0.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 84C41D81-ABF0-404B-8A1F-077159C3B98C
// Assembly location: C:\Users\andre\Downloads\TrainYourTroops.dll

using Newtonsoft.Json;
using System;
using System.IO;
using TaleWorlds.Library;

namespace TrainYourTroops
{
    public class ModOptions
    {
        public static TroopTrainingOptions LoadOptions()
        {
            string path = Path.Combine(BasePath.Name, "Modules", "TrainYourTroops", "TrainYourTroopsOptions.json");
            TroopTrainingOptions troopTrainingOptions = (TroopTrainingOptions)null;
            try
            {
                string str = File.ReadAllText(path);
                if (str != null)
                    troopTrainingOptions = JsonConvert.DeserializeObject<TroopTrainingOptions>(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
            }
            return troopTrainingOptions ?? new TroopTrainingOptions();
        }
    }
}

using Artifact_Weapon_Arsenal.DamageSetting;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Artifact_Weapon_Arsenal
{
    [StaticConstructorOnStartup]
    public class AWAModSettingUI : Mod
    {
        public static AWAModSetting aWAModSetting;
        public static AWAModSettingUI Instance { get; private set; }
        public AWAModSettingUI(ModContentPack content) : base(content)
        {
            aWAModSetting = GetSettings<AWAModSetting>();//读取本地数据 设置setting中的mod关联
            Instance = this;
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            WeaponDamagePostProcessor.ApplyMeleeDamageChange();
        }

        public override string SettingsCategory()
        {
            return "AWAModSettingUITitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            Text.Font = GameFont.Small;
            listingStandard.Label("MeleeDamageSetting".Translate());
            listingStandard.Label(AWAModSetting.MeleeDamage.ToString());
            AWAModSetting.MeleeDamage = listingStandard.Slider(AWAModSetting.MeleeDamage, 0.50f, 10f);
            AWAModSetting.MeleeDamage = (float)Math.Round(AWAModSetting.MeleeDamage, 2);
            Text.Font = GameFont.Tiny;
            if (listingStandard.ButtonText("reset".Translate())) { aWAModSetting.InitData(); }
            listingStandard.End();
        }
    }
}

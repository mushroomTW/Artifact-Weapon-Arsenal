using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Artifact_Weapon_Arsenal
{
    [StaticConstructorOnStartup]
    internal class WeaponDamagePostProcessor
    {
        // 1. 儲存所有 AWA 武器工具的原始傷害值：鍵是 Tool 物件，值是原始的 Power
        private static Dictionary<Tool, float> OriginalToolPower = new Dictionary<Tool, float>();

        // 2. 定義您模組所有武器的 DefName 列表，用於精確篩選
        private static readonly string[] AWA_WeaponDefNames = new string[]
        {
            "katana",
            "Entei",
            "Dianqing"
            // 請確保這裡包含您所有要調整的近戰武器 DefName
        };

        // 靜態建構函式：在遊戲載入 Def 後執行一次
        static WeaponDamagePostProcessor()
        {
            InitializeOriginalDamage();
            // 在啟動時先應用一次傷害變更（使用預設或儲存的設定）
            ApplyMeleeDamageChange();
        }

        // 載入時，將原始傷害值儲存起來
        private static void InitializeOriginalDamage()
        {
            // 迭代所有物品定義 (ThingDef)
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                // 精確篩選：判斷 DefName 是否在我們定義的列表內
                //if (AWA_WeaponDefNames.Contains(thingDef.defName))
               // {
                    if (thingDef.tools != null)
                    {
                        foreach (Tool tool in thingDef.tools)
                        {
                            // 儲存原始傷害值。這裡的 tool 物件是 DefDatabase 中的實例。
                            if (tool.power > 0f)
                            {
                                OriginalToolPower.Add(tool, tool.power);
                            }
                        }
                    }
                //}
            }
        }

        // 核心方法：應用傷害調整，並從原始值開始計算
        public static void ApplyMeleeDamageChange()
        {
            // 迭代我們儲存的所有 (工具, 原始傷害) 配對
            foreach (KeyValuePair<Tool, float> entry in OriginalToolPower)
            {
                Tool tool = entry.Key;
                float originalDamage = entry.Value;

                // 呼叫 DamageChage 裡的靜態方法來計算調整後的傷害
                // 注意：這裡使用儲存的 originalDamage，而非 tool.power 的當前值
                float newDamage = DamageChage.GetMeleeDamage(originalDamage);

                // 將最終結果應用到 DefDatabase 中的 Tool 物件上
                tool.power = newDamage;

            }
        }
    }
}

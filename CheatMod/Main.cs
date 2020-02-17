using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;
using France.Game.model.role;
using System.IO;
using France.Game.model.level;
using France.Resource;
using France.Game.model;

namespace BotM
{
    static class Main
    {
        public static ModEntry mod;
        public static Settings _settings;

        static bool Load(UnityModManager.ModEntry modEntry)
        {

            mod = modEntry;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            _settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            _settings.Draw(modEntry);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            _settings.Save(modEntry);
        }

        [HarmonyPatch(typeof(Role))]
        [HarmonyPatch("gainExp")]
        class ExpMultiplier
        {
            static void Prefix(ref int exp)
            {
                exp = exp * _settings.ExpMultiplierInteger;
            }
        }

        [HarmonyPatch(typeof(Role))]
        [HarmonyPatch("UseWeapon")]
        class UseWeapon
        {
            static void Prefix(ref int count)
            {
                if (_settings.InfWeaponUsage)
                    count = 0;
            }
        }

        [HarmonyPatch(typeof(Role))]
        [HarmonyPatch("buySkill")]
        class buySkill
        {
            static void Prefix(Role __instance, int skillId)
            {
                if (!_settings.InfSkillBuy)
                    return;
                if (__instance.isCanBuySkill(skillId) != 0)
                {
                    return;
                }
                Skill skill = new Skill(skillId);
                SkillData data = skill.getData();
                int creditType = (int)data.learnParams[2];
                int num2 = (int)data.learnParams[3];
                PlayerData.ModifyCreditPoint(creditType, num2);

            }
        }

        [HarmonyPatch(typeof(Role))]
        [HarmonyPatch("promotionClass")]
        class promotionClass
        {
            static void Prefix(Role __instance, int classId)
            {
                if (!_settings.InfPromoteBuy)
                    return;
                CharacterClassData classInfo = __instance.getClassInfo();
                if (__instance.lv < classInfo.maxLevel)
                {
                    return;
                }
                int i = 0;
                while (i < classInfo.promotionDatas.Count)
                {
                    if (classInfo.promotionDatas[i].promotionClassId == classId)
                    {
                        ClassPromotionData classPromotionData = classInfo.promotionDatas[i];
                        if (PlayerData.Money < (long)classPromotionData.money || PlayerData.GetCreditPoint(classPromotionData.creditType) < classPromotionData.creditPoint)
                        {
                            return;
                        }
                        PlayerData.ModifyCreditPoint(classPromotionData.creditType, classPromotionData.creditPoint);
                    }
                    else
                    {
                        i++;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(GFightUnit))]
        [HarmonyPatch("useItem")]
        class UseItem
        {
            static void Prefix(GFightUnit __instance, int index)
            {
                if (!_settings.InfItemUsage)
                    return;
                var role = __instance.getRole();
                var item = role.items[index];
                if (item.getItemType() == Item.ITEM_TYPE.ITEM)
                    item.count += 1;

            }
        }

        [HarmonyPatch(typeof(Role))]
        [HarmonyPatch("lvUp")]
        class LevelUp
        {
            static void Prefix(Role __instance, out float[] __state)
            {
                __state = new float[7] { __instance.hpMax, __instance.attack, __instance.defence[0], __instance.defence[1], __instance.dexterity, __instance.speed, __instance.luck };
            }

            static void Postfix(Role __instance, float[] __state)
            {
                var minAmount = new int[7] { _settings.MinHPGrowth, _settings.MinAttackGrowth, _settings.MinDef1Growth, _settings.MinDef2Growth, _settings.MinDexterityGrowth, _settings.MinSpeedGrowth, _settings.MinLuckGrowth };
                var localState = new float[7] { __instance.hpMax, __instance.attack, __instance.defence[0], __instance.defence[1], __instance.dexterity, __instance.speed, __instance.luck };
                for (int i = 0; i < 7; i++)
                {
                    if (__state[i] - localState[i] < minAmount[i])
                        localState[i] = __state[i] + minAmount[i];
                }
                __instance.hpMax = localState[0];
                __instance.attack = localState[1];
                __instance.defence[0] = localState[2];
                __instance.defence[1] = localState[3];
                __instance.dexterity = localState[4];
                __instance.speed = localState[5];
                __instance.luck = localState[6];
            }
        }
    }

    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Header("Exp Multiplier Mod")]
        [Draw("Exp Multplier", Precision = 0, Min = 0), Space(5)] 
        public int ExpMultiplierInteger = 1;

        [Header("Minimum Stat Growth Mod")]
        [Draw("Min HP Growth", Precision = 0, Min = 0), Space(5)]
        public int MinHPGrowth = 0;
        [Draw("Min Attack Growth", Precision = 0, Min = 0), Space(5)]
        public int MinAttackGrowth = 0;
        [Draw("Min Def1 Growth", Precision = 0, Min = 0), Space(5)]
        public int MinDef1Growth = 0;
        [Draw("Min Def2 Growth", Precision = 0, Min = 0), Space(5)]
        public int MinDef2Growth = 0;
        [Draw("Min Dexterity Growth", Precision = 0, Min = 0), Space(5)]
        public int MinDexterityGrowth = 0;
        [Draw("Min Speed Growth", Precision = 0, Min = 0), Space(5)]
        public int MinSpeedGrowth = 0;
        [Draw("Min Luck Growth", Precision = 0, Min = 0), Space(5)]
        public int MinLuckGrowth = 0;

        [Header("Item Usage Mod")]
        [Draw("Infinite Item Usage")] public bool InfItemUsage = false;
        [Draw("Infinite Weapon Usage")] public bool InfWeaponUsage = false;
        [Draw("Infinite Credits to Buy Skills")] public bool InfSkillBuy = false;
        [Draw("Infinite Credits to Promote")] public bool InfPromoteBuy = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
        }
    }
}
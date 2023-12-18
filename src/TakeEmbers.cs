using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace Fire_RV
{
    internal class TakeEmbers
    {
        //internal static void AddFeedFireActions(List<Panel_ActionPicker.ActionPickerItemData> list)
        //{
        //    list.Insert(2, new Panel_ActionPicker.ActionPickerItemData("ico_skills_fireStarting", "GAMEPLAY_TakeEmbers", new Action  ExecuteTakeEmbers));
        //    //list.Insert(1, new Panel_ActionPicker.ActionPickerItemData("ico_forge", "Blow", ExecuteBlow));
        //}

        internal static void ExecuteTakeEmbers()
        {
            GearItem emberBox = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_EmberBox");
            if (emberBox == null)
            {
                GameAudioManager.PlayGUIError();
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_ToolRequiredToForceOpen").Replace("{item-name}", Localization.Get("GAMEPLAY_EmberBox")), false);
                return;
            }

            Panel_FeedFire panel = InterfaceManager.m_Panel_FeedFire;
            Fire fire = Traverse.Create(panel).Field("m_Fire").GetValue<Fire>();
            if (fire && !fire.m_IsPerpetual)
            {
                fire.ReduceHeatByDegrees(1);
            }

            GameManager.GetInventoryComponent().DestroyGear(emberBox.gameObject);
            GearItem activeEmberBox = GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory("GEAR_ActiveEmberBox");
            GearMessage.AddMessage(activeEmberBox, Localization.Get("GAMEPLAY_Harvested"), activeEmberBox.m_DisplayName, false);

            InterfaceManager.m_Panel_FeedFire.ExitFeedFireInterface();
        }
        public static void MayApplychanges(GameObject go)
        {
            GearItem gearitem = go.GetComponent<GearItem>();
            var setting = Fire_RVSettings.Settings.options;         
            if (setting != null) gearitem.m_DailyHPDecay = 24f / setting.Embersboxduration;
        }
    }
}
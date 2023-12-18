using Harmony;
using UnityEngine;
using System.Collections.Generic;
using System;











/*

 * 

 * 
 *   MOD Fire
 *   
 *   
 *   >>>Fire heat retention / Add indoor space temperature that decays over time
 *   >>> shieldheart's idea, but...  I want heat source to represent the resevoir of heat left in a space after a fire. 
 *   >>really I want a situation where the fire says 6 degrees, but the air temperature goes above 6
 *   not linear decay but proportional to temp differential between inside and outside modified by "insulation" factor
 *   
 *   each heat source needs a heat reservoir, with a temperature and decay factor.
 *   decay factor is a combintion of reservoir size and insulation, but bigger 
 *   reservoirs are practically the same as more insulation as we only care
 *   about how much "temperature" is leaving the system (not how much heat energy)
 *   not true. a big reservoir will gain heat slower than good insulation...
 *   
 *   reservoir temp+= fire_inc_pm + (indoor_final-reservoir_temp)*ins
 *   
 *   
 *   Toilet water frozen if local temperature below freezing
 *   >>>fix bug warmth lantern in hand switched off
 *   fix fire must bring air temperature above 0 to defrost carcass [9]
 *  

 */


namespace Fire_RV
{

    [HarmonyPatch(typeof(GameManager), "SetAudioModeForLoadedScene")]
    internal class GameManager_SetAudioModeForLoadedScene
    {
        internal static void Postfix()
        {
            if (!(GameManager.m_ActiveScene == "MainMenu"))
            {
                Fire_RV.scene_loading = false;
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "LoadSceneWithLoadingScreen")]
    internal class GameManager_LoadSceneWithLoadingScreen
    {
        internal static void Postfix()
        {
            Fire_RV.scene_loading = true;
        }
    }


    //code "borrowed" straight from MikeP
    [HarmonyPatch(typeof(BreakDown), "Start")]
    class BreakDown_Start
    {
        static void Postfix(BreakDown __instance)
        {
            // Branches take 5 mins instead of 10
            if (__instance.m_DisplayName == "Branch")
            {
                __instance.m_TimeCostHours /= 2f;
            }

            // Limbs take 30 mins base (15 mins with hatchet) instead of 90 mins base (45 mins with hatchet)
            if (__instance.m_DisplayName.EndsWith("Limb"))
            {
                __instance.m_TimeCostHours /= 3f;
            }
        }
    }

    [HarmonyPatch(typeof(Fire), "GetWeatherAdjustedElapsedDuration")]

    internal class Fire_GetWeatherAdjustedElapsedDuration
    {
        private static bool Prefix(Fire __instance, float realtimeSeconds, ref float __result)
        {
            __result = GameManager.GetTimeOfDayComponent().GetTODSeconds(realtimeSeconds);
            return false;
        }
    }




    [HarmonyPatch(typeof(Fire), "TurnOn")]

    internal class Fire_TurnOn
    {
        private static bool Prefix(Fire __instance, FuelSourceItem fuel, bool maskTempIncrease)
        {
            GearItem component = fuel.GetComponent<GearItem>();
            if (!component)
            {
                return false;
            }
            //Fire_RV.PrintHierarchy(__instance.gameObject);



            __instance.m_HeatSource.TurnOn();
            __instance.m_HeatSource.m_MaskTempIncrease = maskTempIncrease;
            __instance.m_HeatSource.m_MaxTempIncrease += Fire_RV.getModifiedHeatIncrease(component);
            __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = fuel.m_HeatInnerRadius;

            float outradscale = __instance.GetFireOuterRadiusScale();      
            HeatReservoir reservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));
            if (reservoir == null) __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = fuel.m_HeatOuterRadius * outradscale;
            else __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = fuel.m_HeatOuterRadius * outradscale + reservoir.heatingsize;
            if (__instance.m_FX)
            {
                __instance.m_FX.TriggerStage(__instance.m_FireState, true, true);
            }
            __instance.m_FuelHeatIncrease = __instance.m_HeatSource.m_MaxTempIncrease;
            __instance.m_ElapsedOnTODSeconds = 0f;
            __instance.m_ElapsedOnTODSecondsUnmodified = 0f;
            __instance.m_MaxOnTODSeconds = Fire_RV.getStoveDurationModifier(__instance.gameObject) * Fire_RV.getModifiedDuration(component) * 60f * 60f;
            return false;
        }
    }




    [HarmonyPatch(typeof(FuelSourceItem), "GetModifiedBurnDurationHours")]

    internal class FuelSourceItem_GetModifiedBurnDurationHours
    {
        private static bool Prefix(FuelSourceItem __instance, float normalizedCondition, ref float __result)
        {
            GearItem component = __instance.GetComponent<GearItem>();
            if (!component)
            {
                return false;
            }
            __result = Fire_RV.getModifiedDuration(component);
            return false;

        }
    }


    [HarmonyPatch(typeof(Fire), "AddFuel")]

    internal class Fire_AddFuel
    {
        private static bool Prefix(Fire __instance, GearItem fuel, bool inForge)
        {
            //   Debug.Log("Fire:" + __instance.name);
            //Fire_RV.PrintHierarchy(__instance.gameObject);
            __instance.OnFuelBurnt(fuel);
            if (__instance.IsEmbers())
            {
                __instance.m_FX.TriggerStage(FireState.FullBurn, true, false);
                __instance.m_HeatSource.TurnOn();
            }
            if (__instance.m_MaxOnTODSeconds < __instance.m_ElapsedOnTODSeconds) __instance.m_MaxOnTODSeconds = __instance.m_ElapsedOnTODSeconds;


            float old_time_remaining = __instance.GetRemainingLifeTimeSeconds();
            float old_max_tod = __instance.m_MaxOnTODSeconds;
            float num = Fire_RV.getStoveDurationModifier(__instance.gameObject) * fuel.m_FuelSourceItem.GetModifiedBurnDurationHours(fuel.GetNormalizedCondition()) * 60f * 60f;
            if (!__instance.m_IsPerpetual)
            {
                __instance.m_MaxOnTODSeconds += num;
                float num2 = __instance.m_MaxOnTODSeconds - GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire * 60f * 60f;
                if (num2 > 0f)
                {
                    __instance.m_ElapsedOnTODSeconds -= num2;
                    __instance.m_ElapsedOnTODSeconds = Mathf.Clamp(__instance.m_ElapsedOnTODSeconds, 0f, float.PositiveInfinity);
                }
                __instance.m_MaxOnTODSeconds = Mathf.Clamp(__instance.m_MaxOnTODSeconds, 0f, GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire * 60f * 60f);
            }
            float num3;
            if (inForge && fuel.m_FuelSourceItem.m_FireAgeMinutesBeforeAdding > 0f)
            {
                num3 = GameManager.GetFireManagerComponent().m_MaxHeatIncreaseOfFireInForge - __instance.m_FuelHeatIncrease;
            }
            else
            {
                num3 = GameManager.GetFireManagerComponent().m_MaxHeatIncreaseOfFire - __instance.m_FuelHeatIncrease;
            }
            num3 = Mathf.Clamp(num3, 0f, float.PositiveInfinity);

            float fuel_heat_increase = Mathf.Min(num3, Fire_RV.getModifiedHeatIncrease(fuel));
            float fuel_duration_increase = __instance.GetRemainingLifeTimeSeconds() - old_time_remaining;
            __instance.m_MaxOnTODSeconds = old_max_tod;


            float degmins_old = old_time_remaining * __instance.m_FuelHeatIncrease;
            float degmins_fuel = fuel_duration_increase * fuel_heat_increase;
            float degmins_total = degmins_fuel + degmins_old;

            float shape_heat = __instance.m_FuelHeatIncrease + fuel_heat_increase;
            float shape_duration = old_time_remaining + fuel_duration_increase;


            float correction = Mathf.Sqrt(degmins_total / (shape_heat * shape_duration));

            Debug.Log("Old Fire Duration:" + old_time_remaining / 60f + " Fire temp:" + __instance.m_FuelHeatIncrease);
            Debug.Log("Fuel Item:" + fuel.name + " Heat:" + fuel_heat_increase + " Duration:" + fuel_duration_increase / 60f);
            Debug.Log("degmins total:" + degmins_total + " reshaped degmins:" + (shape_heat * correction * shape_duration * correction));

            __instance.m_FuelHeatIncrease = shape_heat * correction;
            __instance.m_MaxOnTODSeconds = __instance.m_ElapsedOnTODSeconds + shape_duration * correction;

            __instance.m_HeatSource.m_MaxTempIncrease = __instance.m_FuelHeatIncrease;
            __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = Mathf.Max(fuel.m_FuelSourceItem.m_HeatInnerRadius, __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius);

            float fire_outer = __instance.GetFireOuterRadiusScale();

            HeatReservoir reservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));

            float extrasize = 0;
            if (reservoir != null)
            {
                reservoir.heatingsize += fuel.m_FuelSourceItem.m_HeatOuterRadius * fire_outer;
                extrasize = reservoir.heatingsize;
            }
            //if (!GameManager.GetWeatherComponent().IsIndoorEnvironment()) __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = Mathf.Clamp(reservoir.heatingsize, 5f, 25f);

            __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = extrasize;  //Mathf.Max(fuel.m_FuelSourceItem.m_HeatOuterRadius * fire_outer, __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius);



            __instance.m_FX.TriggerFlareupLarge();

            return false;
        }
    }

    [HarmonyPatch(typeof(Fire), "CanTakeTorch")]

    internal class Fire_CanTakeTorch
    {
        private static bool Prefix(Fire __instance, ref bool __result)
        {
            if (__instance.m_IsPerpetual)
            {
                __result = false;
                return false;
            }
            float fire_temp = __instance.m_FuelHeatIncrease;
            float fire_time = __instance.GetRemainingLifeTimeSeconds();
            float remaining_fire_cs = fire_time * fire_temp;
            __result = remaining_fire_cs > (Fire_RV.torch_cost(__instance));
            return false;
        }
    }

    //[HarmonyPatch(typeof(Panel_ActionPicker), "ShowFeedFireActionPicker")]
    //internal class Panel_ActionPicker_ShowFeedFireActionPicker
    //{
    //    private static bool Prefix(Panel_ActionPicker __instance, GameObject objectInteractedWith)

    //    {

    //        Fire componentInChildren = objectInteractedWith.GetComponentInChildren<Fire>();
    //        Fire_RV.currentFire = componentInChildren;
    //        if (componentInChildren && componentInChildren.IsBurning() && GameManager.GetPlayerManagerComponent().PlayerHoldingTorchThatCanBeLit())
    //        {
    //            InterfaceManager.m_Panel_TorchLight.SetFireSourceToLightTorch(componentInChildren);
    //            InterfaceManager.m_Panel_TorchLight.Enable(true);
    //            return false;
    //        }

    //        __instance.m_ActionPickerItemDataList.Clear();
    //        __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_feed_fire", "GAMEPLAY_AddFuel", new Action(() => __instance.FireInteractCallback())));

    //        if (componentInChildren && Fire_RV.canStokeFire(componentInChildren))
    //        {

    //            __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_campFireProgress", "Heap " + GetEmberStateAsText(componentInChildren), new Action(Fire_RV.tryHeapFire)));
    //        }
    //        if (componentInChildren && Fire_RV.canSpreadFire(componentInChildren))
    //        {
    //            __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_campFireProgress", "Spread " + GetEmberStateAsText(componentInChildren), new Action(Fire_RV.trySpreadFire)));
    //        }

    //        if (componentInChildren && Fire_RV.canbreakdownFire(componentInChildren))
    //        {

    //            __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_campFireProgress", "Breakdown", new Action(Fire_RV.tryBreakdownFire)));
    //        }

    //        if (componentInChildren && componentInChildren.CanTakeTorch())
    //        {
    //            __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_torch", "GAMEPLAY_TakeTorch", new Action(() => __instance.TakeTorchCallback())));
    //        }

    //       // __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_cooking_pot", "GAMEPLAY_Cook", new Action(() => AccessTools.Method(typeof(Panel_ActionPicker), "FireCookCallback").Invoke(__instance, null))));
    //       // __instance.m_ActionPickerItemDataList.Add(new Panel_ActionPicker.ActionPickerItemData("ico_water_prep", "GAMEPLAY_Water", new Action(() => AccessTools.Method(typeof(Panel_ActionPicker), "FireWaterCallback").Invoke(__instance, null))));
    //        __instance.m_ObjectInteractedWith = objectInteractedWith;


    //        __instance.EnableWithCurrentList();


    //        return false;
    //    }

    //    private static string GetEmberStateAsText(Fire componentInChildren)
    //    {
    //        var text = "Fire";
    //        if (componentInChildren.IsEmbers()) text = "Embers";
    //        return text;
    //    }
    //}
    ////ok
    //[HarmonyPatch(typeof(Panel_ActionPicker), "Start")]
    //internal class Panel_ActionPicker_Start
    //{
    //    private static void Postfix(Panel_ActionPicker __instance)
    //    {
    //        Debug.Log("Panel_ActionPicker");

    //        ActionPickerItem[] newList = __instance.m_ActionPickerItemList;
    //        newList[0] = __instance.m_ActionPickerItemList[0];
    //        newList[1] = __instance.m_ActionPickerItemList[1];
    //        newList[2] = __instance.m_ActionPickerItemList[2];
    //        newList[3] = __instance.m_ActionPickerItemList[3];
    //        newList[4] = __instance.m_ActionPickerItemList[4];
    //        newList[5] = NGUITools.AddChild(__instance.gameObject, newList[4].gameObject).GetComponent<ActionPickerItem>();
    //        newList[5].gameObject.transform.position = newList[0].gameObject.transform.position;
    //        newList[6] = NGUITools.AddChild(__instance.gameObject, newList[4].gameObject).GetComponent<ActionPickerItem>();
    //        newList[6].gameObject.transform.position = newList[0].gameObject.transform.position;
    //        newList[7] = NGUITools.AddChild(__instance.gameObject, newList[4].gameObject).GetComponent<ActionPickerItem>();
    //        newList[7].gameObject.transform.position = newList[0].gameObject.transform.position;
    //        __instance.m_ActionPickerItemList = newList;
    //    }
    //}



    [HarmonyPatch(typeof(Panel_FeedFire), "OnTakeTorch")]
    internal class Panel_FeedFire_OnTakeTorch
    {
        private static bool Prefix(Panel_FeedFire __instance)

        {

            if ((bool)__instance.ProgressBarIsActive() || !__instance.m_Fire)
            {
                GameAudioManager.PlayGUIError();
                return false;
            }
            if (!__instance.m_Fire.CanTakeTorch())//then we can't take torch
            {
                GameAudioManager.PlayGUIError();
                return false;
            }
            GearItem gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory("GEAR_Torch");
            if (gearItem == null || gearItem.m_TorchItem == null)
            {
                return false;
            }
            gearItem.OverrideGearCondition(GearStartCondition.Low, false);
            gearItem.m_TorchItem.m_ElapsedBurnMinutes = Mathf.Clamp01(1f - gearItem.m_CurrentHP / gearItem.m_MaxHP) * gearItem.m_TorchItem.GetModifiedBurnLifetimeMinutes();
            GameManager.GetPlayerAnimationComponent().MaybeDropAndResetCurrentWeapon();
            gearItem.m_TorchItem.Ignite();
            GameManager.GetPlayerManagerComponent().EquipItem(gearItem, false);
            GameManager.GetPlayerManagerComponent().ClearRestoreItemInHandsAfterInteraction();
            if (!__instance.m_Fire.m_IsPerpetual)
            {
                float torchcost_cs = Fire_RV.torch_cost(__instance.m_Fire);
                float fire_temp = __instance.m_Fire.m_FuelHeatIncrease;
                float fire_time = __instance.m_Fire.GetRemainingLifeTimeSeconds();
                float remaining_fire_cs = fire_time * fire_temp;
                float new_fire_cs = (remaining_fire_cs - torchcost_cs);
                float myfactor = Mathf.Pow(new_fire_cs / remaining_fire_cs, 0.5f);
                float time_reduction = fire_time - (fire_time * myfactor);
                float heat_reduction = fire_temp - (fire_temp * myfactor);
                Debug.Log("new_cs:" + new_fire_cs + " ratio::" + (new_fire_cs / remaining_fire_cs) + " myfactor::" + myfactor + " timered::" + time_reduction + " heat_red:" + heat_reduction);
                __instance.m_Fire.ReduceDurationByTODSeconds(time_reduction);
                __instance.m_Fire.ReduceHeatByDegrees(heat_reduction);

                fire_temp = __instance.m_Fire.m_FuelHeatIncrease;
                fire_time = __instance.m_Fire.GetRemainingLifeTimeSeconds();
                Debug.Log("Reducing Fire: Old cs:" + remaining_fire_cs + " New cs:" + fire_temp * fire_time);
            }
            __instance.ExitFeedFireInterface();
            return false;
        }
    }



    //just a little fix to make harvesting sticks from torches less painful

   [HarmonyPatch(typeof(Panel_Inventory_Examine), "OnHarvest")]
    internal class Panel_Inventory_Examine_OnHarvest
    {
        private static bool Prefix(Panel_Inventory_Examine __instance)
        {
            if (GameManager.GetWeatherComponent().IsTooDarkForAction(ActionsToBlock.Harvest))
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_RequiresLightToHarvest"), false);
                GameAudioManager.PlayGUIError();
                return false;
            }
            Harvest component = __instance.m_GearItem.GetComponent<Harvest>();
            Debug.Log("gear:" + component.m_YieldGear[0].name);
            if (component.m_YieldGear[0].name == "GEAR_Stick") __instance.m_HarvestTimeSeconds /= 6.0f;
            __instance.StartHarvest();
            if (component.m_YieldGear[0].name == "GEAR_Stick") __instance.m_HarvestTimeSeconds *= 6.0f;
            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "OnFeedFire")]
    internal class Panel_FeedFire_OnFeedFire
    {
        private static bool Prefix(Panel_FeedFire __instance)
        {
            if ((bool)__instance.ProgressBarIsActive())
            {
                GameAudioManager.PlayGUIError();
                return false;
            }
            GearItem selectedFuelSource = (GearItem)__instance.GetSelectedFuelSource();
            if (selectedFuelSource == null)
            {
                GameAudioManager.PlayGUIError();
                return false;
            }
            FuelSourceItem fuelSourceItem = selectedFuelSource.m_FuelSourceItem;
            if (fuelSourceItem == null)
            {
                GameAudioManager.PlayGUIError();
                return false;
            }
            if (!__instance.m_FireContainer)
            {
                GameAudioManager.PlayGUIError();
                return false;
            }
            if (!__instance.m_Fire)
            {
                return false;
            }
            if (__instance.m_Fire.FireShouldBlowOutFromWind())
            {
                // HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooWindyToAddFuel"), false);
                GameAudioManager.PlayGUIError();
                return false;
            }
            bool flag = true;
            if ((bool)__instance.FireInForge() && fuelSourceItem.m_FireAgeMinutesBeforeAdding > 0f)
            {
                flag = false;
            }
            flag = false; //allow any duration of fire
            if (flag)
            {
                float num = fuelSourceItem.GetModifiedBurnDurationHours(selectedFuelSource.GetNormalizedCondition()) * 60f;
                float num2 = __instance.m_Fire.GetRemainingLifeTimeSeconds() / 60f;
                float num3 = (num + num2) / 60f;
                if (num3 > GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire)
                {
                    GameAudioManager.PlayGUIError();
                    //HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotAddMoreFuel"), false);
                    return false;
                }

            }
            int num4 = Mathf.RoundToInt(fuelSourceItem.m_FireAgeMinutesBeforeAdding - __instance.m_Fire.GetUnmodifiedMinutesBurning() - 10);
            if (num4 >= 1)
            {
                //string text = Localization.Get("GAMEPLAY_BurnTimeNeededForCoal");
                //text = text.Replace("{minutes}", num4.ToString());
                //HUDMessage.AddMessage(text, false);
                GameAudioManager.PlayGUIError();
                return false;
            }
            //if (selectedFuelSource.m_ResearchItem && !selectedFuelSource.m_ResearchItem.IsResearchComplete())
            //{
            //    __instance.m_ResearchItemToBurn = selectedFuelSource;
            //    ForceBurnResearchItem forceBurnResearchItem = () => __instance.ForceBurnResearchItem();
            //    InterfaceManager.m_Panel_Confirmation.ShowBurnResearchNotification(()forceBurnResearchItem) ;// AccessTools.Method(typeof(Panel_FeedFire), "ForceBurnResearchItem", null, null).Invoke(__instance, null); });
            //    return false;
            //}
            if (selectedFuelSource == GameManager.GetPlayerManagerComponent().m_ItemInHands)
            {
                GameManager.GetPlayerManagerComponent().UnequipItemInHandsSkipAnimation();
            }
            if (selectedFuelSource == GameManager.GetPlayerManagerComponent().m_PickupGearItem)
            {
                GameManager.GetPlayerManagerComponent().ResetPickup();
            }
            GameAudioManager.PlaySound(__instance.m_FeedFireAudio, InterfaceManager.GetSoundEmitter());
            __instance.m_Fire.AddFuel(selectedFuelSource, __instance.FireInForge());
            GameManager.GetPlayerManagerComponent().ConsumeUnitFromInventory(fuelSourceItem.gameObject);
            return false;
        }
    }
    public delegate void ForceBurnResearchItem();




    [HarmonyPatch(typeof(Fire), "MaybeBlowOutFromWind")]
    internal class Fire_MaybeBlowOutFromWind
    {
        private static bool Prefix(Fire __instance)
        {
            var setting = Fire_RVSettings.Settings.options;

            if (__instance.m_Campfire && !__instance.m_Campfire.CanFeedFire())
            {
                return false;
            }
            if (!setting.WindReworked)
            {
                if (__instance.FireShouldBlowOutFromWind())
                {

                    float num = Mathf.Clamp(__instance.m_MaxOnTODSeconds - __instance.m_ElapsedOnTODSeconds, 0f, float.PositiveInfinity);
                    float safezone = GameManager.GetFireManagerComponent().m_TODMinutesFadeOutFireAudio * 60f;

                    if (num > safezone)
                    {
                        Fire_RV.breakdownFire(__instance);
                    }
                }
            }
            else
            {
                if (!__instance.m_Campfire) return false;

                Vector3 position = __instance.transform.position;
                position.y += 1f;
                if (!GameManager.GetWindComponent().IsPositionOccludedFromWind(position))
                {

                    if (Fire_RV.ReworkedFireBlowOut(__instance.m_MaxOnTODSeconds, __instance.m_ElapsedOnTODSeconds, __instance.GetCurrentTempIncrease())) Fire_RV.breakdownFire(__instance);
                }
            }


            return false;
        }
    }

    [HarmonyPatch(typeof(Fire), "FireShouldBlowOutFromWind")]
    internal class Fire_FireShouldBlowOutFromWind
    {
        private static bool Prefix(Fire __instance, ref bool __result)
        {
            var setting = Fire_RVSettings.Settings.options;

            if (!setting.WindReworked) return true;
            if (!__instance.m_Campfire) return true;


            Vector3 position = __instance.transform.position;
            position.y += 1f;

            if (!Fire_RV.ReworkedFireBlowOut(__instance.m_MaxOnTODSeconds, __instance.m_ElapsedOnTODSeconds, __instance.GetCurrentTempIncrease())) return false;
            else if (!GameManager.GetWindComponent().IsPositionOccludedFromWind(position))
            {
                __result = true;

            }
            return true;


        }
    }

    [HarmonyPatch(typeof(Weather), "GetDebugWeatherText")]
    internal class Weather_GetDebugWeatherText
    {
        private static void Postfix(ref string __result)
        {

            Vector3 pos = GameManager.GetPlayerTransform().position;


            Fire result = null;
            float num = float.PositiveInfinity;
            for (int i = 0; i < FireManager.m_Fires.Count; i++)
            {
                Fire firem = FireManager.m_Fires[i];
                float num2 = Vector3.Distance(pos, firem.transform.position);
                if (num2 < num)
                {
                    num = num2;
                    result = firem;

                }
            }
            Fire fire = result;
            if (fire == null) return;

            HeatSource myheat = fire.m_HeatSource;
            float maxTemp = myheat.m_MaxTempIncrease;
            float innerRad = myheat.m_MaxTempIncreaseInnerRadius;
            float outerRad = myheat.m_MaxTempIncreaseOuterRadius; 
            float dist = Vector3.Distance(pos, fire.transform.position);
            FireState mystate = fire.m_FireState;
            if (dist > 20) return;
            __result += "\n\nFire  state:" + Enum.GetName(typeof(FireState), mystate) + " dist:" + string.Format("{0:0.0}", dist) + " >>Heat  temp:" + string.Format("{0:0.00}", myheat.GetCurrentTempIncrease()) + " max:" + maxTemp + " isembers:" + fire.IsEmbers();
            __result += "\nRadius, Inner:" + innerRad + " Outer:" + outerRad;
            HeatReservoir myres = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(fire.gameObject));
            if (myres == null)
            {
                // Debug.Log("no heat reservoir associated with fire");
                return;
            }
            __result += "\nRsvr temp:" + string.Format("{0:0.00}", myres.temp) + " size:" + myres.size_cmins + " ins:" + myres.insulation_factor + " LastFireTemp:" + string.Format("{0:0.00}", myres.lastFireTemp);

        }
    }

    [HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData")]
    internal class SaveGameSystemPatch_RestoreGlobalData
    {
        internal static void Postfix(string name)
        {
            Fire_RV.LoadData(name);
        }
    }

    [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData")]
    internal class SaveGameSystemPatch_SaveGlobalData
    {
        public static void Postfix(SaveSlotType gameMode, string name)
        {
            Fire_RV.SaveData(gameMode, name);
        }
    }



    [HarmonyPatch(typeof(Fire), "Deserialize")]
    internal class Fire_Deserialize
    {
        private static void Prefix(Fire __instance, string text)
        {
            __instance.m_EmberDurationSecondsTOD = float.PositiveInfinity;                     // may not be beautiful but a simple way to prevent the game of turning of fires on Deserialize

        }
        private static void Postfix(Fire __instance, string text)
        {


            HeatReservoir myreservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));

            if (myreservoir == null) return;
            FireSaveDataProxy fireSaveDataProxy = Utils.DeserializeObject<FireSaveDataProxy>(text);

            float newelapsedtime = fireSaveDataProxy.m_ElapsedOnTODSecondsProxy + (GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() - fireSaveDataProxy.m_HoursPlayed) * 60 * 60;

            float maxburn = fireSaveDataProxy.m_MaxOnTODSecondsProxy;

            if (newelapsedtime > maxburn)
            {
                //fire went out inbetween. log minutes ago into reservoir.
                myreservoir.fireLastOnAt = Fire_RV.unitime() - (newelapsedtime - maxburn) / (60 * 60 * 24);
            }

        }
    }


    //[HarmonyPatch(typeof(Fire), "Update")]
    //internal class Fire_Update_Prefix
    //{
    //    private static bool Prefix(Fire __instance)
    //    {

    //        var setting = Fire_RVSettings.Settings.options;

    //        if (Fire_RV.scene_loading || GameManager.m_IsPaused) return false;

    //        if (__instance.m_FireState == FireState.Off)
    //        {

    //            HeatReservoir reservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));             //we have to update 

    //            if (reservoir != null)
    //            {
    //                if (__instance.m_HeatSource == null)
    //                {

    //                    __instance.m_HeatSource = new HeatSource();
    //                    GameManager.GetHeatSourceManagerComponent().AddHeatSource(__instance.m_HeatSource);
    //                }
    //                if (!GameManager.GetHeatSourceManagerComponent().m_HeatSources.Contains(__instance.m_HeatSource))
    //                {

    //                    GameManager.GetHeatSourceManagerComponent().AddHeatSource(__instance.m_HeatSource);
    //                }
    //                reservoir.Update(__instance);
    //                __instance.m_HeatSource.m_MaxTempIncrease = reservoir.temp;
    //                __instance.m_HeatSource.m_TempIncrease = reservoir.temp;
    //                if (reservoir.temp < 0.05f) Fire_RV.RemoveReservoir(reservoir.GUID);


    //                __instance.m_HeatSource.m_MaskTempIncrease = false;
    //            }

    //            Utils.SetActive(__instance.m_FX.lighting.gameObject, false);
    //            __instance.m_FX.DeactivateAllFX();
    //            // this.m_TimeOffSeconds = float.NegativeInfinity;
    //            //}
    //            return false;
    //        }

    //        __instance.UpdateFireStage();
    //        if (!__instance.m_IsPerpetual)
    //        {

    //            __instance.MaybeBlowOutFromWind();

    //            float deltaTime = GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);
    //            float f = 1;
    //            if ((bool)__instance.m_Campfire)
    //            {


    //                Vector3 position = __instance.transform.position;
    //                position.y += 1f;
    //                if (!GameManager.GetWindComponent().IsPositionOccludedFromWind(position) && setting.WindReworked)
    //                {
    //                    float relativwind = GameManager.GetWindComponent().GetSpeedMPH() / GameManager.GetFireManagerComponent().m_WindSpeedThatBlowsOutFires;
    //                    float temp = __instance.m_HeatSource.m_TempIncrease;

    //                    f *= (1 + relativwind);

    //                    temp /= (1 + relativwind * deltaTime / 60f);
    //                    __instance.m_HeatSource.m_TempIncrease = temp;
                        
    //                }
    //            }
    //            __instance.m_ElapsedOnTODSecondsUnmodified += deltaTime;
    //            __instance.m_ElapsedOnTODSeconds += deltaTime * f;
    //        }

    //        __instance.UpdateFireAudio();

    //        bool fireOn = __instance.m_HeatSource.IsTurnedOn();

    //        //if (!fireOn || __instance.IsEmbers()) { __instance.m_FuelHeatIncrease = 0; }

    //        HeatReservoir myreservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));

    //        if (myreservoir == null && fireOn)
    //        {
    //            //shucks create new one...
    //            Fire_RV.CreateHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));
    //            myreservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.gameObject));

    //        }


    //        float myreservoirtemp = 0;
    //        if (myreservoir != null)
    //        {
    //            myreservoirtemp = myreservoir.temp;
    //            //update the reservoir
    //            myreservoir.Update(__instance);

    //            if (!GameManager.GetWeatherComponent().IsIndoorEnvironment()) myreservoir.heatingsize = Mathf.Clamp(myreservoir.heatingsize, 5f, 25f);
    //            else myreservoir.heatingsize = Mathf.Clamp(myreservoir.heatingsize, 5f, 1000f);

    //            //myreservoir.heatingsize = fuel.m_FuelSourceItem.m_HeatOuterRadius * fire_outer + reservoir.heatingsize;
    //            __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = myreservoir.heatingsize;

    //            if (!fireOn && Mathf.Abs(myreservoirtemp) < 0.05) Fire_RV.RemoveReservoir(myreservoir.GUID);
    //        }



    //        if (__instance.m_ElapsedOnTODSeconds <= __instance.m_MaxOnTODSeconds)
    //        {
    //            //fire is not in ember stage
    //            __instance.m_UseEmbers = false;
    //            __instance.m_HeatSource.m_MaxTempIncrease = __instance.m_FuelHeatIncrease + myreservoirtemp;


    //            // do some weird score keeping
    //            float num = __instance.m_ElapsedOnTODSecondsUnmodified / 60f / 60f;
    //            __instance.MaybeUpdateLongestBurningFireStat(num);
    //            __instance.m_BurningTimeTODHours = num;
    //        }
    //        else
    //        {
    //            //fire is past its natural lifetime, ember stage or out...


    //            //fire is past its natural lifetime, ember stage or out...



    //            // Debug.Log("HeatReservoir   " + myreservoir.embercmins.ToString());

    //            float fireRivieSkill = 1;
    //            switch (GameManager.GetSkillFireStarting().GetCurrentTierNumber())
    //            {
    //                case 0:
    //                    fireRivieSkill = 10f;
    //                    break;
    //                case 1:
    //                    fireRivieSkill = 1f;
    //                    break;
    //                case 2:
    //                    fireRivieSkill = 0.1f;
    //                    break;
    //                case 3:
    //                    fireRivieSkill = 0.01f;
    //                    break;
    //                case 4:
    //                    fireRivieSkill = 0.001f;
    //                    break;

    //            }
    //            if (myreservoir != null && (myreservoir.embercmins > fireRivieSkill || __instance.m_FuelHeatIncrease > 0.05) && (__instance.m_FuelHeatIncrease + myreservoir.temp) > 1)
    //            {
    //                __instance.m_UseEmbers = true;
    //                __instance.m_FX.TriggerStage(FireState.Starting_TinderSmolder, __instance.m_UseEmbers, false);
    //                __instance.m_HeatSource.m_MaxTempIncrease = __instance.m_FuelHeatIncrease + myreservoir.temp;
                   
    //            }
    //            else
    //            {
    //                //fire out
    //                __instance.m_UseEmbers = false;
    //                __instance.m_FX.TriggerStage(FireState.Starting_TinderSmolder, __instance.m_UseEmbers, false);


    //                //if(myreservoir.temp<0.01) __instance.m_HeatSource.TurnOff();

    //                __instance.TurnOff();


    //                // AccessTools.Field(typeof(HeatSource), "m_TempIncrease").SetValue(__instance.m_HeatSource, (object)(myreservoir.temp));
    //                if (__instance.m_Campfire) __instance.m_Campfire.SetState(CampfireState.BurntOut);
    //            }

    //        }

    //        return false;

    //    }
    //}


    //[HarmonyPatch(typeof(Campfire), "GetHoverText")]
    //internal class Campfire_GetHoverText
    //{
    //    private static void Postfix(Campfire __instance, ref string __result)
    //    {
    //        if (__instance.m_Fire.GetRemainingLifeTimeSeconds() == 0) __result += "\n(" + __instance.m_Fire.GetHeatIncreaseText() + ")";
    //        //if (__instance.m_Fire.m_HeatSource.IsTurnedOn()&& __instance.m_Fire.GetRemainingLifeTimeSeconds()==0) __result += "\n(" + __instance.m_Fire.GetHeatIncreaseText() + ")";
    //    }
    //}

    //[HarmonyPatch(typeof(WoodStove), "GetHoverText")]
    //internal class WoodStove_GetHoverText
    //{
    //    private static void Postfix(Campfire __instance, ref string __result)
    //    {
    //        if (__instance.m_Fire.GetRemainingLifeTimeSeconds() == 0) __result += "\n(" + __instance.m_Fire.GetHeatIncreaseText() + ")";
    //        //if (__instance.m_Fire.m_HeatSource.IsTurnedOn() && __instance.m_Fire.GetRemainingLifeTimeSeconds() == 0) __result += "\n(" + __instance.m_Fire.GetHeatIncreaseText() + ")";
    //    }
    //}

    [HarmonyPatch(typeof(Fire), "OnFuelBurnt", new Type[] { typeof(GearItem) })]
    internal class Fire_OnFuelBurnt
    {
        private static void Prefix(Fire __instance, GearItem fuel)
        {
            HeatReservoir heatReservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(((Component)(object)__instance).gameObject));

            //Debug.Log(Fire_RV.getCentigradeMinutes(__instance).ToString());
            if (heatReservoir != null)
            {
                heatReservoir.TrackedBurntNames.Add(fuel.name);
                heatReservoir.TrackedBurntItemsCentigradminutesFire.Add(Fire_RV.getCentigradeMinutes(__instance) / Fire_RV.getStoveDurationModifier(((Component)(object)__instance).gameObject));
                heatReservoir.TrackedBurntGearItemHP.Add(fuel.GetNormalizedCondition());
            }
            else
            {
                Fire_RV.LastBurntItemsName = fuel.name;
                Fire_RV.LastBurntItemsCentigradminutesFire = Fire_RV.getCentigradeMinutes(__instance) / Fire_RV.getStoveDurationModifier(((Component)(object)__instance).gameObject);
                Fire_RV.LastBurntGearItemHP = fuel.GetNormalizedCondition();
            }

        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "RefreshFuelSources")]
    internal static class Panel_FeedFire_RefreshFuelSources
    {
        private static void Prefix(Panel_FeedFire __instance)
        {

            Inventory inventoryComponent = GameManager.GetInventoryComponent();
            foreach (GearItemObject item in inventoryComponent.m_Items)
            {
                GearItem gearItem = item;
                if ((bool)gearItem)
                {
                    FuelSourceItem fuelSourceItem = gearItem.m_FuelSourceItem;
                    if ((bool)fuelSourceItem)
                    {
                            foreach (string tinder in Lists.tinderitems)
                        {
                            if (gearItem.name == tinder)
                            {
                                gearItem.m_FuelSourceItem.m_IsTinder = false;
                            }
                        }
                    }

                }
            }
        }

        private static void Postfix(Panel_FeedFire __instance)
        {

            Inventory inventoryComponent = GameManager.GetInventoryComponent();
            foreach (GearItemObject item in inventoryComponent.m_Items)
            {
                GearItem gearItem = item;
                if ((bool)gearItem)
                {
                    FuelSourceItem fuelSourceItem = gearItem.m_FuelSourceItem;
                    if ((bool)fuelSourceItem)
                    {
                        foreach (string tinder in Lists.tinderitems)
                        {
                            if (gearItem.name == tinder)
                            {
                                gearItem.m_FuelSourceItem.m_IsTinder = true;
                            }
                        }
                    }



                }
            }
        }

    }


    [HarmonyPatch(typeof(Forge), "ForgeHotEnoughForUse")]
    
     internal static class Forge_ForgeHotEnoughForUse
    {
        private static void Prefix(Forge __instance)
        {
            var settings = Fire_RVSettings.Settings.options;
            __instance.m_MinTemperatureForCrafting = settings.MinTemperature;

            HeatReservoir myreservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(__instance.m_Fire.gameObject));
            if (myreservoir != null)
            {
                __instance.m_MinTemperatureForCrafting -= myreservoir.temp;
            }


            
        }

    }
    [HarmonyPatch(typeof(Inventory), "AddGear", null)]
    public class Inventory_AddGear
    {
        private static void Postfix(GameObject go)
        {
            if(go == null)
            {
                return;
            }
            if (go.name.ToLower().Contains("activeemberbox"))
            {
                TakeEmbers.MayApplychanges(go);
            }

        }
    }
}








    /*
     * 
     * private bool FilterItemFuelSource(GearItem gi) <-- Panel_FireStart
    {
        FuelSourceItem fuelSourceItem = gi.m_FuelSourceItem;
        return !(fuelSourceItem == null) && !fuelSourceItem.m_IsTinder;
    }
     * 
     * 
     * 
     * 
        [HarmonyPatch(typeof(HeatSource), "Update")]
        internal class HeatSource_Update
        {
            private static bool Prefix(HeatSource __instance, ref float __instance.m_TempIncrease)
        {
            if (GameManager.m_IsPaused)
            {
                return;
            }

           if (Utils.IsZero(this.m_TimeToReachMaxTempMinutes))
           {
                float todminutes = GameManager.GetTimeOfDayComponent().GetTODMinutes(Time.deltaTime);
                float changepermin = __instance.m_MaxTempIncrease - (__instance.m_TempIncrease - outsideTemp) * insulation_factor;
                __instance.m_TempIncrease += changepermin* todminutes;
            }
                     else
            {
                    float todminutes = GameManager.GetTimeOfDayComponent().GetTODMinutes(Time.deltaTime);
                float num = todminutes / this.m_TimeToReachMaxTempMinutes * this.m_MaxTempIncrease;
                if (this.m_TurnedOn)
                {
                    this.m_TempIncrease += num;
                }
                else
                {
                    this.m_TempIncrease -= num;
                }
                if (this.m_MaxTempIncrease > 0f)
                {
                    this.m_TempIncrease = Mathf.Clamp(this.m_TempIncrease, 0f, this.m_MaxTempIncrease);
                }
                else
                {
                    this.m_TempIncrease = Mathf.Clamp(this.m_TempIncrease, this.m_MaxTempIncrease, 0f);
                }
            }
        }
    */







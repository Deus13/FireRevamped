using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using Unity;
using MelonLoader;
using Il2CppTLD.Gear;
using Il2CppRewired;
using System.Security.Cryptography;
using static Il2Cpp.UIScrollView;
using static Il2CppSystem.Uri;


namespace ImprovedFires.Patches
{
	internal class FirePatches
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

				

				__instance.m_HeatSource.TurnOn();
				__instance.m_HeatSource.m_MaskTempIncrease = maskTempIncrease;
				__instance.m_HeatSource.m_TempIncrease = 1.0f;
				__instance.m_HeatSource.m_MaxTempIncrease += Fire_RV.getModifiedHeatIncrease(component);
				__instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = fuel.m_HeatInnerRadius;

				float outradscale = __instance.GetFireOuterRadiusScale();
				__instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = fuel.m_HeatOuterRadius * outradscale;

				if (__instance.m_FX)
				{
					__instance.m_FX.TriggerStage(__instance.m_FireState, true, true);
				}
				__instance.m_ElapsedOnTODSeconds = 0f;
				__instance.m_ElapsedOnTODSecondsUnmodified = 0f;
				__instance.m_MaxOnTODSeconds = Fire_RV.getStoveDurationModifier(__instance.gameObject) * Fire_RV.getModifiedDuration(component) * 60f * 60f;
				__instance.m_HeatSource.m_TimeToReachMaxTempMinutes = Fire_RV.getStoveDurationModifier(__instance.gameObject) * Fire_RV.getModifiedDuration(component) * 60f * Fire_RV.skillFireHeatingFactor();


				HeatReservoir heatReservoir = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(((Component)(object)__instance).gameObject));

				//Debug.Log("getCentigradeMinutes: " +Fire_RV.getCentigradeMinutes(__instance).ToString());

				heatReservoir.TrackedBurntItemsNames.Add(fuel.name);
				heatReservoir.TrackedBurntItemsCentigradminutesFire.Add(0f);
				heatReservoir.TrackedBurntGearItemHP.Add(component.CurrentHP);

				return false;
			}

			private static void Postfix(Fire __instance, FuelSourceItem fuel, bool maskTempIncrease)
			{
				//Debug.Log("getCentigradeMinutes: " + Fire_RV.getCentigradeMinutes(__instance).ToString());
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

				if (Il2Cpp.Utils.RollChance(10)) GameManager.GetSkillsManager().IncrementPointsAndNotify(SkillType.Firestarting, 1, SkillsManager.PointAssignmentMode.AssignOnlyInSandbox);
				HeatReservoir heatReservoir = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(((Component)(object)__instance).gameObject));

				//Debug.Log("getCentigradeMinutes: " + (Fire_RV.getCentigradeMinutes(__instance) / Fire_RV.getStoveDurationModifier(((Component)(object)__instance).gameObject)).ToString());

				heatReservoir.TrackedBurntItemsNames.Add(fuel.name);
				heatReservoir.TrackedBurntItemsCentigradminutesFire.Add(Fire_RV.getCentigradeMinutes(__instance) / Fire_RV.getStoveDurationModifier(((Component)(object)__instance).gameObject));
				heatReservoir.TrackedBurntGearItemHP.Add(fuel.CurrentHP);


				//   //Debug.Log("Fire:" + __instance.name);
				//Fire_RV.PrintHierarchy(__instance.gameObject);
				__instance.OnFuelBurnt(fuel);

				float old_time_remaining = __instance.GetRemainingLifeTimeSeconds();
				float old_max_tod = __instance.m_MaxOnTODSeconds;

				if (__instance.IsEmbers())
				{
					__instance.m_FX.TriggerStage(FireState.FullBurn, true, false);
					__instance.m_HeatSource.TurnOn();
					old_time_remaining = 0;
					old_max_tod = 0;
				}
				if (__instance.m_MaxOnTODSeconds < __instance.m_ElapsedOnTODSeconds) __instance.m_MaxOnTODSeconds = __instance.m_ElapsedOnTODSeconds;



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
					num3 = GameManager.GetFireManagerComponent().m_MaxHeatIncreaseOfFireInForge - __instance.m_HeatSource.m_MaxTempIncrease;
				}
				else
				{
					num3 = GameManager.GetFireManagerComponent().m_MaxHeatIncreaseOfFire - __instance.m_HeatSource.m_MaxTempIncrease;
				}
				num3 = Mathf.Clamp(num3, 0f, float.PositiveInfinity);

				float fuel_heat_increase = Mathf.Min(num3, Fire_RV.getModifiedHeatIncrease(fuel));
				float fuel_duration_increase = __instance.GetRemainingLifeTimeSeconds() - old_time_remaining;
				__instance.m_MaxOnTODSeconds = old_max_tod;


				float degmins_old = old_time_remaining * __instance.m_HeatSource.m_MaxTempIncrease;
				float degmins_fuel = fuel_duration_increase * fuel_heat_increase;
				float degmins_total = degmins_fuel + degmins_old;

				float shape_heat = __instance.m_HeatSource.m_MaxTempIncrease + fuel_heat_increase;
				float shape_duration = old_time_remaining + fuel_duration_increase;


				float correction = Mathf.Sqrt(degmins_total / (shape_heat * shape_duration));

				//Debug.Log("Old Fire Duration:" + old_time_remaining / 60f + " Fire temp:" + __instance.m_HeatSource.m_MaxTempIncrease);
				//Debug.Log("Fuel Item:" + fuel.name + " Heat:" + fuel_heat_increase + " Duration:" + fuel_duration_increase / 60f);
				//Debug.Log("degmins total:" + degmins_total + " reshaped degmins:" + (shape_heat * correction * shape_duration * correction));

				__instance.m_HeatSource.m_MaxTempIncrease = shape_heat * correction;
				__instance.m_MaxOnTODSeconds = __instance.m_ElapsedOnTODSeconds + shape_duration * correction;




				__instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = Mathf.Max(fuel.m_FuelSourceItem.m_HeatInnerRadius, __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius);
				__instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = Mathf.Max(fuel.m_FuelSourceItem.m_HeatOuterRadius * __instance.GetFireOuterRadiusScale(), __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius);

				if (fuel.m_FuelSourceItem.m_IsTinder)
				{
					__instance.m_HeatSource.m_TimeToReachMaxTempMinutes = Math.Max(1, __instance.m_HeatSource.m_TimeToReachMaxTempMinutes - fuel.m_FuelSourceItem.m_BurnDurationHours / 60f);
					__instance.m_HeatSource.m_TempIncrease = __instance.m_HeatSource.m_TempIncrease + fuel.m_FuelSourceItem.m_HeatIncrease;
				}
				else if (!fuel.name.ToLower().StartsWith("gear_stick"))
				{

					if (old_time_remaining > 0) __instance.m_HeatSource.m_TempIncrease = (__instance.m_HeatSource.m_TempIncrease) * (old_time_remaining / __instance.GetRemainingLifeTimeSeconds() - Fire_RV.skillFireHeatingFactor());
					else __instance.m_HeatSource.m_TempIncrease = (__instance.m_HeatSource.m_TempIncrease - Fire_RV.skillFireHeatingFactor());


					__instance.m_HeatSource.m_TimeToReachMaxTempMinutes = ((1.0f - __instance.m_HeatSource.m_TempIncrease / __instance.m_HeatSource.m_MaxTempIncrease) * __instance.m_HeatSource.m_TimeToReachMaxTempMinutes + shape_duration * correction / 60f) * Fire_RV.skillFireHeatingFactor() * (1f - heatReservoir.embersPoprtion(__instance));
				}


				if (__instance.m_HeatSource.m_TempIncrease <= 0)
				{
					Fire_RV.breakdownFire(__instance);
					__instance.TurnOffImmediate();
				}


				//Debug.Log("m_TimeToReachMaxTempMinutes:" + __instance.m_HeatSource.m_TimeToReachMaxTempMinutes + " m_TempIncrease" + __instance.m_HeatSource.m_TempIncrease);

				__instance.m_FX.TriggerFlareupLarge();


				return false;
			}

				
			private static void Postfix(Fire __instance, GearItem fuel, bool inForge)
			{
				//Debug.Log("getCentigradeMinutes: " + (Fire_RV.getCentigradeMinutes(__instance) / Fire_RV.getStoveDurationModifier(((Component)(object)__instance).gameObject)).ToString());
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
				float fire_temp = __instance.m_HeatSource.m_MaxTempIncrease;
				float fire_time = __instance.GetRemainingLifeTimeSeconds();
				float remaining_fire_cs = fire_time * fire_temp;
				__result = remaining_fire_cs > (Fire_RV.torch_cost(__instance));
				return false;
			}
		}


		[HarmonyPatch(typeof(Panel_FeedFire), "OnTakeTorch")]
		internal class Panel_FeedFire_OnTakeTorch
		{
			//private static bool Prefix(Panel_FeedFire __instance)

			//{

			//	if (!__instance.m_FireManager)
			//	{
			//		GameAudioManager.PlayGUIError();
			//		return false;
			//	}
			//	if (!__instance.CanTakeTorch())//then we can't take torch
			//	{
			//		GameAudioManager.PlayGUIError();
			//		return false;
			//	}
			//	GearItem gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory("GEAR_Torch");
			//	if (gearItem == null || gearItem.m_TorchItem == null)
			//	{
			//		return false;
			//	}
			//	gearItem.OverrideGearCondition(GearStartCondition.Low, false);
			//	gearItem.m_TorchItem.m_ElapsedBurnMinutes = Mathf.Clamp01(1f - gearItem.m_CurrentHP / gearItem.m_MaxHP) * gearItem.m_TorchItem.GetModifiedBurnLifetimeMinutes();
			//	GameManager.GetPlayerAnimationComponent().MaybeDropAndResetCurrentWeapon();
			//	gearItem.m_TorchItem.Ignite();
			//	GameManager.GetPlayerManagerComponent().EquipItem(gearItem, false);
			//	GameManager.GetPlayerManagerComponent().ClearRestoreItemInHandsAfterInteraction();
			//	if (!__instance.m_FireManager. .m_Fire.m_IsPerpetual)
			//	{
			//		float torchcost_cs = Fire_RV.torch_cost(__instance.m_Fire);
			//		float fire_temp = __instance.m_Fire.m_HeatSource.m_MaxTempIncrease;
			//		float fire_time = __instance.m_Fire.GetRemainingLifeTimeSeconds();
			//		float remaining_fire_cs = fire_time * fire_temp;
			//		float new_fire_cs = (remaining_fire_cs - torchcost_cs);
			//		float myfactor = Mathf.Pow(new_fire_cs / remaining_fire_cs, 0.5f);
			//		float time_reduction = fire_time - (fire_time * myfactor);
			//		float heat_reduction = fire_temp - (fire_temp * myfactor);
			//		//Debug.Log("new_cs:" + new_fire_cs + " ratio::" + (new_fire_cs / remaining_fire_cs) + " myfactor::" + myfactor + " timered::" + time_reduction + " heat_red:" + heat_reduction);
			//		__instance.m_Fire.ReduceDurationByTODSeconds(time_reduction);
			//		__instance.m_Fire.ReduceHeatByDegrees(heat_reduction);

			//		fire_temp = __instance.m_Fire.m_HeatSource.m_MaxTempIncrease;
			//		fire_time = __instance.m_Fire.GetRemainingLifeTimeSeconds();
			//		//Debug.Log("Reducing Fire: Old cs:" + remaining_fire_cs + " New cs:" + fire_temp * fire_time);
			//	}
			//	__instance.ExitFeedFireInterface();
			//	return false;
			//}
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
				//Debug.Log("gear:" + component.m_YieldGear[0].name);
				if (component.m_YieldGear[0].name == "GEAR_Stick") __instance.m_HarvestTimeSeconds /= 6.0f;
				__instance.StartHarvest();
				if (component.m_YieldGear[0].name == "GEAR_Stick") __instance.m_HarvestTimeSeconds *= 6.0f;
				return false;
			}
		}


		[HarmonyPatch(typeof(Fire), "MaybeBlowOutFromWind")]
		internal class Fire_MaybeBlowOutFromWind
		{
			private static bool Prefix(Fire __instance)
			{
				var setting = Settings.Instance;

				if (__instance.m_Campfire && !__instance.CanFeedFire())
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
				var setting = Settings.Instance;

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
				HeatReservoir myres = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(fire.gameObject));
				if (myres == null)
				{
					// //Debug.Log("no heat reservoir associated with fire");
					return;
				}


			}
		}


		[HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
		internal static class RestoreGlobalData
		{
			[HarmonyPriority(Priority.Low)]
			internal static void Postfix()
			{
				if (!GameManager.IsMainMenuActive() && !GameManager.IsBootSceneActive() && !GameManager.IsEmptySceneActive()) return;
				//Debug.Log("load");
				new SaveManager().LoadHeatReservoirs();
			}
		}

		[HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
		internal static class SaveGlobalData
		{
			[HarmonyPriority(Priority.Low)]
			internal static void Postfix(SlotData slot)
			{
				if (!GameManager.IsMainMenuActive() && !GameManager.IsBootSceneActive() && !GameManager.IsEmptySceneActive()) return;
				//Debug.Log("save");
				new SaveManager().SaveHeatReservoirs();
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


				HeatReservoir myreservoir = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(__instance.gameObject));

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
		[HarmonyPatch(typeof(Panel_FeedFire), "Update")]
		internal class Panel_FeedFire_Prefix
		{
			private static void Postfix(Panel_FeedFire __instance)
			{
				(__instance.m_Sprite_FireFill as UIBasicSprite).fillAmount = __instance.m_FireplaceInteraction.Fire.m_HeatSource.m_TempIncrease / __instance.m_FireplaceInteraction.Fire.m_HeatSource.m_MaxTempIncrease;
			}
		}

		[HarmonyPatch(typeof(Fire), "Update")]
		internal class Fire_Update_Prefix
		{
			private static bool Prefix(Fire __instance)
			{
				HeatReservoir myreservoir = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(__instance.gameObject));
				myreservoir.Update(__instance);

				if(__instance.m_UseEmbers == true)
				{
					__instance.m_FX.TriggerStage(FireState.Starting_TinderSmolder, __instance.m_UseEmbers, false);
					__instance.UpdateFireStage();

					return false;
				}

				return true;
				
			}
		}

			
		//		[HarmonyPatch(typeof(Campfire), "GetHoverText")]
		//internal class Campfire_GetHoverText
		//{
		//	private static void Postfix(Campfire __instance, ref string __result)
		//	{
		//		if (__instance.Fire.GetRemainingLifeTimeSeconds() == 0) __result += "\n(" + __instance.Fire.GetHeatIncreaseText() + ")";
		//		//if (__instance.m_Fire.m_HeatSource.IsTurnedOn()&& __instance.m_Fire.GetRemainingLifeTimeSeconds()==0) __result += "\n(" + __instance.m_Fire.GetHeatIncreaseText() + ")";
		//	}
		//}

		//[HarmonyPatch(typeof(WoodStove), "GetHoverText")]
		//internal class WoodStove_GetHoverText
		//{
		//	private static void Postfix(Campfire __instance, ref string __result)
		//	{
		//		if (__instance.Fire.GetRemainingLifeTimeSeconds() == 0) __result += "\n(" + __instance.Fire.GetHeatIncreaseText() + ")";
		//		//if (__instance.m_Fire.m_HeatSource.IsTurnedOn() && __instance.m_Fire.GetRemainingLifeTimeSeconds() == 0) __result += "\n(" + __instance.m_Fire.GetHeatIncreaseText() + ")";
		//	}
		//}



		[HarmonyPatch(typeof(Panel_FeedFire), "RefreshFuelSources")]
		internal static class Panel_FeedFire_RefreshFuelSources
		{
			
			private static void Prefix(Panel_FeedFire __instance)
			{

				//Debug.Log("Prefix RefreshFuelSources:");
				Inventory inventoryComponent = GameManager.GetInventoryComponent();
				foreach (GearItemObject item in inventoryComponent.m_Items)
				{
					GearItem gearItem = item;
					if ((bool)gearItem)
					{
						FuelSourceItem fuelSourceItem = gearItem.m_FuelSourceItem;
						if ((bool)fuelSourceItem)
						{
							foreach (string tinder in Fire_RV.tinderitems)
							{
								if (gearItem.name == tinder)
								{
									//Debug.Log("gearItem.name");
									gearItem.m_FuelSourceItem.m_IsTinder = false;
								}
							}
						}

					}
				}
			}

			private static void Postfix(Panel_FeedFire __instance)
			{
				//Debug.Log("Postfix RefreshFuelSources");
				Inventory inventoryComponent = GameManager.GetInventoryComponent();
				foreach (GearItemObject item in inventoryComponent.m_Items)
				{
					GearItem gearItem = item;
					if ((bool)gearItem)
					{
						FuelSourceItem fuelSourceItem = gearItem.m_FuelSourceItem;
						if ((bool)fuelSourceItem)
						{
							foreach (string tinder in Fire_RV.tinderitems)
							{
								if (gearItem.name == tinder)
								{
									//Debug.Log("gearItem.name");
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
				var settings = Settings.Instance;
				__instance.m_MinTemperatureForCrafting = settings.MinTemperature;

			}

		}

	}

}

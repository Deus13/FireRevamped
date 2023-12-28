using UnityEngine;
using HarmonyLib;
using Il2Cpp;
using static Il2CppAK.SWITCHES;

namespace ImprovedFires
{
	internal class FireshapeButton
	{
		internal static string Text;
		internal static GameObject ButtonBreack;
		internal static GameObject ButtonHeap;
		internal static GameObject ButtonSpread;

		internal static void Initialize(Panel_FeedFire panel_FeedFire, string Text , float x , Action act, GameObject Button)
		{
			if (panel_FeedFire == null) return;



			Button = GameObject.Instantiate<GameObject>(panel_FeedFire.m_ActionButtonObject, panel_FeedFire.m_ActionButtonObject.transform.parent, true);
			Button.transform.Translate(x, 0.18f, 0);
			Utils.GetComponentInChildren<UILabel>(Button).text = Text;



			AddAction(Button, act);

			NGUITools.SetActive(Button, true);
		}
		private static void AddAction(GameObject button, System.Action action)
		{
			Il2CppSystem.Collections.Generic.List<EventDelegate> placeHolderList = new Il2CppSystem.Collections.Generic.List<EventDelegate>();
			placeHolderList.Add(new EventDelegate(action));
			Utils.GetComponentInChildren<UIButton>(button).onClick = placeHolderList;
		}
		internal static void SetActive(bool active, GameObject Button)
		{
			NGUITools.SetActive(Button, active);
		}
	}

	[HarmonyPatch(typeof(Panel_FeedFire), "Initialize")]
	internal class Panel_FeedFire_Initialize
	{
		private static void Postfix(Panel_FeedFire __instance)
		{
			//MelonLoader.MelonLogger.Msg("FeedFire_Initialize");
			//FireshapeButton.Initialize(__instance, "breack", -1f, new Action(() => ), FireshapeButton.ButtonBreack);
			FireshapeButton.Initialize(__instance, "heap", -0.5f, new Action(() => Fire_RV.heapFire(__instance.m_FireplaceInteraction.Fire)), FireshapeButton.ButtonHeap);
			FireshapeButton.Initialize(__instance, "spread", 0.0f , new Action(() => Fire_RV.spreadFire(__instance.m_FireplaceInteraction.Fire)), FireshapeButton.ButtonSpread);
		}
	}

	[HarmonyPatch(typeof(Panel_FeedFire), "Enable")]
	internal class Panel_FeedFire_Enable
	{
		private static void Postfix(Panel_FeedFire __instance, bool enable)
		{
			//MelonLoader.MelonLogger.Msg("FeedFire_Enable");
			if (!enable) return;
			if (Fire_RV.canStokeFire(__instance.m_FireplaceInteraction.Fire)) FireshapeButton.SetActive(true, FireshapeButton.ButtonHeap);
			else FireshapeButton.SetActive(false, FireshapeButton.ButtonHeap);

			if (Fire_RV.canSpreadFire(__instance.m_FireplaceInteraction.Fire)) FireshapeButton.SetActive(true, FireshapeButton.ButtonSpread);
			else FireshapeButton.SetActive(false, FireshapeButton.ButtonSpread);
		}
	}
}
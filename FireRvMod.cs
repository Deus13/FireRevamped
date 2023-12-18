using MelonLoader;
using UnityEngine;

namespace Fire_RV
{
	internal class FireRVMod : MelonMod
	{

		public override void OnApplicationStart()
		{
			Fire_RVSettings.Settings.OnLoad();
			Fire_RV.OnLoad();
			Debug.Log($"[{InfoAttribute.Name}] version {InfoAttribute.Version} loaded!");
		}
	}
}

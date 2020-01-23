using Harmony;
using System.Reflection;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;


namespace Fire_RV
{
    public static class Fire_RV
    {
        private static List<HeatReservoir> myreservoirs;

        public static bool scene_loading = true;

        public static Fire currentFire = null;

        public static float LastBurntItemsCentigradminutesFire;

        public static float LastBurntGearItemHP;

        public static string LastBurntItemsName;

        private static Dictionary<string, float> heatResSizes = new Dictionary<string, float> {
{"BankA",200f},
{"BarnHouseA",500f},
{"BearCave",2000f},
{"Boot",1},
{"CampOffice",300f},
{"CaveB",2000f},
{"CaveC",2000f},
{"CaveD",2000f},
{"ChurchB",250f},
{"CoastalHouseA",250f},
{"CoastalHouseB",250f},
{"CoastalHouseC",250f},
{"CoastalHouseD",250f},
{"CoastalHouseE",250f},
{"CoastalHouseF",250f},
{"CoastalHouseG",250f},
{"CoastalHouseH",250f},
{"CoastalRegion",1f},
{"CommunityHall",250f},
{"ConvenienceStoreA",200f},
{"CrashMountainRegion",1},
{"CrossroadsRegion",1},
{"Dam",2000f},
{"DamCaveTransitionZone",2000f},
{"DamRiverTransitionZoneB",1},
{"DamTrailerB",100f},
{"DamTransitionZone",1},
{"Empty",1},
{"FarmHouseA",400f},
{"FarmHouseABasement",150f},
{"FarmHouseB",400f},
{"FishingCabinA",100f},
{"FishingCabinB",100f},
{"FishingCabinC",100f},
{"FishingCabinD",100f},
{"GreyMothersHouseA",400f},
{"GreyMothersHouseA_DialogueTest",400f},
{"Gym_gear_indoors",1},
{"Gym_gear_outdoors",1},
{"Hangar",800f},
{"HighwayMineTransitionZone",200f},
{"HighwayTransitionZone",1f},
{"HouseBasementC",100f},
{"HouseBasementE",100f},
{"HouseBasementF",100f},
{"HouseBasementPV",100f},
{"HuntingLodgeA",400f},
{"IceCaveA",2000f},
{"IceCaveB",2000f},
{"IceCaveC",2000f},
{"LakeCabinA",100f},
{"LakeCabinB",100f},
{"LakeCabinC",100f},
{"LakeCabinD",100f},
{"LakeCabinE",100f},
{"LakeCabinF",100f},
{"LakeRegion",100f},
{"LighthouseA",600f},
{"LoneLakeCabinA",100f},
{"MainMenu",1},
{"MaintenanceShedA",900f},
{"MarshRegion",1f},
{"MiltonHouseA",250f},
{"MiltonHouseC",250f},
{"MiltonHouseD",250f},
{"MiltonHouseF1",250f},
{"MiltonHouseF2",250f},
{"MiltonHouseF3",250f},
{"MiltonHouseG",250f},
{"MiltonHouseH1",250f},
{"MiltonHouseH2",250f},
{"MiltonHouseH3",250f},
{"MiltonTrailerB",250f},
{"MineTransitionZone",2000f},
{"MountainCaveA",2000f},
{"MountainCaveB",2000f},
{"MountainTownCaveA",2000f},
{"MountainTownCaveB",2000f},
{"MountainTownRegion",1f},
{"PostOfficeA",150f},
{"PrepperCacheA",100f},
{"PrepperCacheAurora",100f},
{"PrepperCacheB",100f},
{"PrepperCacheC",100f},
{"PrepperCacheD",100f},
{"PrepperCacheE",100f},
{"PrepperCacheEmpty",100f},
{"PrepperCacheF",100f},
{"PrepperCacheHank",100f},
{"QuonsetGasStation",300f},
{"RadioControlHut",200f},
{"RavineTransitionZone",1},
{"RiverValleyRegion",1},
{"RiverValleyTransitionCave",2000f},
{"RuralRegion",1},
{"RuralStoreA",200f},
{"SafeHouseA",200f},
{"TracksRegion",1f},
{"TrailerA",100f},
{"TrailerB",100f},
{"TrailerC",100f},
{"TrailerD",100f},
{"TrailerSShape",1},
{"TransitionCHtoDP",1},
{"TransitionCHtoPV",1},
{"TransitionMLtoCH",1},
{"TransitionMLtoPV",1},
{"WhalingMine",2000f},
{"WhalingShipA",600f},
{"WhalingStationRegion",1},
{"WhalingWarehouseA",1200f},


        };

        private static Dictionary<string, float> heatResSizesMax = new Dictionary<string, float> {
{"BankA",1},
{"BarnHouseA",1},
{"BearCave",20000f},
{"Boot",1},
{"CampOffice",1},
{"CaveB",20000f},
{"CaveC",20000f},
{"CaveD",20000f},
{"ChurchB",1},
{"CoastalHouseA",1},
{"CoastalHouseB",1},
{"CoastalHouseC",1},
{"CoastalHouseD",1},
{"CoastalHouseE",1},
{"CoastalHouseF",1},
{"CoastalHouseG",1},
{"CoastalHouseH",1},
{"CoastalRegion",1f},
{"CommunityHall",1},
{"ConvenienceStoreA",1},
{"CrashMountainRegion",1},
{"CrossroadsRegion",1},
{"Dam",1},
{"DamCaveTransitionZone",20000f},
{"DamRiverTransitionZoneB",1},
{"DamTrailerB",1},
{"DamTransitionZone",1},
{"Empty",1},
{"FarmHouseA",1},
{"FarmHouseABasement",1},
{"FarmHouseB",1},
{"FishingCabinA",1},
{"FishingCabinB",1},
{"FishingCabinC",1},
{"FishingCabinD",1},
{"GreyMothersHouseA",1},
{"GreyMothersHouseA_DialogueTest",1},
{"Gym_gear_indoors",1},
{"Gym_gear_outdoors",1},
{"Hangar",1},
{"HighwayMineTransitionZone",20000f},
{"HighwayTransitionZone",1f},
{"HouseBasementC",1},
{"HouseBasementE",1},
{"HouseBasementF",1},
{"HouseBasementPV",1},
{"HuntingLodgeA",1},
{"IceCaveA",20000f},
{"IceCaveB",20000f},
{"IceCaveC",20000f},
{"LakeCabinA",1},
{"LakeCabinB",1},
{"LakeCabinC",1},
{"LakeCabinD",1},
{"LakeCabinE",1},
{"LakeCabinF",1},
{"LakeRegion",1},
{"LighthouseA",1},
{"LoneLakeCabinA",1},
{"MainMenu",1},
{"MaintenanceShedA",1},
{"MarshRegion",1f},
{"MiltonHouseA",1},
{"MiltonHouseC",1},
{"MiltonHouseD",1},
{"MiltonHouseF1",1},
{"MiltonHouseF2",1},
{"MiltonHouseF3",1},
{"MiltonHouseG",1},
{"MiltonHouseH1",1},
{"MiltonHouseH2",1},
{"MiltonHouseH3",1},
{"MiltonTrailerB",1},
{"MineTransitionZone",20000f},
{"MountainCaveA",20000f},
{"MountainCaveB",20000f},
{"MountainTownCaveA",20000f},
{"MountainTownCaveB",20000f},
{"MountainTownRegion",1f},
{"PostOfficeA",1},
{"PrepperCacheA",1},
{"PrepperCacheAurora",1},
{"PrepperCacheB",1},
{"PrepperCacheC",1},
{"PrepperCacheD",1},
{"PrepperCacheE",1},
{"PrepperCacheEmpty",1},
{"PrepperCacheF",1},
{"PrepperCacheHank",1},
{"QuonsetGasStation",1},
{"RadioControlHut",1},
{"RavineTransitionZone",1},
{"RiverValleyRegion",1},
{"RiverValleyTransitionCave",1},
{"RuralRegion",1},
{"RuralStoreA",1},
{"SafeHouseA",1},
{"TracksRegion",1f},
{"TrailerA",1},
{"TrailerB",1},
{"TrailerC",1},
{"TrailerD",1},
{"TrailerSShape",1},
{"TransitionCHtoDP",1},
{"TransitionCHtoPV",1},
{"TransitionMLtoCH",1},
{"TransitionMLtoPV",1},
{"WhalingMine",20000f},
{"WhalingShipA",1},
{"WhalingStationRegion",1f},
{"WhalingWarehouseA",1},
        };

        private static Dictionary<string, float> heatResIns = new Dictionary<string, float> {
{"BankA",1},
{"BarnHouseA",1},
{"BearCave",1},
{"Boot",1},
{"CampOffice",1},
{"CaveB",1},
{"CaveC",1},
{"CaveD",1},
{"ChurchB",1},
{"CoastalHouseA",1},
{"CoastalHouseB",1},
{"CoastalHouseC",1},
{"CoastalHouseD",1},
{"CoastalHouseE",1},
{"CoastalHouseF",1},
{"CoastalHouseG",1},
{"CoastalHouseH",1},
{"CoastalRegion",1f},
{"CommunityHall",1},
{"ConvenienceStoreA",1},
{"CrashMountainRegion",1},
{"CrossroadsRegion",1},
{"Dam",1},
{"DamCaveTransitionZone",1},
{"DamRiverTransitionZoneB",1},
{"DamTrailerB",1},
{"DamTransitionZone",1},
{"Empty",1},
{"FarmHouseA",1},
{"FarmHouseABasement",1},
{"FarmHouseB",1},
{"FishingCabinA",1},
{"FishingCabinB",1},
{"FishingCabinC",1},
{"FishingCabinD",1},
{"GreyMothersHouseA",1},
{"GreyMothersHouseA_DialogueTest",1},
{"Gym_gear_indoors",1},
{"Gym_gear_outdoors",1},
{"Hangar",1},
{"HighwayMineTransitionZone",1},
{"HighwayTransitionZone",1f},
{"HouseBasementC",1},
{"HouseBasementE",1},
{"HouseBasementF",1},
{"HouseBasementPV",1},
{"HuntingLodgeA",1},
{"IceCaveA",1},
{"IceCaveB",1},
{"IceCaveC",1},
{"LakeCabinA",1},
{"LakeCabinB",1},
{"LakeCabinC",1},
{"LakeCabinD",1},
{"LakeCabinE",1},
{"LakeCabinF",1},
{"LakeRegion",1},
{"LighthouseA",1},
{"LoneLakeCabinA",1},
{"MainMenu",1},
{"MaintenanceShedA",1},
{"MarshRegion",1f},
{"MiltonHouseA",1},
{"MiltonHouseC",1},
{"MiltonHouseD",1},
{"MiltonHouseF1",1},
{"MiltonHouseF2",1},
{"MiltonHouseF3",1},
{"MiltonHouseG",1},
{"MiltonHouseH1",1},
{"MiltonHouseH2",1},
{"MiltonHouseH3",1},
{"MiltonTrailerB",1},
{"MineTransitionZone",1},
{"MountainCaveA",1},
{"MountainCaveB",1},
{"MountainTownCaveA",1},
{"MountainTownCaveB",1},
{"MountainTownRegion",1f},
{"PostOfficeA",1},
{"PrepperCacheA",1},
{"PrepperCacheAurora",1},
{"PrepperCacheB",1},
{"PrepperCacheC",1},
{"PrepperCacheD",1},
{"PrepperCacheE",1},
{"PrepperCacheEmpty",1},
{"PrepperCacheF",1},
{"PrepperCacheHank",1},
{"QuonsetGasStation",1},
{"RadioControlHut",1},
{"RavineTransitionZone",1},
{"RiverValleyRegion",1},
{"RiverValleyTransitionCave",1},
{"RuralRegion",1},
{"RuralStoreA",1},
{"SafeHouseA",1},
{"TracksRegion",1f},
{"TrailerA",1},
{"TrailerB",1},
{"TrailerC",1},
{"TrailerD",1},
{"TrailerSShape",1},
{"TransitionCHtoDP",1},
{"TransitionCHtoPV",1},
{"TransitionMLtoCH",1},
{"TransitionMLtoPV",1},
{"WhalingMine",1},
{"WhalingShipA",1},
{"WhalingStationRegion",1f},
{"WhalingWarehouseA",1},




        };

        public static void OnLoad()
        {
            //Debug.Log("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);


            uConsole.RegisterCommand("fire-log", FireLog);
        }

        internal static void FireLog()
        {
            Vector3 pos = GameManager.GetPlayerTransform().position;
            //list fires with heat sources
            for (int i = 0; i < FireManager.m_Fires.Count; i++)
            {
                Fire fire = FireManager.m_Fires[i];
                float dist = Vector3.Distance(pos, fire.transform.position);
                Debug.Log("Fire " + i + " lit;" + fire.IsBurning() + " dist:" + dist + " fire GUID:" + Utils.GetGuidFromGameObject(fire.gameObject) + " heatsource GUID:" + Utils.GetGuidFromGameObject(fire.m_HeatSource.gameObject));
                Debug.Log("\tHeat Reservoir " + Utils.SerializeObject(Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(fire.gameObject))));

            }

            List<HeatSource> myheats = (List<HeatSource>)AccessTools.Field(typeof(HeatSourceManager), "m_HeatSources").GetValue(GameManager.GetHeatSourceManagerComponent());
            for (int i = 0; i < myheats.Count; i++)
            {
                HeatSource myheat = myheats[i];
                float dist = Vector3.Distance(pos, myheat.transform.position);
                Debug.Log("Heat " + i + " temp;" + myheat.GetCurrentTempIncrease() + " dist:" + dist + " heatsource GUID:" + Utils.GetGuidFromGameObject(myheat.gameObject));
            }

            for (int i = 0; i < myreservoirs.Count; i++)
            {
                HeatReservoir myheatres = myreservoirs[i];
                Debug.Log("Res " + i + ":"+ Utils.SerializeObject(myreservoirs[i]));
            }


        }


        private const string SAVE_FILE_NAME = "fire_rv-settings";

        public static string serialize()
        {
            StringArray stringArray = new StringArray();
            stringArray.strings = new string[1];
            if (myreservoirs == null) myreservoirs = new List<HeatReservoir>();
            stringArray.strings[0] = Utils.SerializeObject(myreservoirs);
            Debug.Log("finished serializing myreservoirs, count:"+myreservoirs.Count);
            return (Utils.SerializeObject(stringArray));
        }

        public static void deserialize(string serialString)
        {
            StringArray stringArray = Utils.DeserializeObject<StringArray>(serialString);
            myreservoirs = Utils.DeserializeObject<List<HeatReservoir>>(stringArray.strings[0]);
            List < HeatReservoir > toDelete = new List<HeatReservoir>();

            string version = GetCurrentVersion();
            

            foreach (HeatReservoir HR in myreservoirs) if(HR.ReservoirCreationVerion ==null || HR.ReservoirCreationVerion != version) toDelete.Add(HR);
            foreach (HeatReservoir HR in toDelete) myreservoirs.Remove(HR);


                Debug.Log("finished deserializing myreservoirs, count:"+myreservoirs.Count);
        }


        internal static void LoadData(string name)
        {
            string data = SaveGameSlots.LoadDataFromSlot(name, SAVE_FILE_NAME);
            if (data != null)
            {
                Fire_RV.deserialize(data);

            }
            if (data == null) myreservoirs.Clear();
            
        }

        internal static void SaveData(SaveSlotType gameMode, string name)
        {
            SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, SAVE_FILE_NAME, Fire_RV.serialize());
        }

        internal static string GetCurrentVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;

        }


            internal static void Disable()
        {
            //if (isEnabled)
            // {
            //     isEnabled = false;
            //    settings.enabled = false;
            //    RestoreKeyframeTimes(GameManager.GetUniStorm());
            // }
        }

        public static void PrintHierarchy(GameObject obj)
        {
            Debug.Log("Printing Hierarchy for:" + obj.name + " :" + obj.GetType());
            while (obj.transform&& obj.transform.parent&& obj.transform.parent.gameObject)
            {
                obj = obj.transform.parent.gameObject;
            }
            Debug.Log(Fire_RV.PrintSubHierarchy(obj,0));


        }

        public static string PrintSubHierarchy(GameObject obj,int depth)
        {
            string tabs="";
            for (int j = 0; j < depth; j++) tabs += "\t";
            string myres = tabs + obj.name + " :" + obj.GetType() + " " + obj.transform.childCount + " components:";
            Component[] mycomps = obj.GetComponents(typeof(Component));
            for (int k = 0; k < mycomps.Length; k++)
            {
                myres += " "+k+":" + mycomps[k].GetType().Name;
            }
            myres += " Children:\n";
            

            // for (int j=0;j<obj.g)
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                myres = myres + PrintSubHierarchy(obj.transform.GetChild(i).gameObject,depth+1);
            }
            return myres;
        }

        public static string getFireType(GameObject curObj)
        {
            string fire_type = "Campfire";
            bool done = false;
            while (curObj.transform.parent&& !done)
            {
                if (curObj.transform.parent.gameObject.name.Length>12 && curObj.transform.parent.gameObject.name.Substring(0, 11) == "INTERACTIVE")
                {
                    fire_type = curObj.transform.parent.name;
                    done = true;
                }
                curObj = curObj.transform.parent.gameObject;
            }
            return fire_type;
        }

        internal static string ggtimeS(float intime)
        {
            intime = intime / 3600f;
            return string.Format("{0:00}:{1:00}:{2:00}", Mathf.FloorToInt(intime), Mathf.FloorToInt(((intime) % 1) * 60), Mathf.FloorToInt((((intime) % 1) * 3600) % 60));
        }



        public static float getCentigradeMinutes(Fire fire)
        {
            float time_remaining = fire.GetRemainingLifeTimeSeconds()/60f;
            float temperature = (float)AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").GetValue(fire);
            return temperature * time_remaining  ;
        }

        public static bool canStokeFire(Fire fire)
        {
            return getHeatTimeRatio(fire) < 0.5f;
        }
        public static bool canSpreadFire(Fire fire)
        {
            return getHeatTimeRatio(fire) > 0.05f;
        }

        public static float getHeatTimeRatio(Fire fire)
        {
            float time_remaining = fire.GetRemainingLifeTimeSeconds()/60f;
            float temperature = (float)AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").GetValue(fire);
            return temperature/ time_remaining;
        }

        public static void tryHeapFire()
        {
            InterfaceManager.m_Panel_GenericProgressBar.Launch("Heaping Fire",5f, 2f, 0, null, null, false, true, new OnExitDelegate(heapFire));
        }
        public static void heapFire(bool success, bool playerCancel, float progress)
        {
            
            if (!success)
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Failedattempttostartfire"), false);
            }
            // GameManager.GetFireManagerComponent().UpdateSkillAfterFireCreationAttempt(success);
            // GameManager.GetFireManagerComponent().ExitFireStarting();
            /// AkSoundEngine.StopPlayingID(this.m_FireStarterLoop, GameManager.GetGameAudioManagerComponent().m_StopAudioFadeOutMicroseconds);
            // if (base.gameObject.transform.parent)
            //  {
            //     WoodStove component = base.gameObject.transform.parent.gameObject.GetComponent<WoodStove>();
            //     if (component)
            //     {
            //         component.Close();
            //     }
            //  }
            if (success)
            {
                heapFire(currentFire);
            }
        }

        public static void heapFire(Fire fire)
        {
            Debug.Log("In heap fire:"+getHeatTimeRatio(fire));
            float time_remaining = fire.GetRemainingLifeTimeSeconds();
            float temperature = (float)AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").GetValue(fire);

            float MaxOnTod = (float)AccessTools.Field(typeof(Fire), "m_MaxOnTODSeconds").GetValue(fire);

            float newMaxOnTod = MaxOnTod - time_remaining + time_remaining / 1.2f;

            AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").SetValue(fire, temperature * 1.2f);
            AccessTools.Field(typeof(Fire), "m_MaxOnTODSeconds").SetValue(fire, newMaxOnTod);
        }

        public static void trySpreadFire()
        {
            InterfaceManager.m_Panel_GenericProgressBar.Launch("Spreading Fire", 5f, 2f, 0, null, null, false, true, new OnExitDelegate(spreadFire));

        }

        public static void spreadFire(bool success, bool playerCancel, float progress)
        {
            if (!success)
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Failedattempttostartfire"), false);
            }
            if (success)
            {
                spreadFire(currentFire);
            }
        }
        public static void spreadFire(Fire fire)
        {
            Debug.Log("In spread fire:"+getHeatTimeRatio(fire));
            float time_remaining = fire.GetRemainingLifeTimeSeconds();
            float temperature = (float)AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").GetValue(fire);

            float MaxOnTod = (float)AccessTools.Field(typeof(Fire), "m_MaxOnTODSeconds").GetValue(fire);

            float newMaxOnTod = MaxOnTod - time_remaining + time_remaining * 1.2f;

            AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").SetValue(fire, temperature / 1.2f);
            AccessTools.Field(typeof(Fire), "m_MaxOnTODSeconds").SetValue(fire, newMaxOnTod);
        }


        public static void tryBreakdownFire()
        {
            InterfaceManager.m_Panel_GenericProgressBar.Launch("Breaking Down Fire", 5f, 2f, 0, null, null, false, true, new OnExitDelegate(breakdownFire));

        }
        public static void breakdownFire(bool success, bool playerCancel, float progress)
        {
            if (!success)
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Failedattempttostartfire"), false);
            }
            if (success)
            {
                breakdownFire(currentFire);
            }
        }
        public static bool canbreakdownFire(Fire fire)
        {
            
            HeatReservoir heatReservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(((Component)(object)fire).gameObject));

            bool flag = false;
          
            if (heatReservoir.TrackedBurntNames != null)
            {
               
                List<String> names = heatReservoir.TrackedBurntNames;

                if (names.Count > 0)
                {
                    if (names[names.Count - 1] != null&& names[names.Count - 1].StartsWith("GEAR_"))
                    {
                        GearItem gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet(names[names.Count - 1], 1);
                        float cmins = Fire_RV.getCentigradeMinutes(fire) / Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject);
                        flag = GetConditionFromRemainingCentigradminutes(gearItem, cmins) > 0.05f;



                        UnityEngine.Object.Destroy(gearItem.gameObject);
                        UnityEngine.Object.Destroy(gearItem);
                    }
                }
            }
           
            return flag;
        }
        public static void breakdownFire(Fire fire)
        {
            HeatReservoir heatReservoir = Fire_RV.GetHeatReservoir(Utils.GetGuidFromGameObject(((Component)(object)fire).gameObject));

            List<String> names = heatReservoir.TrackedBurntNames;
            List<float> cmin = heatReservoir.TrackedBurntItemsCentigradminutesFire;
            List<float> HP = heatReservoir.TrackedBurntGearItemHP;

            List<GearItem> SpawnedFuel = new List<GearItem>();
            List<float> CminsFuel = new List<float>();
            List<float> deltas = new List<float>();

            float cimreaining = Fire_RV.getCentigradeMinutes(fire) / Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject);

          //  Debug.LogFormat("[Fire_rv] {0}", "counts:  " + names[0] + "       " + cmin.Count.ToString() + "       " + HP.Count.ToString());

            int i = names.Count;
            while (i > 0)               //remove fuel until no more items in the fire
            {
                
                i--;
                if (names[i] != null && names[i].StartsWith("GEAR_"))
                {
                    Debug.LogFormat("[Fire_rv] {0}", "Names:  " + names[i] + "    i: " + i.ToString());



                    GearItem val = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet(names[i], 1);

                    if (val != null)
                    {

                        AccessTools.Field(typeof(GearItem), "m_RolledSpawnChance").SetValue(val, true);
                        val.m_SpawnChance = 100f;
                        val.m_BeenInPlayerInventory = true;
                        val.m_BeenInspected = true;
                        val.m_ItemLooted = true;
                        //GearManager.DestroyNextUpdate(val,  false);


                        if (Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject) == 1f)
                        {
                            Vector3 position = ((Component)(object)fire).transform.position;
                            float d = UnityEngine.Random.Range(0.75f, 1f);
                            int num = UnityEngine.Random.Range(0, 359);
                            Vector3 vector = Quaternion.Euler(0f, num, 0f) * Vector3.forward;
                            ((Component)(object)val).gameObject.transform.forward = vector;
                            Vector3 vector2 = position + vector * d;
                            vector2.y += 1f;
                            val.StickToGroundAndOrientOnSlope(vector2, (NavMeshCheck)1);
                            ((Component)(object)val).gameObject.transform.position += new Vector3(0f, 0.3f, 0f);
                            Rigidbody component = ((Component)(object)val).GetComponent<Rigidbody>();
                            component.isKinematic = false;
                            component.velocity = new Vector3(0f, -0.01f, 0f);
                        }




                        val.m_CurrentHP = val.m_MaxHP * HP[i];
                        float cimsGearwhenadded = Fire_RV.getModifiedHeatIncrease(val) * Fire_RV.getModifiedDuration(val) * 60f;

                        if (HP[i] > 0.01f)
                        {
                            SpawnedFuel.Insert(0, val);
                            deltas.Insert(0, cmin[i] + cimsGearwhenadded - cimreaining);
                            CminsFuel.Insert(0, cimsGearwhenadded);
                            Debug.LogFormat("[Fire_rv] {0}", "Names:  " + names[i].ToString() + "   cmin:" + cmin[i].ToString() + "   cimsGearwhenadded:" + cimsGearwhenadded.ToString() + "   cimreaining:" + cimreaining.ToString() + "     deltas: " + (cmin[i] + cimsGearwhenadded - cimreaining).ToString());
                            cimreaining = cmin[i];
                        }
                        else
                        {
                            Debug.LogFormat("[Fire_rv] {0}", "DestroyNextUpdate:" + val.name);

                            GearManager.DestroyNextUpdate(val, value: true);
                        }
                    }
                }
                //	if (cimreaining < 0) break;			 //or breack because cretigradminutes depeted.
            }
            //float acc = 0;
            /*
            for (i = deltas.Count-1; i>0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {

                    deltas[j] -= deltas[i];
                }

                Debug.LogFormat("[Fire_rv] {0}", "deltas: " + deltas[i].ToString());

            }
            Debug.LogFormat("[Fire_rv] {0}", "deltas: " + deltas[0].ToString());*/

            int ihigh = 0;
            int ilow = 0;
            Debug.LogFormat("[Fire_rv] {0}", SpawnedFuel.Count.ToString());
            
            while(ihigh < SpawnedFuel.Count)
            {
                Debug.LogFormat("[Fire_rv] {0}", "ihigh" + ihigh.ToString()+ "   ilow" + ihigh.ToString()+   "    Count"+ SpawnedFuel.Count.ToString());
                
                int n = (ihigh - ilow + 1);
                //if (n == 0) Debug.LogFormat("[Fire_rv] {0}", "n is 0!!!!!!!!!! ");
                int s = GetIndexOfsmalest(CminsFuel, ilow, ihigh);
                if (s == -1) break;

                Debug.LogFormat("[Fire_rv] {0}", deltas[ihigh].ToString() + "       "+ (CminsFuel[s] * n).ToString());
                while (deltas[ihigh] > CminsFuel[s] * n && SpawnedFuel.Count!=1)
                {
                    //Debug.LogFormat("[Fire_rv] {0}",  "       " + (CminsFuel[s] * n).ToString());
                    float tmpcimns = CminsFuel[s];

                    RemoveInRange(CminsFuel, tmpcimns, ilow, ihigh);

                    deltas[ihigh] -= tmpcimns * n;
                    Debug.LogFormat("[Fire_rv] {0}", "item burnt off  " + s.ToString()+ "   deltas[ihigh]: "+deltas[ihigh].ToString());

                    if (ihigh == s&&s>0) deltas[s - 1] += deltas[s];
                    GearManager.DestroyNextUpdate(SpawnedFuel[s], value: true);
                    SpawnedFuel.RemoveAt(s);
                    deltas.RemoveAt(s);
                    CminsFuel.RemoveAt(s);
                    
                   
                    ihigh--;
                   
                    if (ilow > ihigh) ihigh = ilow;

                    n = (ihigh - ilow + 1);
                    s = GetIndexOfsmalest(CminsFuel, ilow, ihigh);

                    Debug.LogFormat("[Fire_rv] {0}", deltas[ihigh].ToString() + "       " + (CminsFuel[s] * n).ToString());
                }

               
                    RemoveInRange(CminsFuel, deltas[ihigh]/(float)n, ilow, ihigh);
                   

                    deltas[ihigh] = 0;          //just to be sure
                    
                    ihigh++;
                
                    
            }
            
            

            float totgearremoved = 0;
            for(i =0; i < SpawnedFuel.Count; i++ )
            {
                float condition = GetConditionFromRemainingCentigradminutes(SpawnedFuel[i], CminsFuel[i]);
                

                if (SpawnedFuel[i].GetNormalizedCondition() - condition < 0.01f) condition = SpawnedFuel[i].GetNormalizedCondition() - 0.01f;   //minial condition loss

                if (condition > 0.05f)                                                                          //maximal condition loss
                {

                    SpawnedFuel[i].m_CurrentHP = condition * SpawnedFuel[i].m_MaxHP;
                }
                else GearManager.DestroyNextUpdate(SpawnedFuel[i], value: true);

                float cimscondition = Fire_RV.getModifiedHeatIncrease(SpawnedFuel[i]) * Fire_RV.getModifiedDuration(SpawnedFuel[i]) * 60f;
                totgearremoved += cimscondition;
                Debug.LogFormat("cimreaining:  " + cimscondition.ToString() + " condition: " + condition.ToString());
            }

            float tot = Fire_RV.getCentigradeMinutes(fire) / Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject);
            

           

            Debug.LogFormat("cimreaining:  " + totgearremoved.ToString() + " tot: " + tot.ToString());

            for (i = 0; i < HP.Count; i++) HP[i] = 0f;


            float remainingLifeTimeSeconds = fire.GetRemainingLifeTimeSeconds();
            float num2 = (float)AccessTools.Field(typeof(Fire), "m_MaxOnTODSeconds").GetValue(fire) - remainingLifeTimeSeconds;
           // AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").SetValue(fire, 0);
            AccessTools.Field(typeof(Fire), "m_MaxOnTODSeconds").SetValue(fire, num2);

        }
        public static float GetConditionFromRemainingCentigradminutes(GearItem fuel, float Centigradminutes)
        {
            float tmp = fuel.m_CurrentHP;
            fuel.m_CurrentHP = fuel.m_MaxHP;
            float CimsMax = Fire_RV.getModifiedHeatIncrease(fuel) * Fire_RV.getModifiedDuration(fuel)*60f;

            float Condition = Mathf.Sqrt(Mathf.Clamp01(Centigradminutes / CimsMax));

            fuel.m_CurrentHP = tmp;
            return Condition;
        }
        public static int GetIndexOfsmalest(List<float> list, int imin, int imax)
        {
            if (imax >= list.Count) imax = list.Count - 1;
            float tmp = float.PositiveInfinity;
            int itmp = -1;
            for (int i = imin; i <= imax; i++)
            {
                if (list[i] < tmp)
                {
                    tmp = list[i];
                    itmp = i;
                }
            }
            return itmp;
        }
        public static void RemoveInRange(List<float> list,float amount, int imin, int imax)
        {
            if (imax >= list.Count) imax = list.Count - 1;         
            for (int i = imin; i <= imax; i++)
            {
                list[i] -= amount;
            }
           
        }


        public static void spawnTorches(Fire fire, int num_torches)
        {
            GearItem gearItem;
            for (int i = 0; i < num_torches; i++)
            {
                gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet("GEAR_Torch",1);
                if (gearItem == null || gearItem.m_TorchItem == null)
                {
                    return;
                }
                gearItem.OverrideGearCondition(GearStartCondition.Low);
                gearItem.m_TorchItem.m_ElapsedBurnMinutes = gearItem.m_TorchItem.GetModifiedBurnLifetimeMinutes();
                object[] list = { TorchState.BurnedOut };
                AccessTools.Method(typeof(TorchItem),"SetState").Invoke(gearItem.m_TorchItem,list);

                Vector3 position = fire.transform.position;
                float d = UnityEngine.Random.Range(0.3f, 1);
                int num2 = UnityEngine.Random.Range(0, 359);
                Quaternion rotation = Quaternion.Euler(0f, (float)num2, 0f);
                Vector3 a = rotation * Vector3.forward;
                gearItem.gameObject.transform.forward = a;
                Vector3 desiredPosition = position + a * d;
                desiredPosition.y += 1f;
                gearItem.StickToGroundAndOrientOnSlope(desiredPosition, NavMeshCheck.IgnoreNavMesh);
                gearItem.gameObject.transform.position += new Vector3(0, 0.3f, 0);
                Rigidbody component = gearItem.GetComponent<Rigidbody>();
                component.isKinematic = false;
                component.velocity =new  Vector3(0, -0.01f, 0);

            }
        }
    
    public static float getStoveDurationModifier(GameObject curObj)
        {    //these are both about efficiency and also moulding different shapes of fire...


            string firetype = Fire_RV.getFireType(curObj);
            float firetype_mod;
            switch (firetype)
            {
                case "Campfire":
                    firetype_mod = 1.0f;
                    break;
                case "INTERACTIVE_StoveMetalA":
                    firetype_mod = 1.1f;
                    break;
                case "INTERACTIVE_StoveWoodC":
                    firetype_mod = 1.4f;
                    break;
                case "INTERACTIVE_PotBellyStove":
                    firetype_mod = 1.5f;
                    break;
                case "INTERACTIVE_FirePlace":
                    firetype_mod = 1.2f;
                    break;
                case "INTERACTIVE_FireBarrel":
                    firetype_mod = 1.2f;
                    break;
                case "INTERACTIVE_Forge":
                    firetype_mod = 1.4f;
                    break;
                default:
                    Debug.Log("unrecognised fire:" + firetype);
                    firetype_mod = 1.2f;
                    break;
            }
            return firetype_mod;
        }

    public static float getModifiedHeatIncrease(GearItem fuel)
        {
            var setting = Fire_RVSettings.Instance;
            string name = fuel.name;
            float mymod = 1;
           
            switch (name.Substring(5,4))
            {
                case "Stic"://GEAR_Stick
                    mymod = setting.SmalfuelHeat;
                    break;
                case "Torc":// treat torches like sticks
                    mymod = setting.SmalfuelHeat;
                    break;
                case "Book": //  // GEAR_BookManual GEAR_BookD GEAR_BookE   GEAR_BookBopen
                    mymod = setting.SmalfuelHeat;
                    break;
                case "Recl":         //GEAR_ReclaimedWoodB
                    mymod = setting.WoodHeat;
                    break;
                case "Fire":// GEAR_Firelog
                    mymod = setting.WoodHeat;
                    break;
                case "Soft"://GEAR_Softwood
                    mymod = setting.WoodHeat;
                    break;
                case "Hard"://GEAR_Hardwood
                    mymod = setting.WoodHeat;
                    break;
                case "Coal":    //GEAR_Coal
                    mymod = setting.CoalHeat;
                    break;
                default:
                    Debug.Log("unrecognised fuel is treated as tinder:" + fuel.name);
                    mymod = setting.TinderHeat;
                    break;
            }
            float answ=  fuel.m_FuelSourceItem.m_HeatIncrease * fuel.GetNormalizedCondition() * mymod;
           // if (name.Substring(5, 4) == "Torc") answ = 1f *  mymod; //treat torches like sticks

           // Debug.Log("Fire.getModifiedHeatIncrease for " + name + ":" + fuel.m_FuelSourceItem.m_HeatIncrease + " to:" + answ);
            return answ;

        }
        public static float getModifiedDuration(GearItem fuel)
        {
            var setting = Fire_RVSettings.Instance;
            string name = fuel.name;
            
            float mymod;
            switch (name.Substring(5, 4))
            {
                case "Stic":// 0.13h  1c  0.13ch =>  x3 x3    9Xch //change this remember to update hard code in on take torch
                    mymod = setting.SmalfuelBurntime;
                    break;
                case "Torc":// 0.13h  1c  0.13ch =>  0.25h  6c   1.5ch //change this remember to update hard code in on take torch
                    mymod = setting.SmalfuelBurntime;
                    break;
                case "Book": // 0.3h  2c  0.6ch  =>  0.6h  6c   3.6ch  
                    mymod = setting.SmalfuelBurntime; 
                    break;
                case "Recl": // 0.5h  3c  1.5ch  =>  1.5h 10c  15.0ch  
                    mymod = setting.WoodBurntime;
                    break;
                case "Fire"://  2.0h  3c  6.0ch  =>  4.0h  6c  24.0ch
                    mymod = setting.WoodBurntime;
                    break;
                case "Soft"://  1.0h  6c  6.0ch  =>  3.0h 12c  36.0ch
                    mymod = setting.WoodBurntime;
                    break;
                case "Hard"://  1.5h  8c  12.0ch =>  4.5h 16c  72.0ch
                    mymod = setting.WoodBurntime;
                    break;
                case "Coal"://  1.0h 20c  20.0ch =>  4.0h 20c  80.0ch
                    mymod = setting.CoalBurntime;
                    break;                         
                default:
                    Debug.Log("unrecognised is treated as tinder:" + fuel.name);
                    mymod = setting.TinderBurntime;
                    break;
            }
            if (ThreeDaysOfNight.IsActive())
            {
                mymod *= 2f;
            }
           float answ=  fuel.m_FuelSourceItem.m_BurnDurationHours * GameManager.GetSkillFireStarting().GetDurationScale() * fuel.GetNormalizedCondition() * mymod;
           // if (name.Substring(5, 4)=="Torc") answ = 0.13f * GameManager.GetSkillFireStarting().GetDurationScale()  * mymod; //treat torches like sticks
           // Debug.Log("Fire.getModifiedBurnDuration for " + name + ":" + fuel.m_FuelSourceItem.m_BurnDurationHours + " to:" + answ);
            return answ;
        }

        public static float torch_cost(Fire fire)
        {
            GearItem gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet("GEAR_Torch", 1);
            float centisecs = getStoveDurationModifier(fire.gameObject) * 1.2f * getModifiedDuration(gearItem) * 3600 * getModifiedHeatIncrease(gearItem);
            //Debug.Log("In torch cost, stove:" + getStoveDurationModifier(fire.gameObject) + " time:" + getModifiedDuration(gearItem) + " heat:" + getModifiedHeatIncrease(gearItem) + " centisecs:" + centisecs);
            UnityEngine.Object.Destroy(gearItem.gameObject);
            UnityEngine.Object.Destroy(gearItem);
            return centisecs;
        }

     
        public static HeatReservoir GetHeatReservoir(string GUID)
        {
            HeatReservoir found = null;
            if (myreservoirs == null) return null;

            for (int i = 0; i < myreservoirs.Count; i++)
            {
               // Debug.Log(GUID + ":" + myreservoirs[i].GUID + "=" + (GUID == myreservoirs[i].GUID));
;                if (GUID == myreservoirs[i].GUID)
                {
                    found = myreservoirs[i];
                }
              
            }
            return found;
        }
        public static void RemoveReservoir(string GUID)
        {
            if (myreservoirs == null)return;
            
            for (int i = 0; i < myreservoirs.Count; i++)
            {
                // Debug.Log("Count:"+myreservoirs.Count);
                if (GUID == myreservoirs[i].GUID) myreservoirs.RemoveAt(i);
            }
        }

        public static void CreateHeatReservoir(string GUID)
        {
            if (myreservoirs == null) { myreservoirs = new List<HeatReservoir>(); }
            HeatReservoir newRes = new HeatReservoir();
            newRes.GUID = GUID;
            newRes.ReservoirCreationVerion = GetCurrentVersion();
            newRes.temp = 0;
 
            newRes.size_cmins = GetReservoirSize(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            newRes.size_cmins_max = GetReservoirSizeMax(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            if (newRes.size_cmins_max < newRes.size_cmins) newRes.size_cmins_max = newRes.size_cmins;

                newRes.insulation_factor = GetReservoirIns(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            newRes.fireLastOnAt = -1;
            newRes.lastUpdate = -1;
            newRes.embercmins = 0;
            newRes.heatingsize = 5;
            newRes.TrackedBurntNames= new List<string>
            {
                LastBurntItemsName
            };
            newRes.TrackedBurntItemsCentigradminutesFire = new List<float>
            {
                LastBurntItemsCentigradminutesFire
            };
            newRes.TrackedBurntGearItemHP = new List<float>
            {
                LastBurntGearItemHP
            };

            // newRes.lastBackgroundTemp = GameManager.GetWeatherComponent().GetCurrentTemperatureWithoutHeatSources();
            Debug.Log("Creating new heat reservoir");
            Debug.Log(Utils.SerializeObject(newRes));
            myreservoirs.Add(newRes);
        }


        internal static float GetReservoirSize(string regname)
        {
            if (!heatResSizes.ContainsKey(regname))
            {
                Debug.LogWarningFormat("Scene \"{0}\" has not been associated with a heat reservoir:", regname);
                return 0;
            }

            float answ = heatResSizes[regname];

            if (answ == 1) //default value test flags
            {

                if (GameManager.GetWeatherComponent().IsIndoorEnvironment() && !(GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger && GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature))
                {
                    //game definition of indoor environment, base insulation off of the indoor warmth variable
                    answ = 900;
                }
                else if (GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger)
                {
                    //back of cave, high reservoir low insulation
                    answ = 1500;
                }
                else
                {
                    //outside
                    answ = 1;
                }

            }
            Debug.Log("Heat Reservoir Size:" + answ);
            return answ ;
        }

        internal static float GetReservoirSizeMax(string regname)
        {
            if (!heatResSizesMax.ContainsKey(regname))
            {
                Debug.LogWarningFormat("Scene \"{0}\" has not been associated with a heat reservoir:", regname);
                return 0;
            }

            float answ = heatResSizesMax[regname];

            if (answ == 1) //default value test flags
            {
                /*
                if (GameManager.GetWeatherComponent().IsIndoorEnvironment() && !(GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger && GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature))
                {
                    //game definition of indoor environment, base insulation off of the indoor warmth variable
                    answ = 900;
                }
                else if (GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger)
                {
                    //back of cave, high reservoir low insulation
                    answ = 1500;
                }
                else
                {
                    //outside
                    answ = 1;
                }*/

            }
            Debug.Log("Heat Reservoir Size Max:" + answ);
            return answ ;
        }

        internal static float GetReservoirIns(string regname)
        {
            if (!heatResIns.ContainsKey(regname))
            {
                Debug.LogWarningFormat("Scene \"{0}\" has not been associated with a heat insulation:", regname);
                return 0;
            }

            float answ = heatResIns[regname];

            if (answ == 1) //default value test flags
            {

                if (GameManager.GetWeatherComponent().IsIndoorEnvironment() && !(GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger && GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature))
                {
                    //game definition of indoor environment, base insulation off of the indoor warmth variable 
                    //cannt do that there are indoor locations with negativ m_IndoorTemperatureCelsius
                    answ = GameManager.GetWeatherComponent().m_IndoorTemperatureCelsius + 10f; 
                }
                else if (GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger)
                {
                    //back of cave, high reservoir low insulation
                    answ = 2;
                }
                else
                {
                    //outside
                    answ = 0.05f;
                }

            }
            Debug.Log("Heat Reservoir Insulation:" + answ);
            return answ;
            
        }

        public static float unitime()
        {
            //maybe want to make this proofed against weird daycounter changes on scene loading...
            return GameManager.GetUniStorm().m_DayCounter + GameManager.GetUniStorm().m_NormalizedTime;
        }

    }


    public class HeatReservoir
    {
        public string GUID;
        public string ReservoirCreationVerion;
        public float temp;
        public float insulation_factor;
        public float size_cmins;
        public float size_cmins_max;        //for cave and mines
        public float lastUpdate;
        public float lastFireTemp;
        public float fireLastOnAt;
        public float lastBackgroundTemp;//this can be deleted

        public float heatingsize;

        public float embercmins;

        public List<string> TrackedBurntNames;

        public List<float> TrackedBurntItemsCentigradminutesFire;

        public List<float> TrackedBurntGearItemHP;


        public void Update(Fire fire)
        {

            float fireTemp = (float)AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").GetValue(fire);

            if (lastUpdate == -1) lastUpdate = Fire_RV.unitime();

            float minutespassed = (Fire_RV.unitime() - lastUpdate) * 24 * 60;

            float step = 0.05f;
            float fuel_heat;
            while (minutespassed > 0)
            {
                if (minutespassed < step) step = minutespassed;
                fuel_heat = 0;
                if (fireTemp==0 && fireLastOnAt > (Fire_RV.unitime() - minutespassed / (24 * 60)))
                {
                    Debug.Log("buffing reservoir from a passed fire laston:" + fireLastOnAt + " now:" + (Fire_RV.unitime() - minutespassed / (24 * 60)));
                    fuel_heat = lastFireTemp;
                }
                else if(fireTemp>0)
                {
                    fuel_heat = fireTemp;
                }
                
                updateEmber(fire, step);
                if(insulation_factor<=0)
                {
                    insulation_factor = Fire_RV.GetReservoirIns(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }

                float size = Mathf.Lerp(size_cmins, size_cmins_max, heatingsize / 2000f);                   // Pool size dependent on heatingsize, for lage cavesystems

                float fuel_buff = step * (fuel_heat / size);
                float rad_loss = step * temp / (insulation_factor * size);

                if (temp > 1    || ((fuel_buff - rad_loss)<0)&& temp!=0) heatingsize *= 1 + (fuel_buff - rad_loss) / temp;                           //expand/shrink size with increasing/falling res_temp
                //else heatingsize = 10;


                temp += fuel_buff - rad_loss;
                
                //Debug.Log("minutes passed:"+minutespassed+" fuel_buff:"+fuel_buff+"rad_loss"+rad_loss+"res_temp:"+ this.temp+ " fuelheat:"+fuel_heat);
                //Debug.Log("Reservoir temp:" + temp +" delta:"+ (insulation_factor).ToString());
                minutespassed -= step;

            }
            lastUpdate = Fire_RV.unitime();
            
            if (fireTemp>0)
            {
                lastFireTemp = fireTemp; //we need this to catch up reservoir.
                fireLastOnAt = Fire_RV.unitime();
            }


        }
        public bool updateEmber(Fire fire, float mins)
        {
            float cmins =Fire_RV.getCentigradeMinutes(fire);
            float temperature = (float)AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").GetValue(fire);

           //Debug.Log("cmins:" + cmins.ToString() + " embercmins:" + embercmins.ToString());

            float accrate = 0.001f;
            float loserate = 1f;


            if (cmins> embercmins&& cmins!=0)
            {
                float delta = (cmins - embercmins) * accrate * mins;
                embercmins += delta;

                float deltarel = 1 - delta / cmins;
                AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").SetValue(fire, temperature* deltarel);
            }
           
            if(fire.IsEmbers())     
            {
                float embercminsold = embercmins;
                float delta = loserate * mins * temperature;
                embercmins -= delta;

                float tnew;
                if (embercminsold>0) tnew = temperature * embercmins / embercminsold;
                else tnew = 0;

             
                AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").SetValue(fire, tnew);


                TrackedBurntNames.Clear();

                TrackedBurntItemsCentigradminutesFire.Clear();

                TrackedBurntGearItemHP.Clear();

            }
            else if(!fire.IsBurning())
            {
                TrackedBurntNames.Clear();

                TrackedBurntItemsCentigradminutesFire.Clear();

                TrackedBurntGearItemHP.Clear();

                AccessTools.Field(typeof(Fire), "m_FuelHeatIncrease").SetValue(fire, 0);

            }

            return true;
        }
    }
}
    




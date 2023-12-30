
using Il2Cpp;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEMPLATE;
using UnityEngine.AddressableAssets;

namespace ImprovedFires
{
    class Fire_RV
    {
        public static List<HeatReservoir>? myreservoirs;

        public static bool scene_loading = true;

        public static Fire currentFire = null;

        public static float LastBurntItemsCentigradminutesFire;

        public static float LastBurntGearItemHP;

        public static GearItem? LastBurntItem;

        public static List<string> tinderitems =
[
            "GEAR_NewsprintRoll",
    "GEAR_PaperStack",
    "GEAR_Newsprint",

    "GEAR_CashBundle",
    "GEAR_BarkTinder",
    "GEAR_Tinder",
    "GEAR_CattailTinder",
];






        public static void OnLoad()
        {
            ////Debug.Log("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);


            uConsole.RegisterCommand("fire-log", new Action(FireLog));
        }
        private static System.Action LogReturn(System.Func<object> commandReturn)
        {
            return () =>
            {
                object result = commandReturn();
                uConsole.Log(result == null ? "(null)" : result.ToString());
            };
        }

        internal static void FireLog()
        {
            Vector3 pos = GameManager.GetPlayerTransform().position;
            //list fires with heat sources
            for (int i = 0; i < FireManager.m_Fires.Count; i++)
            {
                Fire fire = FireManager.m_Fires[i];
                float dist = Vector3.Distance(pos, fire.transform.position);
                //Debug.Log("Fire " + i + " lit;" + fire.IsBurning() + " dist:" + dist + " fire GUID:" + ObjectGuid.GetGuidFromGameObject(fire.gameObject) + " heatsource GUID:" + ObjectGuid.GetGuidFromGameObject(fire.m_HeatSource.gameObject));
                //Debug.Log("\tHeat Reservoir " + JSON.Dump(GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(fire.gameObject))));

            }

            var myheats = GameManager.GetHeatSourceManagerComponent().m_HeatSources;
            for (int i = 0; i < myheats.Count; i++)
            {
                HeatSource myheat = myheats[i];
                float dist = Vector3.Distance(pos, myheat.transform.position);
                //Debug.Log("Heat " + i + " temp;" + myheat.GetCurrentTempIncrease() + " dist:" + dist + " heatsource GUID:" + ObjectGuid.GetGuidFromGameObject(myheat.gameObject));
            }

            for (int i = 0; i < myreservoirs.Count; i++)
            {
                HeatReservoir myheatres = myreservoirs[i];
                //Debug.Log("Res " + i + ":" + JSON.Dump(myreservoirs[i]));
            }


        }


        public static bool WulfFirePackInstalled()
        {
            try
            {
                string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Assembly assembly = Assembly.LoadFrom(Path.Combine(directoryName, "Fire-Pack.dll"));
                return assembly != null;
            }
            catch
            {
            }
            return false;
        }



        private const string SAVE_FILE_NAME = "fire_rv-settings";

        public static string serialize()
        {
            StringArray stringArray = new StringArray();
            stringArray.strings = new string[1];
            if (myreservoirs == null) myreservoirs = new List<HeatReservoir>();
            stringArray.strings[0] = JSON.Dump(myreservoirs);
            //Debug.Log("finished serializing myreservoirs, count:" + myreservoirs.Count);
            return (Utils.SerializeObject(stringArray));
        }

        public static void deserialize(string serialString)
        {
            StringArray stringArray = Utils.DeserializeObject<StringArray>(serialString);
            myreservoirs = Utils.DeserializeObject<List<HeatReservoir>>(stringArray.strings[0]);
            List<HeatReservoir> toDelete = new List<HeatReservoir>();

            string version = GetCurrentVersion();


            foreach (HeatReservoir HR in myreservoirs) if (HR.ReservoirCreationVerion == null || HR.ReservoirCreationVerion != version) toDelete.Add(HR);
            foreach (HeatReservoir HR in toDelete) myreservoirs.Remove(HR);


            //Debug.Log("finished deserializing myreservoirs, count:" + myreservoirs.Count);
        }


        internal static string GetCurrentVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;

        }


        public static void PrintHierarchy(GameObject obj)
        {
            //Debug.Log("Printing Hierarchy for:" + obj.name + " :" + obj.GetType());
            while (obj.transform && obj.transform.parent && obj.transform.parent.gameObject)
            {
                obj = obj.transform.parent.gameObject;
            }
            //Debug.Log(Fire_RV.PrintSubHierarchy(obj, 0));


        }

        public static string PrintSubHierarchy(GameObject obj, int depth)
        {
            //string tabs = "";
            //for (int j = 0; j < depth; j++) tabs += "\t";
            //string myres = tabs + obj.name + " :" + obj.GetType() + " " + obj.transform.childCount + " components:";
            //Component[] mycomps = obj.GetComponents(typeof(Component));
            //for (int k = 0; k < mycomps.Length; k++)
            //{
            //    myres += " " + k + ":" + mycomps[k].GetType().Name;
            //}
            //myres += " Children:\n";


            //// for (int j=0;j<obj.g)
            //for (int i = 0; i < obj.transform.childCount; i++)
            //{
            //    myres = myres + PrintSubHierarchy(obj.transform.GetChild(i).gameObject, depth + 1);
            //}
            //return myres;
            return null;
        }

        public static string getFireType(GameObject curObj)
        {
            string fire_type = "Campfire";
            bool done = false;
            while (curObj.transform.parent && !done)
            {
                if (curObj.transform.parent.gameObject.name.Length > 12 && curObj.transform.parent.gameObject.name.Substring(0, 11) == "INTERACTIVE")
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

        public static float skillFireHeatingFactor()
        {

            return 1f / (1f + GameManager.GetSkillFireStarting().GetCurrentTierNumber());


		}
    
		public static float getCentigradeMinutes(Fire fire)
        {
            float time_remaining = fire.GetRemainingLifeTimeSeconds() / 60f;
            float temperature = fire.m_HeatSource.m_MaxTempIncrease;
			return temperature * time_remaining;
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
            HeatReservoir myreservoir = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(fire.gameObject));
            float embertime = 0;
            if (myreservoir != null) embertime = myreservoir.embercmins / fire.m_HeatSource.m_MaxTempIncrease;

            float time_remaining = fire.GetRemainingLifeTimeSeconds() / 60f + embertime;
            float temperature = fire.m_HeatSource.m_MaxTempIncrease;
            return temperature / time_remaining;
        }
        public static bool ReworkedFireBlowOut(float MaxOnTODSeconds, float ElapsedOnTODSeconds, float TempIncrease)
        {
            float num = Mathf.Clamp(MaxOnTODSeconds - ElapsedOnTODSeconds, 0f, float.PositiveInfinity) * TempIncrease;
            float blowoutzone = GameManager.GetFireManagerComponent().m_TODMinutesFadeOutFireAudio * 60f * GameManager.GetWindComponent().GetSpeedMPH() / (GameManager.GetSkillFireStarting().GetCurrentTierNumber() + 1);

            //Debug.Log("ReworkedFireBlowOut:" + num.ToString() + "   " + blowoutzone.ToString());
            if (num < blowoutzone) return true;

            return false;
		}


        public static void heapFire(Fire fire)
        {
			if (Il2Cpp.Utils.RollChance(10)) GameManager.GetSkillsManager().IncrementPointsAndNotify(SkillType.Firestarting, 1, SkillsManager.PointAssignmentMode.AssignOnlyInSandbox);
			//Debug.Log("In heap fire:" + getHeatTimeRatio(fire));
            float time_remaining = fire.GetRemainingLifeTimeSeconds();
            float temperature = fire.m_HeatSource.m_MaxTempIncrease;

            float MaxOnTod = fire.m_MaxOnTODSeconds;

            float newMaxOnTod = MaxOnTod - time_remaining + time_remaining / 1.2f;
			fire.m_MaxOnTODSeconds = newMaxOnTod;
            if (getHeatTimeRatio(fire) < 1f)
            {
                fire.m_HeatSource.m_MaxTempIncrease = temperature * 1.2f;
                fire.m_HeatSource.m_TempIncrease = (fire.m_HeatSource.m_TempIncrease - Fire_RV.skillFireHeatingFactor()) * 1.1f;
            }
            else fire.m_HeatSource.m_TempIncrease = (fire.m_HeatSource.m_TempIncrease - Fire_RV.skillFireHeatingFactor()) * 1.5f;
			
            fire.m_HeatSource.m_TimeToReachMaxTempMinutes /= 1.5f;

            if (fire.m_HeatSource.m_TempIncrease <= 0)
            {
                breakdownFire(fire);
			}

		}



        public static void spreadFire(Fire fire)
        {
			if (Il2Cpp.Utils.RollChance(10)) GameManager.GetSkillsManager().IncrementPointsAndNotify(SkillType.Firestarting, 1, SkillsManager.PointAssignmentMode.AssignOnlyInSandbox);
			//Debug.Log("In spread fire:" + getHeatTimeRatio(fire));
            float time_remaining = fire.GetRemainingLifeTimeSeconds();
            float temperature = fire.m_HeatSource.m_MaxTempIncrease;

            float MaxOnTod = fire.m_MaxOnTODSeconds;

            float newMaxOnTod = MaxOnTod - time_remaining + time_remaining * 1.2f;
			if (fire.m_HeatSource.m_TempIncrease / 1.5f - Fire_RV.skillFireHeatingFactor() <= 0)
                {
                breakdownFire(fire);
                return;
                    }


			fire.m_HeatSource.m_MaxTempIncrease = temperature / 1.2f;
            fire.m_HeatSource.m_TempIncrease = fire.m_HeatSource.m_TempIncrease / 1.5f - Fire_RV.skillFireHeatingFactor();


			fire.m_MaxOnTODSeconds = newMaxOnTod;


        }


         public static void breakdownFire(Fire fire)
        {
			HeatReservoir heatReservoir = Fire_RV.GetHeatReservoir(ObjectGuid.GetGuidFromGameObject(((Component)(object)fire).gameObject));

            List<string> Items = heatReservoir.TrackedBurntItemsNames;
            List<float> cmin = heatReservoir.TrackedBurntItemsCentigradminutesFire;
            List<float> HP = heatReservoir.TrackedBurntGearItemHP;

            List<GearItem> SpawnedFuel = new List<GearItem>();
            List<float> CminsFuel = new List<float>();
            List<float> deltas = new List<float>();

            float cimreaining = Fire_RV.getCentigradeMinutes(fire) / Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject);

            //Debug.LogFormat("[Fire_rv] {0}", "counts:  " + Items.Count + "       " + cmin.Count.ToString() + "       " + HP.Count.ToString());

            int i = Items.Count;
            while (i > 0)               //remove fuel until no more items in the fire
            {

                i--;
                if (Items[i] != null)
                {
                    //Debug.Log("Names:  " + Items[i] + "    i: " + i.ToString());



                    GearItem val = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet(Addressables.LoadAssetAsync<GameObject>(Items[i]).WaitForCompletion().GetComponent<GearItem>(), 1);

                    if (val != null)
                    {

                        val.m_RolledSpawnChance = true;
                        val.m_SpawnChance = 100f;
                        val.m_BeenInPlayerInventory = true;
                        val.m_BeenInspected = true;
                        val.m_ItemLooted = true;
                        //GearManager.DestroyNextUpdate(val,  false);


                        //if (Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject) == 1f)
                        //{
                        //    Vector3 position = ((Component)(object)fire).transform.position;
                        //    float d = UnityEngine.Random.Range(0.75f, 1f);
                        //    int num = UnityEngine.Random.Range(0, 359);
                        //    Vector3 vector = Quaternion.Euler(0f, num, 0f) * Vector3.forward;
                        //    ((Component)(object)val).gameObject.transform.forward = vector;
                        //    Vector3 vector2 = position + vector * d;
                        //    vector2.y += 1f;
                        //    val.StickToGroundAndOrientOnSlope(vector2, (NavMeshCheck)1, 0);
                        //    ((Component)(object)val).gameObject.transform.position += new Vector3(0f, 0.3f, 0f);
                        //    Rigidbody component = ((Component)(object)val).GetComponent<Rigidbody>();
                        //    component.isKinematic = false;
                        //    component.velocity = new Vector3(0f, -0.01f, 0f);
                        //}




                        val.m_CurrentHP = HP[i];
                        


						float cimsGearwhenadded = Fire_RV.getModifiedHeatIncrease(val) * Fire_RV.getModifiedDuration(val) * 60f;

                        if (HP[i] > 0.01f)
                        {
                            SpawnedFuel.Insert(0, val);
                            deltas.Insert(0, cmin[i] + cimsGearwhenadded - cimreaining);
                            CminsFuel.Insert(0, cimsGearwhenadded);
                            //Debug.Log("Names:  " + Items[i].ToString() + "   cmin:" + cmin[i].ToString() + "   cimsGearwhenadded:" + cimsGearwhenadded.ToString() + "   cimreaining:" + cimreaining.ToString() + "     deltas: " + (cmin[i] + cimsGearwhenadded - cimreaining).ToString());
                            cimreaining = cmin[i];
                        }
                        else
                        {
//Debug.Log("DestroyNextUpdate:" + val.name);

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

                //Debug.LogFormat("[Fire_rv] {0}", "deltas: " + deltas[i].ToString());

            }
            //Debug.LogFormat("[Fire_rv] {0}", "deltas: " + deltas[0].ToString());*/

            int ihigh = 0;
            int ilow = 0;
            //Debug.Log(SpawnedFuel.Count.ToString());

            while (ihigh < SpawnedFuel.Count)
            {
                //Debug.Log("ihigh" + ihigh.ToString() + "   ilow" + ihigh.ToString() + "    Count" + SpawnedFuel.Count.ToString());

                int n = (ihigh - ilow + 1);
                //if (n == 0) //Debug.LogFormat("[Fire_rv] {0}", "n is 0!!!!!!!!!! ");
                int s = GetIndexOfsmalest(CminsFuel, ilow, ihigh);
                if (s == -1) break;

                //Debug.Log(deltas[ihigh].ToString() + "       " + (CminsFuel[s] * n).ToString());
                while (deltas[ihigh] > CminsFuel[s] * n && SpawnedFuel.Count != 1)
                {
                    ////Debug.LogFormat("[Fire_rv] {0}",  "       " + (CminsFuel[s] * n).ToString());
                    float tmpcimns = CminsFuel[s];

                    RemoveInRange(CminsFuel, tmpcimns, ilow, ihigh);

                    deltas[ihigh] -= tmpcimns * n;
                    //Debug.Log("item burnt off  " + s.ToString() + "   deltas[ihigh]: " + deltas[ihigh].ToString());

                    if (ihigh == s && s > 0) deltas[s - 1] += deltas[s];
                    GearManager.DestroyNextUpdate(SpawnedFuel[s], value: true);
                    SpawnedFuel.RemoveAt(s);
                    deltas.RemoveAt(s);
                    CminsFuel.RemoveAt(s);


                    ihigh--;

                    if (ilow > ihigh) ihigh = ilow;

                    n = (ihigh - ilow + 1);
                    s = GetIndexOfsmalest(CminsFuel, ilow, ihigh);

                    //Debug.Log(deltas[ihigh].ToString() + "       " + (CminsFuel[s] * n).ToString());
                }


                RemoveInRange(CminsFuel, deltas[ihigh] / (float)n, ilow, ihigh);


                deltas[ihigh] = 0;          //just to be sure

                ihigh++;

		

			}

			heatReservoir.TrackedBurntItemsNames.Clear();
			heatReservoir.TrackedBurntItemsCentigradminutesFire.Clear();
			heatReservoir.TrackedBurntGearItemHP.Clear();

			float totgearremoved = 0;
            for (i = 0; i < SpawnedFuel.Count; i++)
            {
                float condition = GetConditionFromRemainingCentigradminutes(SpawnedFuel[i], CminsFuel[i]);


                if (SpawnedFuel[i].GetNormalizedCondition() - condition < 0.01f) condition = SpawnedFuel[i].GetNormalizedCondition() - 0.01f;   //minial condition loss

                if (condition > 0.05f)                                                                          //maximal condition loss
                {

                    SpawnedFuel[i].m_CurrentHP *= condition;
                    SpawnedFuel[i].WeightKG *= SpawnedFuel[i].GetNormalizedCondition();

				}
				else GearManager.DestroyNextUpdate(SpawnedFuel[i], value: true);

                float cimscondition = Fire_RV.getModifiedHeatIncrease(SpawnedFuel[i]) * Fire_RV.getModifiedDuration(SpawnedFuel[i]) * 60f;
                totgearremoved += cimscondition;
                //Debug.Log("cimreaining:  " + cimscondition.ToString() + " condition: " + condition.ToString());
            }

            float tot = Fire_RV.getCentigradeMinutes(fire) / Fire_RV.getStoveDurationModifier(((Component)(object)fire).gameObject);




            //Debug.Log("cimreaining:  " + totgearremoved.ToString() + " tot: " + tot.ToString());

            for (i = 0; i < HP.Count; i++) HP[i] = 0f;


            float remainingLifeTimeSeconds = fire.GetRemainingLifeTimeSeconds();
            float num2 = fire.m_MaxOnTODSeconds - remainingLifeTimeSeconds;
            // AccessTools.Field(typeof(Fire), "m_HeatSource.m_MaxTempIncrease").SetValue(fire, 0);
            fire.m_MaxOnTODSeconds = num2;

        }
        public static float GetConditionFromRemainingCentigradminutes(GearItem fuel, float Centigradminutes)
        {
            float tmp = fuel.m_CurrentHP;
            fuel.m_CurrentHP = fuel.m_CurrentHP/fuel.GetNormalizedCondition();
            float CimsMax = Fire_RV.getModifiedHeatIncrease(fuel) * Fire_RV.getModifiedDuration(fuel) * 60f;

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
        public static void RemoveInRange(List<float> list, float amount, int imin, int imax)
        {
            if (imax >= list.Count) imax = list.Count - 1;
            for (int i = imin; i <= imax; i++)
            {
                list[i] -= amount;
            }

        }


        public static void spawnTorches(Fire fire, int num_torches)
        {
            //GearItem gearItem;
            //for (int i = 0; i < num_torches; i++)
            //{
            //    gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet(new GearItem(), 1);
            //    if (gearItem == null || gearItem.m_TorchItem == null)
            //    {
            //        return;
            //    }
            //    gearItem.OverrideGearCondition(GearStartCondition.Low, true);
            //    gearItem.m_TorchItem.m_ElapsedBurnMinutes = gearItem.m_TorchItem.GetModifiedBurnLifetimeMinutes();

            //    gearItem.m_TorchItem.SetState(TorchState.BurnedOut);


            //    Vector3 position = fire.transform.position;
            //    float d = UnityEngine.Random.Range(0.3f, 1);
            //    int num2 = UnityEngine.Random.Range(0, 359);
            //    Quaternion rotation = Quaternion.Euler(0f, (float)num2, 0f);
            //    Vector3 a = rotation * Vector3.forward;
            //    gearItem.gameObject.transform.forward = a;
            //    Vector3 desiredPosition = position + a * d;
            //    desiredPosition.y += 1f;
            //    gearItem.StickToGroundAndOrientOnSlope(desiredPosition, NavMeshCheck.IgnoreNavMesh, 0);
            //    gearItem.gameObject.transform.position += new Vector3(0, 0.3f, 0);
            //    Rigidbody component = gearItem.GetComponent<Rigidbody>();
            //    component.isKinematic = false;
            //    component.velocity = new Vector3(0, -0.01f, 0);

            //}
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
                    //Debug.Log("unrecognised fire:" + firetype);
                    firetype_mod = 1.2f;
                    break;
            }
            return firetype_mod;
        }

        public static float getModifiedHeatIncrease(GearItem fuel)
        {
            var setting = Settings.Instance;
            string name = fuel.name;
            float mymod = 1;

            switch (name.Substring(5, 4))
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
                case "Char": //  1.0h 20c  20.0ch =>  4.0h 20c  80.0ch
                    mymod = setting.CoalHeat;
                    break;
                default:
                    //Debug.Log("unrecognised fuel is treated as tinder:" + fuel.name);
                    mymod = setting.TinderHeat;
                    break;
            }
            float answ = fuel.m_FuelSourceItem.m_HeatIncrease * fuel.GetNormalizedCondition() * mymod;
            // if (name.Substring(5, 4) == "Torc") answ = 1f *  mymod; //treat torches like sticks

            //Debug.Log("Fire.getModifiedHeatIncrease for " + name + ":" + fuel.m_FuelSourceItem.m_HeatIncrease + " to:" + answ);
            return answ;

        }
        public static float getModifiedDuration(GearItem fuel)
        {
            var setting = Settings.Instance;
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
                case "Coal"://  1.0h 20c  20.0ch =>  4.0h 20c  160.0ch
                    mymod = setting.CoalBurntime;
                    break;
                case "Char":
                    mymod = setting.CoalBurntime;
                    break;
                default:
                    //Debug.Log("unrecognised is treated as tinder:" + fuel.name);
                    mymod = setting.TinderBurntime;
                    break;
            }
            if (ThreeDaysOfNight.IsActive())
            {
                mymod *= 2f;
            }
            float answ = fuel.m_FuelSourceItem.m_BurnDurationHours * GameManager.GetSkillFireStarting().GetDurationScale() * fuel.GetNormalizedCondition() * mymod;
            // if (name.Substring(5, 4)=="Torc") answ = 0.13f * GameManager.GetSkillFireStarting().GetDurationScale()  * mymod; //treat torches like sticks
            // //Debug.Log("Fire.getModifiedBurnDuration for " + name + ":" + fuel.m_FuelSourceItem.m_BurnDurationHours + " to:" + answ);
            return answ;
        }

        public static float torch_cost(Fire fire)
        {
            //TODO:
            //GearItem gearItem = GameManager.GetPlayerManagerComponent().InstantiateItemAtPlayersFeet("GEAR_Torch", 1);
            //         float centisecs = getStoveDurationModifier(fire.gameObject) * 1.2f * getModifiedDuration(gearItem) * 3600 * getModifiedHeatIncrease(gearItem);
            //         ////Debug.Log("In torch cost, stove:" + getStoveDurationModifier(fire.gameObject) + " time:" + getModifiedDuration(gearItem) + " heat:" + getModifiedHeatIncrease(gearItem) + " centisecs:" + centisecs);
            //         UnityEngine.Object.Destroy(gearItem.gameObject);
            //         UnityEngine.Object.Destroy(gearItem);
            //         return centisecs;
            return 5;

		}


        public static HeatReservoir GetHeatReservoir(string GUID)
        {
            HeatReservoir found = null;
            if (myreservoirs == null)
            {
                CreateHeatReservoir(GUID);
                return GetHeatReservoir(GUID);
            }


			for (int i = 0; i < myreservoirs.Count; i++)
            {
                ////Debug.Log(GUID + ":" + myreservoirs[i].GUID + "=" + (GUID == myreservoirs[i].GUID));
                if (GUID == myreservoirs[i].GUID)
                {
                    found = myreservoirs[i];
                }


            }
            if (found == null)
            {
                CreateHeatReservoir(GUID);
                return GetHeatReservoir(GUID);

			}


			return found;
        }
        public static void RemoveReservoir(string GUID)
        {
            if (myreservoirs == null) return;

            for (int i = 0; i < myreservoirs.Count; i++)
            {
                // //Debug.Log("Count:"+myreservoirs.Count);
                if (GUID == myreservoirs[i].GUID) myreservoirs.RemoveAt(i);
            }
        }

        public static void CreateHeatReservoir(string GUID)
        {
            if (myreservoirs == null) { myreservoirs = new List<HeatReservoir>(); }
            HeatReservoir newRes = new HeatReservoir();
            newRes.GUID = GUID;
            newRes.ReservoirCreationVerion = GetCurrentVersion();

            newRes.lastUpdate = -1;
            newRes.embercmins = 0;

            newRes.TrackedBurntItemsNames = new List<string>();
            newRes.TrackedBurntItemsCentigradminutesFire = new List<float>();
            newRes.TrackedBurntGearItemHP = new List<float>();

            // newRes.lastBackgroundTemp = GameManager.GetWeatherComponent().GetCurrentTemperatureWithoutHeatSources();
            //Debug.Log("Creating new heat reservoir");
            //Debug.Log(JSON.Dump(newRes));
            myreservoirs.Add(newRes);
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

        public float lastUpdate;
        public float lastFireTemp;
        public float fireLastOnAt;
        public float lastBackgroundTemp;//this can be deleted

        public float heatingsize;

        public float embercmins;

        public List<string> TrackedBurntItemsNames;

        public List<float> TrackedBurntItemsCentigradminutesFire;

        public List<float> TrackedBurntGearItemHP;


        public void Update(Fire fire)
        {

            float fireTemp = fire.m_HeatSource.m_MaxTempIncrease;

            if (lastUpdate == -1) lastUpdate = Fire_RV.unitime();

            float minutespassed = (Fire_RV.unitime() - lastUpdate) * 24 * 60;

            float step = 0.05f;

            while (minutespassed > 0)
            {
                if (minutespassed < step) step = minutespassed;

                updateEmber(fire, step);


                ////Debug.Log("Update(Fire fire)");
                minutespassed -= step;

            }
            lastUpdate = Fire_RV.unitime();

            if (fireTemp >= 0)
            {
                lastFireTemp = fireTemp; //we need this to catch up reservoir.
                fireLastOnAt = Fire_RV.unitime();
            }


        }
         public float embersPoprtion(Fire fire)
        {
			float cmins = Fire_RV.getCentigradeMinutes(fire);

            return Math.Min( embercmins / cmins,1f);
		}


		public bool updateEmber(Fire fire, float mins)
        {
            float cmins = Fire_RV.getCentigradeMinutes(fire);
            float temperature = fire.m_HeatSource.m_MaxTempIncrease;

			

			float accrate = 0.01f;
            float loserate = 1f;
            float emberFirepPoprtion = 1f;



            if (cmins != 0)
            {
                fire.m_UseEmbers = false;
				fire.m_EmberTimer = 0;

				////Debug.Log("cmins:" + cmins.ToString() + " embercmins:" + embercmins.ToString());
				if (cmins < embercmins) accrate *= cmins / Math.Max(1f, embercmins);


                float delta = (cmins - embercmins) * accrate * mins;
                embercmins += delta;


                float deltarel = 1 - delta / cmins;

                fire.m_HeatSource.m_MaxTempIncrease = temperature * deltarel;


            }
            else if (embercmins > 0)
            {
                fire.m_UseEmbers = true;
				////Debug.Log(" embercmins:" + embercmins.ToString());
                fire.m_EmberDurationSecondsTOD = float.PositiveInfinity;

                float embercminsold = embercmins;
                float delta = loserate * mins * temperature;
                embercmins -= delta;

                float tnew;
                if (temperature > Fire_RV.skillFireHeatingFactor())
                {

                    tnew = temperature * embercmins / embercminsold;
					fire.m_HeatSource.m_MaxTempIncrease = tnew;
					fire.m_FuelHeatIncrease = tnew;
				}
                else
                {

					fire.m_UseEmbers = false;
					fire.m_MaxOnTODSeconds = 0;
                    fire.m_EmberDurationSecondsTOD = 0;


                }

                
                TrackedBurntItemsNames.Clear();

                TrackedBurntItemsCentigradminutesFire.Clear();

                TrackedBurntGearItemHP.Clear();
            }
            else
            {
                fire.m_EmberDurationSecondsTOD = 5;

			}


            return true;
        }
    }
}





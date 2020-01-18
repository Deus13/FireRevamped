
using System;
using System.IO;
using System.Reflection;
using JsonModSettings;
using ModSettings;

namespace Fire_RV
{
    internal class Fire_RVSettings : JsonModSettingsBase<Fire_RVSettings>
    {



        [Section("Burntime:")]
        [Name("Smalfuel")]
        [Description("Multiplier for adjusting burntime of sticks, torches and books:")]
        [Slider(1f, 5f, 40)]
        public float SmalfuelBurntime = 3f;


        [Name("Wood")]
        [Description("Multiplier for adjusting burntime of reclaimd wood, cedar and fir:")]
        [Slider(1f, 5f, 40)]
        public float WoodBurntime = 3f;

        [Name("Coal")]
        [Description("Multiplier for adjusting burntime of coal:")]
        [Slider(1f, 5f, 40)]
        public float CoalBurntime = 3f;

        [Name("Tinder")]
        [Description("Multiplier for adjusting burntime of tinder:")]
        [Slider(1f, 5f, 40)]
        public float TinderBurntime = 1f;





        [Section("Fuelheat:")]
        [Name("Smalfuel")]
        [Description("Multiplier for adjusting heat of sticks, torches and books:")]
        [Slider(1f, 5f, 40)]
        public float SmalfuelHeat = 3f;


        [Name("Wood")]
        [Description("Multiplier for adjusting heat of reclaimd wood, cedar and fir:")]
        [Slider(1f, 5f, 40)]
        public float WoodHeat = 3f;

        [Name("Coal")]
        [Description("Multiplier for adjusting heat of coal:")]
        [Slider(1f, 5f, 40)]
        public float CoalHeat = 1f;

        [Name("Tinder")]
        [Description("Multiplier for adjusting heat of tinder:")]
        [Slider(1f, 5f, 40)]
        public float TinderHeat = 1f;


        [Section("Wind:")]
        [Name("Reworked Wind")]
        [Description("Changes how wind effcts fire. If enabled wind will increase fuel consumption of fires and only blow them out ons the are smale enought.")]
        public bool WindReworked = true;

        public static void OnLoad()
        {
            Instance = JsonModSettingsLoader.Load<Fire_RVSettings>();
        }
    }
}

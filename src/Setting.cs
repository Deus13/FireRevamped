
using System;
using System.IO;
using System.Reflection;
using ModSettings;

namespace Fire_RV
{
    internal class Fire_RVSettings : JsonModSettings
    {



        [Section("Burntime:")]
        [Name("Smalfuel")]
        [Description("Multiplier for adjusting burntime of sticks, torches and books:")]
        [Slider(1f, 5f, 41)]
        public float SmalfuelBurntime = 3f;


        [Name("Wood")]
        [Description("Multiplier for adjusting burntime of reclaimd wood, cedar and fir:")]
        [Slider(1f, 5f, 41)]
        public float WoodBurntime = 3f;

        [Name("Coal")]
        [Description("Multiplier for adjusting burntime of coal:")]
        [Slider(1f, 5f, 41)]
        public float CoalBurntime = 3f;

        [Name("Tinder")]
        [Description("Multiplier for adjusting burntime of tinder:")]
        [Slider(1f, 5f, 41)]
        public float TinderBurntime = 1f;





        [Section("Fuelheat:")]
        [Name("Smalfuel")]
        [Description("Multiplier for adjusting heat of sticks, torches and books:")]
        [Slider(1f, 5f, 41)]
        public float SmalfuelHeat = 3f;


        [Name("Wood")]
        [Description("Multiplier for adjusting heat of reclaimd wood, cedar and fir:")]
        [Slider(1f, 5f, 41)]
        public float WoodHeat = 3f;

        [Name("Coal")]
        [Description("Multiplier for adjusting heat of coal:")]
        [Slider(1f, 5f, 41)]
        public float CoalHeat = 2f;

        [Name("Tinder")]
        [Description("Multiplier for adjusting heat of tinder:")]
        [Slider(1f, 5f, 41)]
        public float TinderHeat = 1f;


        [Section("Wind:")]
        [Name("Reworked Wind")]
        [Description("Changes how wind effcts fire. If enabled wind will increase fuel consumption of fires and only blow them out ons the are smale enought.")]
        public bool WindReworked = true;

        [Section("Forge:")]
        [Name("Min Temperature")]
        [Description("Adjust the requiered temperture for using the Forge for crafting.")]
        [Slider(75f, 175f, 101)]
        public float MinTemperature = 100f;

        [Section("Emberbox:")]
        [Name("Embersbox duration")]
        [Description("Adjusts time in hours for hoe long the emberbox, from wulfmarius fire pack, can last.")]
        [Slider(0.5f, 36f, 72)]
        public float Embersboxduration = 18f;


        internal static class Settings
        {
            public static Fire_RVSettings options;
            public static void OnLoad()
            {
                options = new Fire_RVSettings();
                options.RefreshGUI();
                options.AddToModSettings("Fire RV Settings");
            }
        }
    }
}

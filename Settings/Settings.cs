

namespace ImprovedFires
{
    public class Settings : JsonModSettings
    {
        internal static Settings Instance { get; } = new();

		[Section("Burntime:")]
		[Name("Smalfuel")]
		[Description("Multiplier for adjusting burntime of sticks, torches and books:")]
		[Slider(1f, 5f, 41)]
		public float SmalfuelBurntime = 1.5f;


		[Name("Wood")]
		[Description("Multiplier for adjusting burntime of reclaimd wood, cedar and fir:")]
		[Slider(1f, 5f, 41)]
		public float WoodBurntime = 1.5f;

		[Name("Coal")]
		[Description("Multiplier for adjusting burntime of coal:")]
		[Slider(1f, 5f, 41)]
		public float CoalBurntime = 1.5f;

		[Name("Tinder")]
		[Description("Multiplier for adjusting burntime of tinder:")]
		[Slider(1f, 5f, 41)]
		public float TinderBurntime = 1.5f;





		[Section("Fuelheat:")]
		[Name("Smalfuel")]
		[Description("Multiplier for adjusting heat of sticks, torches and books:")]
		[Slider(1f, 5f, 41)]
		public float SmalfuelHeat = 1.5f;


		[Name("Wood")]
		[Description("Multiplier for adjusting heat of reclaimd wood, cedar and fir:")]
		[Slider(1f, 5f, 41)]
		public float WoodHeat = 1.5f;

		[Name("Coal")]
		[Description("Multiplier for adjusting heat of coal:")]
		[Slider(1f, 5f, 41)]
		public float CoalHeat = 1.5f;

		[Name("Tinder")]
		[Description("Multiplier for adjusting heat of tinder:")]
		[Slider(1f, 5f, 41)]
		public float TinderHeat = 1.5f;


		[Section("Wind:")]
		[Name("Reworked Wind")]
		[Description("Changes how wind effcts fire. If enabled wind will increase fuel consumption of fires and only blow them out ons the are smale enought.")]
		public bool WindReworked = true;

		[Section("Forge:")]
		[Name("Min Temperature")]
		[Description("Adjust the requiered temperture for using the Forge for crafting.")]
		[Slider(75f, 175f, 101)]
		public float MinTemperature = 100f;

		// this is called whenever there is a change. Ensure it contains the null bits that the base method has
		protected override void OnChange(FieldInfo field, object? oldValue, object? newValue)
        {
            base.OnChange(field, oldValue, newValue);
        }

        // This is used to load the settings
        internal static void OnLoad()
        {
            Instance.AddToModSettings(BuildInfo.GUIName);
            Instance.RefreshGUI();
        }
    }
}
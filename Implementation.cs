namespace ImprovedFires
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Improved Fires is online!");
            Settings.OnLoad();
        }
    }
}

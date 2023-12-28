using ModData;
using MelonLoader;
using MelonLoader.TinyJSON;

namespace ImprovedFires
{
	internal class SaveManager
	{
		ModDataManager dm = new ModDataManager("ImprovedFires", false);

		public void SaveHeatReservoirs()
		{

			if (Fire_RV.myreservoirs == null) Fire_RV.myreservoirs = new List<HeatReservoir>();
			string dataString = JSON.Dump(Fire_RV.myreservoirs);
			dm.Save(dataString);
		}

		public List<HeatReservoir> LoadHeatReservoirs()
		{
			
			string? dataString = dm.Load();
			if (dataString is null)
			{
				return new List<HeatReservoir>();
			}

			List<HeatReservoir>? data = JSON.Load(dataString).Make<List<HeatReservoir>>();
			return data is not null ? data : new List<HeatReservoir>();
		}
	}
}

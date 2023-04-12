using System.Drawing;
using WebAPI.Model;

namespace WebAPI.ImageProcessing;

public static class ProcessingMethods
{
	public static GasStationPrices BenzinaProcess(Image image)
	{
		return new GasStationPrices { StationId = 0, Natural95 = 37.5, Diesel = 36.9 };
	}

	public static GasStationPrices OjlProcess(Image image)
	{
		return new GasStationPrices { StationId = 0, Natural95 = 37.5, Diesel = 36.9 };
	}
}

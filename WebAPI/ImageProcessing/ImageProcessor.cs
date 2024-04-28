using System.Drawing;
using WebAPI.Model;

namespace WebAPI.ImageProcessing;

public class ImageProcessor
{
	public GasStationPrices? GetPrices(Bitmap bitmap, string stationBrand)
	{
		return stationBrand switch
		{
			"Benzina" => ProcessingMethods.BenzinaProcess(bitmap),
			"Ojl" => ProcessingMethods.OjlProcess(bitmap),
			"Shell" => null,
			_ => null,
		};
	}
}
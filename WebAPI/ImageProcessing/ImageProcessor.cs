using System.Drawing;
using WebAPI.Model;

namespace WebAPI.ImageProcessing;

public class ImageProcessor
{
	public GasStationPrices? GetPrices(Image image, string stationBrand)
	{
		return stationBrand switch
		{
			"Benzina" => ProcessingMethods.BenzinaProcess(image),
			"Ojl a. s." => ProcessingMethods.OjlProcess(image),
			"Shell" => null,
			_ => null,
		};
	}
}
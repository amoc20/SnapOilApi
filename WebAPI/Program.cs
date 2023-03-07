using System.Drawing;
using ImageProcessing;
using WebAPI;

var formFiller = new FormFiller();
var imageProcessor = new ImageProcessor();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/uploadimg", async (IFormFile file) =>
{
	(double natural95, double diesel)? prices = null;

	using (var memoryStream = new MemoryStream())
	{
		await file.CopyToAsync(memoryStream);
		using var img = Image.FromStream(memoryStream);
		string stationName = file.FileName.Split(".").First();
		prices = imageProcessor.GetPrices(img, stationName);
	}

	return prices is null ? Results.NotFound() : Results.Ok(prices);
});

app.MapPost("/uploadprices", (GasStationPrices prices) =>
{
	formFiller.SendInfo(prices);
	return Results.Ok(prices);
});

app.Run();
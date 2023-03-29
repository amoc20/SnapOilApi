using System.Drawing;
using ImageProcessing;
using MySqlConnector;
using WebAPI;

var formFiller = new FormFiller();
var imageProcessor = new ImageProcessor();

// Database connection
const string server = "localhost";
const string username = "snap_oil";
const string password = "123";
const string database = "snap_oil";
const string connString = $"Server={server};User ID={username};Password={password};Database={database}";

using var connection = new MySqlConnection(connString);
Console.WriteLine("Connecting to db...");
await connection.OpenAsync();
Console.WriteLine("Connected to db");

// Build
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Image processing
app.MapPost("/uploadimg", async (IFormFile file) =>
{
	(double natural95, double diesel)? prices = null;

	using (var memoryStream = new MemoryStream())
	{
		await file.CopyToAsync(memoryStream);
		using var img = Image.FromStream(memoryStream);
		string brandName = file.FileName.Split(".").First();
		prices = imageProcessor.GetPrices(img, brandName);
	}

	return prices is null ? Results.NotFound() : Results.Ok(prices);
});

// Prices upload
app.MapPost("/uploadprices", (GasStationPrices prices) =>
{
	formFiller.SendInfo(prices);
	return Results.Ok(prices);
});

// Stations get
app.MapGet("/stations", async () =>
{
	using var command = new MySqlCommand("" +
		$"SELECT id_station, city, address, brand_name, id_brand FROM stations NATURAL JOIN brands;",
		connection);
	using var reader = await command.ExecuteReaderAsync();

	var output = new List<GasStation>();
	while (reader.Read())
	{
		var stationId = reader.GetInt32(0)!;
		var city = reader.GetString(1)!;
		var address = reader.GetString(2)!;
		var brandName = reader.GetString(3)!;
		var brandId = reader.GetInt32(4)!;

		output.Add(new GasStation
		{
			StationId = stationId,
			City = city,
			Address = address,
			Brand = brandName,
			BrandId = brandId
		});
	}

	return output;
});

app.MapGet("/stations/{brand:int}", async (string brand) =>
{
	using var command = new MySqlCommand("" +
		$"SELECT id_station, city, address, brand_name, id_brand FROM stations NATURAL JOIN brands WHERE id_brand = {brand};",
		connection);
	using var reader = await command.ExecuteReaderAsync();

	var output = new List<GasStation>();
	while (reader.Read())
	{
		var stationId = reader.GetInt32(0)!;
		var city = reader.GetString(1)!;
		var address = reader.GetString(2)!;
		var brandName = reader.GetString(3)!;
		var brandId = reader.GetInt32(4)!;

		output.Add(new GasStation
		{
			StationId = stationId,
			City = city,
			Address = address,
			Brand = brandName,
			BrandId = brandId
		});
	}

	return output;
});

app.Run();
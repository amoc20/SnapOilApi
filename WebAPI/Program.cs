using MySqlConnector;
using System.Drawing;
using WebAPI.ImageProcessing;
using WebAPI.Model;
using WebAPI.WebAutomation;

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
app.MapPost("/uploadimage", async (IFormFile file) =>
{
	GasStationPrices? prices = null;

	using (var memoryStream = new MemoryStream())
	{
		await file.CopyToAsync(memoryStream);
		var bitmap = new Bitmap(memoryStream);
		string brandName = file.FileName.Split(".").First();
		prices = imageProcessor.GetPrices(bitmap, brandName);
	}

	return prices is null ? Results.NotFound() : Results.Ok(prices);
});

// Prices POST
app.MapPost("/uploadprices", (GasStationPrices prices) =>
{
	formFiller.SendInfo(prices);
	return Results.Ok(prices);
});

// Brands GET
app.MapGet("/brands", async () =>
{
	using var command = new MySqlCommand(
		$"SELECT id_brand, brand_name FROM brands ORDER BY brand_name;",
		connection);
	using var reader = await command.ExecuteReaderAsync();

	var output = new List<Brand>();
	while (reader.Read())
	{
		var id = reader.GetInt32(0)!;
		var name = reader.GetString(1)!;

		output.Add(new Brand
		{
			Id = id,
			Name = name
		});
	}

	return output;
});

// Stations GET
app.MapGet("/stations/{brand:int}", async (int brand) =>
{
	using var command = new MySqlCommand(
		$"SELECT id_station, city, address, id_brand, brand_name " +
		$"FROM stations NATURAL JOIN brands WHERE id_brand = {brand} ORDER BY city;",
		connection);
	using var reader = await command.ExecuteReaderAsync();

	var output = new List<GasStation>();
	while (reader.Read())
	{
		var stationId = reader.GetInt32(0)!;
		var city = reader.GetString(1)!;
		var address = reader.GetString(2)!;
		var brandId = reader.GetInt32(3)!;
		var brandName = reader.GetString(4)!;

		output.Add(new GasStation
		{
			Id = stationId,
			City = city,
			Address = address,
			Brand = new Brand
			{
				Id = brandId,
				Name = brandName
			}
		});
	}

	return output;
});

app.Run();
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using Tesseract;
using WebAPI.Model;

namespace WebAPI.ImageProcessing;

public static class ProcessingMethods
{
	public static GasStationPrices BenzinaProcess(Bitmap bitmap)
	{
		Mat originalImage = bitmap.ToMat();

		// Gaussian blur
		Mat blurredImg = new();
		CvInvoke.GaussianBlur(originalImage, blurredImg, new Size(35, 35), 0);

		// LAB convert and get A channel
		Mat labImage = new();
		CvInvoke.CvtColor(blurredImg, labImage, ColorConversion.Rgb2Lab);
		VectorOfMat channels = new();
		CvInvoke.Split(labImage, channels);
		Mat AChannel = channels[1];

		// Thresholding
		Mat threshImg = new();
		CvInvoke.Threshold(AChannel, threshImg, 175, 255, ThresholdType.Binary);

		// Dilate and erode
		Mat kernel = new(5, 5, DepthType.Cv8U, threshImg.NumberOfChannels);
		Mat dilatedImg = new();
		Mat erodedImg = new();

		CvInvoke.Dilate(threshImg, dilatedImg, kernel, new Point(0, 0), 3, BorderType.Reflect, new MCvScalar(255));
		CvInvoke.Erode(dilatedImg, erodedImg, kernel, new Point(0, 0), 2, BorderType.Reflect, new MCvScalar(255));

		// Contour detection
		var contours = new VectorOfVectorOfPoint();
		var hierarchy = new Mat();
		CvInvoke.FindContours(erodedImg, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

		// Find bounding rectangle of contours
		int minX = erodedImg.Width;
		int maxX = 0;
		int minY = erodedImg.Height;
		int maxY = 0;

		foreach (Point[] contour in contours.ToArrayOfArray())
			foreach (Point point in contour)
			{
				minX = Math.Min(minX, point.X);
				maxX = Math.Max(maxX, point.X);
				minY = Math.Min(minY, point.Y);
				maxY = Math.Max(maxY, point.Y);
			}

		int width = maxX - minX;
		int height = maxY - minY;
		Rectangle boundingRect = new(minX, minY, width, height);

		Mat croppedImage = new(erodedImg, boundingRect);
		Mat invertedCropImg = new();
		CvInvoke.BitwiseNot(croppedImage, invertedCropImg);

		// Split to individual numbers
		Mat firstNumber = new(invertedCropImg, new Rectangle(0, 0, invertedCropImg.Width, invertedCropImg.Height / 4));
		Mat secondNumber = new(invertedCropImg, new Rectangle(0, invertedCropImg.Height / 4, invertedCropImg.Width, invertedCropImg.Height / 4));

		CvInvoke.Imwrite("./first.jpg", firstNumber);
		CvInvoke.Imwrite("./second.jpg", secondNumber);

		// OCR
		var engine = new TesseractEngine("./tessdata", "eng");
		var tFirstNumber = Pix.LoadFromFile("./first.jpg");
		var tSecondNumber = Pix.LoadFromFile("./second.jpg");

		var page1 = engine.Process(tFirstNumber, PageSegMode.RawLine);
		string result1 = page1.GetText();
		page1.Dispose();
		var page2 = engine.Process(tSecondNumber, PageSegMode.RawLine);
		string result2 = page2.GetText();
		engine.Dispose();

		return new GasStationPrices { StationId = 0, Natural95 = ConvertToDouble(result1), Diesel = ConvertToDouble(result2) };
	}

	public static GasStationPrices OjlProcess(Bitmap bitmap)
	{
		return new GasStationPrices { StationId = 0, Natural95 = 37.5, Diesel = 36.9 };
	}

	private static double ConvertToDouble(string text)
	{
		string digits = "";

		foreach (char ch in text.ToList())
		{
			if (char.IsDigit(ch))
			{
				digits += ch;
			}
		}
		int number = int.Parse(digits);

		return (double)number / 100;
	}
}

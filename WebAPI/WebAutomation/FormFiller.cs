using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebAPI.Model;

namespace WebAPI.WebAutomation;

public class FormFiller
{
    private readonly string URL = "https://m.ceskybenzin.cz/aktualizace.php?id=";
    private readonly string natural95Id = "p1";
    private readonly string dieselId = "p3";

    public void SendInfo(GasStationPrices prices)
    {
        IWebDriver driver = new ChromeDriver();
        driver.Navigate().GoToUrl(URL + prices.StationId);

        var natural95Element = driver.FindElement(By.Id(natural95Id));
        natural95Element.SendKeys(prices.Natural95.ToString());

        var dieselElement = driver.FindElement(By.Id(dieselId));
        dieselElement.SendKeys(prices.Diesel.ToString());

        //dieselElement.Submit();
        driver.Quit();
    }
}

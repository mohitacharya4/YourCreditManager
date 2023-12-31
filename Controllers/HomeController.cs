using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WazeCredit.Models;
using WazeCredit.Models.ViewModels;
using WazeCredit.Service;
using WazeCredit.Utility.AppSettingsClasses;

namespace WazeCredit.Controllers
{
    public class HomeController : Controller
    {   
        public HomeVM homeVM { get; set; }

        private readonly IMarketForecaster _marketForecaster;
        private readonly StripeSettings _stripeSettings;
        private readonly TwilioSettings _twilioSettings;
        private readonly SendGridSettings _sendGridSettings;
        private readonly WazeForecastSettings _wazeForecastSettings;

        public HomeController(IMarketForecaster marketForecaster,
            IOptions<StripeSettings> stripeOptions,
            IOptions<TwilioSettings> twilioOptions,
            IOptions<SendGridSettings> sendGridOptions,
            IOptions<WazeForecastSettings> wazeForecastOptions
            )
        {
            homeVM = new HomeVM();
            _marketForecaster = marketForecaster;
            _stripeSettings = stripeOptions.Value;
            _twilioSettings = twilioOptions.Value;
            _sendGridSettings = sendGridOptions.Value;
            _wazeForecastSettings = wazeForecastOptions.Value;
        }

        public IActionResult Index()
        {
            MarketResult currentMarket = _marketForecaster.GetMarketPrediction();

            switch (currentMarket.MarketCondition) {
                case MarketCondition.StableDown:
                    homeVM.MarketForecast = "Market shows sign to go down in a stable state.";
                    break;
                case MarketCondition.StableUp:
                    homeVM.MarketForecast = "Market shows sign to go up in a stable state.";
                    break;
                case MarketCondition.Volatile:
                    homeVM.MarketForecast = "Market shows sign of volatility.";
                    break;
                default:
                    homeVM.MarketForecast = "Subscribe to our newsletter.";
                    break;
            }

            return View(homeVM);
        }

        public IActionResult AllConfigSettings()
        {
            List<string> messages = new List<string>();
            messages.Add($"Waze config - Forecast Tracker: {_wazeForecastSettings.ForecastTrackerEnabled}");
            messages.Add($"Stripe Publishable Key: {_stripeSettings.PublishableKey}");
            messages.Add($"Send Grid Key: {_sendGridSettings.SendGridKey}");

            return View(messages);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

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
        private readonly ICreditValidator _creditValidator;
        private readonly StripeSettings _stripeSettings;
        private readonly TwilioSettings _twilioSettings;
        private readonly SendGridSettings _sendGridSettings;
        private readonly WazeForecastSettings _wazeForecastSettings;
        [BindProperty]
        public CreditApplication CreditModel { get; set; }

        public HomeController(IMarketForecaster marketForecaster,
            IOptions<StripeSettings> stripeOptions,
            IOptions<TwilioSettings> twilioOptions,
            IOptions<SendGridSettings> sendGridOptions,
            IOptions<WazeForecastSettings> wazeForecastOptions,
            ICreditValidator creditValidator
            )
        {
            homeVM = new HomeVM();
            _marketForecaster = marketForecaster;
            _stripeSettings = stripeOptions.Value;
            _twilioSettings = twilioOptions.Value;
            _sendGridSettings = sendGridOptions.Value;
            _wazeForecastSettings = wazeForecastOptions.Value;
            _creditValidator = creditValidator;
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

        public IActionResult CreditApplication()
        {
            CreditModel = new CreditApplication();

            return View(CreditModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [ActionName("CreditApplication")]
        public async Task<IActionResult> CreditApplicationPOST()
        {
            if (ModelState.IsValid)
            {
                var (validationPassed, errorMessages) = await _creditValidator.PassAllValidations(CreditModel);
            }

            return View(CreditModel);
        }


        public IActionResult CreditResult(CreditResult creditResult)
        {
            return View(creditResult);
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

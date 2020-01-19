using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CheapFlights.Models;
using CheapFlights.Repository;

namespace CheapFlights.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CheapFlightsProcessor cFProc = new CheapFlightsProcessor();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(FlightQuery flightQuery)
        {
            if(flightQuery.DepartureDate > flightQuery.ReturnDate)
            {
                ModelState.AddModelError("ReturnDateHigher", "Please select return date that is later then departure date.");
            }
            if (ModelState.IsValid)
            {
                string formatedDate = flightQuery.DepartureDate.ToString("yyyy-MM-dd");

                string formatedReturnDate = flightQuery.ReturnDate?.ToString("yyyy-MM-dd") ?? "";

                string origin = cFProc.IataFromAirportName(flightQuery.DepartureAirport);

                string dest = cFProc.IataFromAirportName(flightQuery.ArrivalAirport);

                if (flightQuery.Passengers <= 0)
                {
                    flightQuery.Passengers = 1;
                }

                List<FlightInfoViewModel> flightInfoViewModels =
                    await cFProc.GetCheapFlightsAsync(origin, dest, formatedDate, flightQuery.Currency.ToString(),
                    flightQuery.Passengers, returnDate: formatedReturnDate);


                return View("Results", flightInfoViewModels);
            }
            else
            {
                return View();
            }
            
        }

        [HttpGet]
        public string IataJsonResult()
        {
            return System.IO.File.ReadAllText(Path.GetFullPath("~/Repository/IATA.json").Replace("~\\", ""));
        }

        [HttpGet]
        public IActionResult About()
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

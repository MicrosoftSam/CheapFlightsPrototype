using QuickType;
using System.Collections.Generic;

namespace CheapFlights.Models
{
    public class FlightInfoViewModel
    {
        public string Id { get; set; }
        public Price Price { get; set; }
        public string Currency { get; set; }
        public List<Service> Services { get; set; }
        public string DepartureAirportName { get; set; }
        public string ArrivalAirportName { get; set; }
    }

}

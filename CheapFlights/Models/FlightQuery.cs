using CheapFlights.CustomDataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace CheapFlights.Models
{
    public enum Currency
    {
        USD,
        EUR,
        HRK
    }
    public class FlightQuery
    {
        [Required(ErrorMessage = "Please select departure airport.")]
        public string DepartureAirport { get; set; }
        [Required(ErrorMessage = "Please select arrival airport.")]
        public string ArrivalAirport { get; set; }

        [FutureDate(ErrorMessage = "Please select some future date.")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [FutureDate(ErrorMessage = "Please select some future date.")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }
        [Required(ErrorMessage = "Please select currency.")]
        public Currency? Currency { get; set; }

        [Required(ErrorMessage = "Please enter a number of adult passengers.")]
        [MinimalInteger(ErrorMessage = "Please insert number of adult passengers to greather than or equal to 1.")]
        public int Passengers { get; set; }
    }
}

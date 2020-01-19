using CheapFlights.Infrastructure;
using CheapFlights.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuickType;
using Newtonsoft.Json;
using System.IO;

namespace CheapFlights.Repository
{
    //Klasa čija je funkcija targetanje Amadeus API-a. Te svi poslovi dohvaćanja i procesiranja podataka.
    /// <summary>
    /// Klasa za procesiranje Amadeus API-a.
    /// </summary>
    public class CheapFlightsProcessor
    {
        private FlightInfoViewModel flightInfoVM = new FlightInfoViewModel();
        private List<FlightInfoViewModel> flightInfoViewModels = new List<FlightInfoViewModel>();
        /// <summary>
        /// Asinkrona funkcija za dohvaćanje podataka sa Amadeus API-a.
        /// Vraća Listu FlightInfoViewModela koji uzima samo potrebne stavke
        /// </summary>
        /// <param name="origin">Origin IATA</param>
        /// <param name="destination">Destination IATA</param>
        /// <param name="departureDate">Departure date ("yyyy-MM-dd")</param>
        /// <param name="max">Maksimalni broj podataka za query</param>
        /// <returns></returns>
        public async Task<List<FlightInfoViewModel>> GetCheapFlightsAsync(string origin, string destination, string departureDate,
            string currency, int passengers = 1, int max = 25, string returnDate = "")
        {
            string url = "";
            //trenutno targeta samo direktne letove(alpha verzija, tek onda bi trebalo složiti sa stajanjima i vidjeti kako ti objekti rade - trenutno je greška u načinu sortiranja objekata)
            if (String.IsNullOrEmpty(returnDate))
            {
                url = $"https://test.api.amadeus.com/v1/shopping/flight-offers?origin={origin}&destination={destination}&departureDate={departureDate}&currency={currency}&adults={passengers}&max={max}";
            }
            else
            {
                url = $"https://test.api.amadeus.com/v1/shopping/flight-offers?origin={origin}&destination={destination}&departureDate={departureDate}&returnDate={returnDate}&currency={currency}&adults={passengers}&max={max}";
            }
            

            using(HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    string flightInfo = await response.Content.ReadAsStringAsync();

                    var fi = FlightInfo.FromJson(flightInfo);
                    
                    //uzimanje podataka iz JSON-a i prosljeđivanje potrebnih podataka u ViewModel
                    foreach(var data in fi.Data)
                    {
                        flightInfoVM.Id = data.Id;
                        foreach(var offer in data.OfferItems)
                        {
                            flightInfoVM.Price = offer.Price;
                            flightInfoVM.Currency = currency;
                            flightInfoVM.Services = offer.Services;
                            foreach (var service in flightInfoVM.Services)
                            {
                                foreach(var segment in service.Segments)
                                {
                                    segment.FlightSegment.Departure.TargetLocation = AirportNameFromIata(segment.FlightSegment.Departure.IataCode);
                                    segment.FlightSegment.Arrival.TargetLocation = AirportNameFromIata(segment.FlightSegment.Arrival.IataCode);

                                }
                            }
                        }

                        flightInfoViewModels.Add(flightInfoVM);
                        flightInfoVM = new FlightInfoViewModel();
                    }
                }
                else if((int)response.StatusCode >= 400)
                {
                    //korištenje ovog exceptiona samo za dev potrebe TODO - neki decent user friendly exception message
                    throw new Exception($"Status code: {response.StatusCode} Check amadeus for developers for response detail.");
                }
                else if((int)response.StatusCode >= 500)
                {
                    //korištenje ovog exceptiona samo za dev potrebe TODO - neki decent user friendly exception message
                    throw new Exception($"Status code: {response.StatusCode} Check amadeus for developers for response detail.");
                }
            }
            flightInfoViewModels.OrderBy(p => p.Price.Total);
            return flightInfoViewModels;
        }

        public string AirportNameFromIata(string iataCode)
        {
            //Potencijalno učitati sve u dictionary
            //Dictionary<string, string> iataAirportName =
            //    JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(jsonPath));

            JObject iata = JObject.Parse(File.ReadAllText(IATAProcessor.GetIataCodesJson()));

            return (string)iata[iataCode];
        }

        public string IataFromAirportName(string airportName)
        {
            string test = IATAProcessor.GetIataCodesJson();
            Dictionary<string, string> iataAirportName =
                JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(IATAProcessor.GetIataCodesJson()));

            //splitanje stringa dobivenog iz inputa zbog IATA codea u zagradama na listi ponuđenih polazišnih/odredišnih aerodroma
            return iataAirportName.FirstOrDefault(x => x.Value.Contains(airportName.Split('(')?[0].Trim())).Key;

        }
    }
}

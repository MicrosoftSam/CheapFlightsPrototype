using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CheapFlights.Repository
{
    public static class IATAProcessor
    {

        //Funkcija za crawlanje wikipedijom koja ima popis IATA code-ova.
        static async Task<Dictionary<string,string>> GetIataAsync()
        {
            HttpClient webClient = new HttpClient();

            //Dictionary koji će funkcija vratiti nakon obrade.
            Dictionary<string, string> iataAirportPairs = new Dictionary<string, string>();
            List<char> alphabet = Alphabet();

            foreach (char letter in alphabet)
            {
                var url = $"https://en.wikipedia.org/wiki/List_of_airports_by_IATA_code:_{letter}";

                var page = await webClient.GetStringAsync(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                //Dohvaćanje wiki tablice
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table")
                      .Descendants("tr")
                      //Izbjegavanje sub sekcija koje se ne odnose na IATA codeove. 
                      //Count se koristi da se izbjegnju slučajne pogreške u tablici
                      //> 1 radi grešku na -NE- subsekciji jer greškom ima 2 td
                      .Where(tr => tr.Elements("td").Count() > 2)
                      //Selekcija td elemenata u tr elementu. Replace se koristi da se izbjegnu linkovi do naziva
                      .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim().Replace("&#91;1&#93", "")
                      .Replace("&#91;2&#93", "")).ToList())
                      .ToList();

                foreach (var row in table)
                {
                    iataAirportPairs.Add(row[0], row[3]);         
                }
            }
            return iataAirportPairs;
        }

        //Serijalizacija IATA code-ova i kreiranje JSON file-a.
        public static async Task serializeDictionary()
        {
            var serializer = new JsonSerializer();

            Dictionary<string, string> iataCodes = await GetIataAsync();
            using (StreamWriter sw = new StreamWriter(GetIataCodesJson()))
            {
                using(JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, iataCodes);
                }
            }
        }

        public static JObject getIataJobjectFromFile()
        {
            return JObject.Parse(File.ReadAllText(Path.GetFullPath("~/Repository/IATA.json").Replace("~\\", "")));
        }

        //Metoda za izradu abecede za kasnije korištenje u izradi linkova za wiki crawlanje
        /// <summary>
        /// Kreiranje cijele abecede A-Z
        /// </summary>
        /// <returns></returns>
        static List<char> Alphabet()
        {
            List<char> alphabet = new List<char>();

            for (int i = 65; i < 91; i++)
            {
                alphabet.Add((char)i);
            }

            return alphabet;
        }

        public static string GetIataCodesJson()
        {
            //Nije dobra praksa ali prolazi za dohvačanje iz static klase s kojom user nema doticaja 
            return Path.GetFullPath("~/Repository/IATA.json").Replace("~\\","");
        }
    }
}

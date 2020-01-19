using System.Net.Http;
using System.Net.Http.Headers;

namespace CheapFlights.Infrastructure
{
    //Static klasa koja servira jednu instancu HttpClienta te potencijalno ima pomočne metode za rad s API-jem.
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; set; }

        /// <summary>
        /// Meteoda za inicijalizaciju HttpClienta
        /// </summary>
        public static void InitializeClient(string accessToken)
        {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CheapFlights.Infrastructure
{
    //Moguć problem s inicijalizacijom HttpClienta iz ove klase? Za sad radi as intended
    //Background servis koji requesta novi token dok se inicijalizira te svakih 25 minuta, 
    //s obzirom da Amadeus access_token ima lifetime od 30 minuta. 
    internal sealed class AmadeusTokenService : BackgroundService, IAmadeusTokenService
    {
        private const int InitialDelay = 10 * 1000; //deset sekundi za initial Delay prije poziva na API
        private const int Delay = (25 * 60 * 1000); //request za novi token svakih 25 minuta 

        private readonly ILogger<AmadeusTokenService> _logger;
        private readonly IServiceProvider _serviceProvider;

        //
        public AmadeusTokenService(ILogger<AmadeusTokenService> logger, IServiceProvider serviceProvider)
        {
            if(logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if(serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("TokenService started");
                stoppingToken.Register(() => _logger.LogDebug("TokenService stopped because of stopping token."));

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("TokenService waiting to be scheduled.");
                    await Task.Delay(InitialDelay, stoppingToken);
                }

                _logger.LogDebug("TokenService is working");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await GetBeareerKeyFromJson();

                    await Task.Delay(Delay);
                }
            }
            catch(Exception ex)
            {
                _logger.LogDebug($"TokenService encountered error: {ex.Message} and stopped working.");
            }
        }

        //Napraviti background worker service da se automatski query-a
        /// <summary>
        /// Metoda za poziv na Amadeus API. Vraća Json response
        /// </summary>
        private async Task<string> GetNewTokenJson()
        {
            var client = new RestClient("https://test.api.amadeus.com/v1/security/oauth2/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("client_id", "QI9arr9ZxGVG2WNbFFWdnvdvRXz5O9o7");
            request.AddParameter("client_secret", "XU3afL346BwCKoBR");
            request.AddParameter("grant_type", "client_credentials");
            IRestResponse response = await client.ExecuteAsync(request);
            return response.Content;
        }

        //Metoda koja se poziva da dobivanje oauth2 bearer keya. 
        //Loša praksa inicijalizacije HttpClienta iz background servisa? Za sad radi as intended.
        private async Task GetBeareerKeyFromJson()
        {
            string token = await GetNewTokenJson();
            JObject jObject = JObject.Parse(token);

            ApiHelper.InitializeClient((string)jObject["access_token"]);
        }
    }
}

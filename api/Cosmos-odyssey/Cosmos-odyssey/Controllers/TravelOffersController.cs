using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Cosmos_odyssey.Moduls; 

namespace Cosmos_odyssey.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TravelOffersController : ControllerBase
    {
        private readonly string connectionString;
        public TravelOffersController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET api/traveloffers/mars/venus  
        [HttpGet("{fromPlanet}/{toPlanet}")]
        public IActionResult GetTravelOffers(string fromPlanet, string toPlanet) //get all true offers
        {
            List<TravelOffers> offers = GetTravelOffersByPlanets(fromPlanet, toPlanet);

            if (offers.Count == 0)
            {
                return NotFound(new { message = "We don'w have offers :(" });
            }

            return Ok(offers);
        }

        private List<TravelOffers> GetTravelOffersByPlanets(string fromPlanet, string toPlanet)
        {
            List<TravelOffers> travelOffers = new List<TravelOffers>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = "SELECT Id, price_list_Id, from_planet, to_planet, distance, price, flight_start, flight_end, company_name " +
                           "FROM travel_offers " +
                           "WHERE from_planet = @fromPlanet AND to_planet = @toPlanet";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@fromPlanet", fromPlanet);
            cmd.Parameters.AddWithValue("@toPlanet", toPlanet);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                travelOffers.Add(new TravelOffers
                {
                    TravelId = reader.GetGuid("Id").ToString(),
                    PricelistId = reader.GetGuid("price_list_Id").ToString(),
                    FromPlanet = reader.GetString("from_planet"),
                    ToPlanet = reader.GetString("to_planet"),
                    Distance = reader.GetInt32("distance"),
                    Price = reader.GetInt32("price"),
                    TravelStart = reader.GetDateTime("flight_start"),
                    TravelEnd = reader.GetDateTime("flight_end"),
                    CompanyName = reader.GetString("company_name")
                });
            }

            return travelOffers;
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Cosmos_odyssey.Moduls;

namespace Cosmos_odyssey.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdatePriceList : ControllerBase
    {
        private readonly string connectionString;
        private readonly string apiUrl;

        public UpdatePriceList(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            apiUrl = configuration["ApiSettings:TravelPricesUrl"];
        }
        // GET api/UpdatePriceList
        [HttpGet]
        public async Task<IActionResult> GetPriceList() //Check how many pricelists already in db & is it already exists
        {
            var data = await FetchApiData();
            if (data == null)
            {
                return BadRequest("API Error");
            }
            List<string> existingPriceListIds = GetExistingPriceListIds();
            string priceListId = data.RootElement.GetProperty("id").GetString();

            if (existingPriceListIds.Contains(priceListId))
            {
                return Ok(new { message = "Price list already exists" });
            }
            int priceListCount = GetPriceListCount();
            if (priceListCount >= 15)
            {
                DeleteOldestPriceList();
            }

            SavePriceListAndOffers(data, priceListId);
            List<Pricelists> priceLists = GetPriceLists();
            return Ok(priceLists);
        }

        private async Task<JsonDocument> FetchApiData()
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(jsonString);
            }
            return null;
        }

        private List<string> GetExistingPriceListIds()
        {
            List<string> priceListIds = new List<string>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT id FROM price_lists", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                priceListIds.Add(reader.GetGuid(0).ToString());
            }
            return priceListIds;
        }

        private int GetPriceListCount()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM price_lists", connection);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void DeleteOldestPriceList()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "SELECT id FROM price_lists ORDER BY valid_until ASC LIMIT 1", connection);

            var oldestPriceListId = cmd.ExecuteScalar()?.ToString();

            if (oldestPriceListId != null)
            {
                using var deleteCmd = new MySqlCommand("DELETE FROM price_lists WHERE id = @id", connection);
                deleteCmd.Parameters.AddWithValue("@id", oldestPriceListId);
                deleteCmd.ExecuteNonQuery();
            }
        }

        private void SavePriceListAndOffers(JsonDocument data, string priceListId)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (!DateTime.TryParse(data.RootElement.GetProperty("validUntil").GetString(), out DateTime validUntil))
                {
                    throw new FormatException("Invalid date format for validUntil");
                }

                string validUntilFormatted = validUntil.ToString("yyyy-MM-dd HH:mm:ss");

                using (var cmd = new MySqlCommand("INSERT INTO price_lists (id, valid_until) VALUES (@id, @validUntil)", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", priceListId);
                    cmd.Parameters.AddWithValue("@validUntil", validUntilFormatted);
                    cmd.ExecuteNonQuery();
                }

                foreach (var leg in data.RootElement.GetProperty("legs").EnumerateArray())
                {
                    var route = leg.GetProperty("routeInfo");
                    string fromPlanet = route.GetProperty("from").GetProperty("name").GetString();
                    string toPlanet = route.GetProperty("to").GetProperty("name").GetString();
                    long distance = route.GetProperty("distance").GetInt64(); 

                    foreach (var provider in leg.GetProperty("providers").EnumerateArray())
                    {
                        string offerId = provider.GetProperty("id").GetString();
                        string companyName = provider.GetProperty("company").GetProperty("name").GetString();

                        if (!decimal.TryParse(provider.GetProperty("price").GetRawText(), out decimal price))
                        {
                            throw new FormatException($"Invalid price format for offer {offerId}");
                        }

                        if (!DateTime.TryParse(provider.GetProperty("flightStart").GetString(), out DateTime flightStart))
                        {
                            throw new FormatException($"Invalid date format for flightStart in offer {offerId}");
                        }

                        if (!DateTime.TryParse(provider.GetProperty("flightEnd").GetString(), out DateTime flightEnd))
                        {
                            throw new FormatException($"Invalid date format for flightEnd in offer {offerId}");
                        }

                        string flightStartFormatted = flightStart.ToString("yyyy-MM-dd HH:mm:ss");
                        string flightEndFormatted = flightEnd.ToString("yyyy-MM-dd HH:mm:ss");

                        using (var cmd = new MySqlCommand(
                            "INSERT INTO travel_offers (id, price_list_id, from_planet, to_planet, distance, company_name, price, flight_start, flight_end) " +
                            "VALUES (@id, @priceListId, @fromPlanet, @toPlanet, @distance, @companyName, @price, @flightStart, @flightEnd)", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", offerId);
                            cmd.Parameters.AddWithValue("@priceListId", priceListId);
                            cmd.Parameters.AddWithValue("@fromPlanet", fromPlanet);
                            cmd.Parameters.AddWithValue("@toPlanet", toPlanet);
                            cmd.Parameters.AddWithValue("@distance", distance);
                            cmd.Parameters.AddWithValue("@companyName", companyName);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@flightStart", flightStartFormatted);
                            cmd.Parameters.AddWithValue("@flightEnd", flightEndFormatted);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private List<Pricelists> GetPriceLists()
        {
            List<Pricelists> priceLists = new List<Pricelists>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT id, valid_until FROM price_lists", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                priceLists.Add(new Pricelists
                {
                    PriceListId = reader.GetGuid(0).ToString(),
                    PricelistValid = reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            return priceLists;
        }
    }
}

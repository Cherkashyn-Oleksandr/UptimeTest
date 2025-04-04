using Cosmos_odyssey.Moduls;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Cosmos_odyssey.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateReservationController : ControllerBase
    {
        private readonly string _connectionString;

        public CreateReservationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost] // GET api/CreateReservation
        public async Task<IActionResult> CreateReservation([FromBody] Reservations reservation)
        {
            if (reservation == null)
                return BadRequest("Invalid data.");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM reservations 
                        WHERE travel_offer_id = @TravelOfferID 
                          AND price_list_id = @PriceListId 
                          AND first_name = @FirstName
                          AND last_name = @LastName";

                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@TravelOfferID", reservation.TravelOfferID);
                        checkCmd.Parameters.AddWithValue("@PriceListId", reservation.PricelistId);
                        checkCmd.Parameters.AddWithValue("@FirstName", reservation.FirstName);
                        checkCmd.Parameters.AddWithValue("@LastName", reservation.LastName);

                        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                        if (count > 0)
                        {
                            return Ok(new { message = "Reservation already created" });
                        }
                    }

                    string query = @"
                        INSERT INTO reservations 
                        (id, first_name, last_name, travel_offer_id, price_list_id, total_price, total_travel_time, company_name) 
                        VALUES (@Id, @FirstName, @LastName, @TravelOfferID, @PriceListId, @TravelPrice, @TravelTime, @CompanyName)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        cmd.Parameters.AddWithValue("@FirstName", reservation.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", reservation.LastName);
                        cmd.Parameters.AddWithValue("@TravelOfferID", reservation.TravelOfferID);
                        cmd.Parameters.AddWithValue("@PriceListId", reservation.PricelistId);
                        cmd.Parameters.AddWithValue("@TravelPrice", reservation.TravelPrice);
                        cmd.Parameters.AddWithValue("@TravelTime", reservation.TravelTime);
                        cmd.Parameters.AddWithValue("@CompanyName", reservation.CompanyName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Reservation created");
                return Ok(new { message = "Reservation successful!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

namespace Cosmos_odyssey.Moduls
{
    public class TravelOffers
    {

        public string TravelId { get; set; }

        public string PricelistId { get; set; }

        public string FromPlanet {  get; set; }

        public string ToPlanet { get; set; }

        public long Distance { get; set; }

        public decimal Price { get; set; }

        public DateTime TravelStart { get; set; }

        public DateTime TravelEnd { get; set; }

        public string CompanyName { get; set; }
    }
}

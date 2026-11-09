namespace HappyPet.Application.Dtos.Requests
{
    public class ReportLostPetRequest
    {
        /// <summary>When the pet was last seen (UTC).</summary>
        public DateTime LastSeenAt { get; set; }

        /// <summary>Latitude of the last seen location (-90..90).</summary>
        public double Latitude { get; set; }

        /// <summary>Longitude of the last seen location (-180..180).</summary>
        public double Longitude { get; set; }

        /// <summary>Human-readable address of the last seen location.</summary>
        public string? LastSeenAddress { get; set; }

        /// <summary>Contact phone shown publicly on the lost-pet listing. Optional.</summary>
        public string? ContactPhone { get; set; }
    }
}

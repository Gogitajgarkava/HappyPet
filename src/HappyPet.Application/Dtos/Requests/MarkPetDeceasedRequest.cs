namespace HappyPet.Application.Dtos.Requests
{
    public class MarkPetDeceasedRequest
    {
        public DateTime? DeceasedAt { get; set; }

        public string? DeathCause { get; set; }
    }
}

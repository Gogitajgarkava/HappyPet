namespace HappyPet.Application.Dtos.Responses
{
    public class PetPhotoResponse
    {
        public Guid Id { get; set; }
        public Guid PetId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

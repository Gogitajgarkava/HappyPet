namespace HappyPet.Application.Dtos.Responses
{
    public class PetPhotoResponse
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

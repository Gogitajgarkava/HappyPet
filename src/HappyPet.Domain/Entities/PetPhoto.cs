using HappyPet.Domain.Common;

namespace HappyPet.Domain.Entities
{
    public class PetPhoto : BaseEntity
    {
        public Guid PetId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public bool IsPrimary { get; set; }
        public Pet Pet { get; set; } = null!;
    }
}

namespace HappyPet.Application.Interfaces.Storage
{
    public class StoredFile
    {
        public string Key { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public interface IFileStorageService
    {
        Task<StoredFile> UploadAsync(
            Stream stream,
            string folder,
            string extension,
            string contentType,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string key, CancellationToken cancellationToken = default);

        string GetUrl(string key);
    }
}

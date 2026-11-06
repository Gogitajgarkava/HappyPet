using HappyPet.Application.Dtos.Airs;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Airs;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HappyPet.Infrastructure.Airs
{
    public class AirsClient : IAirsClient
    {
        private readonly HttpClient _httpClient;

        public AirsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AirsAnimalDetails?> GetAnimalByMicrochipAsync(
            string microchipNumber,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestPath =
                    $"api/v1/airs/animals/by-microchip/{Uri.EscapeDataString(microchipNumber)}";

                using var response = await _httpClient.GetAsync(requestPath, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new AirsUnavailableException(
                        "AIRS სერვისი დროებით მიუწვდომელია. გთხოვთ, სცადოთ მოგვიანებით.");
                }

                var details = await response.Content.ReadFromJsonAsync<AirsAnimalDetails>(
                    cancellationToken: cancellationToken);

                if (details == null)
                {
                    throw new AirsUnavailableException(
                        "AIRS სერვისმა დააბრუნა ცარიელი პასუხი. გთხოვთ, სცადოთ მოგვიანებით.");
                }

                return details;
            }
            catch (AirsException)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AirsUnavailableException(
                    "AIRS სერვისი დროებით მიუწვდომელია. გთხოვთ, სცადოთ მოგვიანებით.",
                    ex);
            }
            catch (HttpRequestException ex)
            {
                throw new AirsUnavailableException(
                    "AIRS სერვისი დროებით მიუწვდომელია. გთხოვთ, სცადოთ მოგვიანებით.",
                    ex);
            }
        }
    }
}


namespace SharedProject.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly HttpClient _httpClient;
        public FileSystemService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        async public Task<string> PostFileAsync(string file)
        {
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync<string>("/", file);
                httpResponse.EnsureSuccessStatusCode();
                return httpResponse.Content.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}

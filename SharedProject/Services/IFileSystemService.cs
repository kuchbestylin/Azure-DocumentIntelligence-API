
namespace SharedProject.Services
{
    public interface IFileSystemService
    {
        Task<string> PostFileAsync(string file);
    }
}

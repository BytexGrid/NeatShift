using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeatShift.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private const string GITHUB_API_URL = "https://api.github.com/repos/BytexGrid/NeatShift/releases/latest";

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NeatShift");
        }

        public async Task<(bool IsUpdateAvailable, string LatestVersion)> CheckForUpdates()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(GITHUB_API_URL);
                using var document = JsonDocument.Parse(response);
                var root = document.RootElement;
                var latestVersion = root.GetProperty("tag_name").GetString()?.TrimStart('v');

                if (string.IsNullOrEmpty(latestVersion))
                    return (false, NeatShift.Version.Current);

                // Compare versions using System.Version
                var current = new System.Version(NeatShift.Version.Current);
                var latest = new System.Version(latestVersion);

                return (latest > current, latestVersion);
            }
            catch
            {
                // Fail silently - don't bother user if update check fails
                return (false, NeatShift.Version.Current);
            }
        }
    }
} 
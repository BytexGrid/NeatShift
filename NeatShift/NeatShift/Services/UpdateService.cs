using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeatShift.Services
{
    public interface IUpdateService
    {
        Task<UpdateInfo?> CheckForUpdates();
        string GetCurrentVersion();
    }

    public class UpdateInfo
    {
        public string CurrentVersion { get; set; } = string.Empty;
        public string NewVersion { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class UpdateService : IUpdateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string GithubApiUrl = "https://api.github.com/repos/YourUsername/NeatShift/releases/latest";

        public UpdateService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string GetCurrentVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version?.ToString() ?? "1.0.0";
        }

        public async Task<UpdateInfo?> CheckForUpdates()
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("User-Agent", "NeatShift-Update-Checker");
                
                var response = await client.GetStringAsync(GithubApiUrl);
                var versionMatch = Regex.Match(response, "\"tag_name\":\"([0-9.]{5,9})\"", RegexOptions.Multiline);
                
                if (!versionMatch.Success)
                {
                    return null;
                }

                var newVersion = versionMatch.Groups[1].Value;
                var currentVersion = GetCurrentVersion();

                if (currentVersion == newVersion)
                {
                    return null;
                }

                return new UpdateInfo
                {
                    CurrentVersion = currentVersion,
                    NewVersion = newVersion,
                    DownloadUrl = "https://github.com/YourUsername/NeatShift/releases/latest"
                };
            }
            catch (HttpRequestException)
            {
                // Network error or GitHub API issue
                return null;
            }
            catch (JsonException)
            {
                // JSON parsing error
                return null;
            }
            catch (Exception)
            {
                // Any other unexpected error
                return null;
            }
        }
    }
} 
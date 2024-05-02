using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetRelease.Sources;

internal class GitHubParser
{
    private class ApiResponse
    {
        [JsonPropertyName("tag_name")]
        public required string TagName { get; set; }
        [JsonPropertyName("assets")]
        public required ReleaseAssets[] Assets { get; set; }
    }

    private class ReleaseAssets
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("browser_download_url")]
        public required string BrowserDownloadUrl { get; set; }
    }

    private static async Task<ApiResponse?> GetApiResponse(string repo, bool verbose)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Elypha/GetRelease");

        try
        {   if (verbose)
            {
                Console.WriteLine($"Requesting: https://api.github.com/repos/{repo}/releases/latest");
            }
            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/repos/{repo}/releases/latest");
            response.EnsureSuccessStatusCode();

            var data = await JsonSerializer.DeserializeAsync<ApiResponse>(response.Content.ReadAsStream());
            return data;
        }
        catch (HttpRequestException e)
        {
            if (verbose)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            return null;
        }
        catch (JsonException e)
        {
            if (verbose)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            return null;
        }
        finally
        {
            client.Dispose();
        }
    }

    public static int ParseRelease(string repo, string pattern, bool verbose)
    {
        var data = Task.Run(() => GetApiResponse(repo, verbose)).Result;
        if (data is null) return 1;
        if (verbose)
        {
            Console.WriteLine($"Tag: {data.TagName}");
        }

        var regex = new Regex(pattern);
        var assets = data.Assets.Where(asset => regex.Match(asset.Name).Success);

        if (assets.Count() != 1)
        {
            if (verbose)
            {
                Console.WriteLine($"Error: {assets.Count()} assets found");
                foreach (var i in assets)
                {
                    Console.WriteLine($"{i.BrowserDownloadUrl}");
                }
            }
            return 1;

        };

        var asset = assets.First();
        Console.WriteLine($"{asset.BrowserDownloadUrl}");
        return 0;
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpotifyAPI.Web;

namespace MusicSync.RemoteServices.Spotify;

public class SpotifyAdapter : ISpotifyAdapter
{
    private readonly SpotifyConfiguration _configuration;
    private SpotifyClient? _client;

    public SpotifyAdapter(IConfiguration configuration)
    {
        _configuration = configuration.GetSection("Spotify").Get<SpotifyConfiguration>();;
    }

    public async Task<SpotifyClient> Client()
    {
        return _client ??= await BuildSpotifyClient();
    }

    private async Task<SpotifyClient> BuildSpotifyClient()
    {
        var accessToken = await GetAccessToken(_configuration);

        return new SpotifyClient(accessToken);
    }
    private static async Task<string> GetAccessToken(SpotifyConfiguration configuration)
    {
        var formData = new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", configuration.RefreshToken),
            new KeyValuePair<string, string>("client_id", configuration.ClientId)
        };
            
        var request = new HttpRequestMessage(HttpMethod.Post, configuration.RefreshAccessTokenUrl)
        {
            Content = new FormUrlEncodedContent(formData),
        };
        
        request.Headers.Add("Authorization", $"Basic {System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{configuration.ClientId}:{configuration.ClientSecret}"))}");

        HttpResponseMessage response;
        using (var httpClient = new HttpClient())
        {
            response = await httpClient.SendAsync(request);
        }
            
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to refresh token. StatusCode: '{response.StatusCode}' Reason: '{response.ReasonPhrase}'");
        }

        var contentString = await response.Content.ReadAsStringAsync();
        var refreshAccessTokenResponse = JsonSerializer.Deserialize<RefreshAccessTokenResponse>(contentString);

        if (refreshAccessTokenResponse == null)
        {
            throw new Exception($"Failed to deserialize CreateClientResult. StatusCode: '{response.StatusCode}' ResponseContent: '{contentString}'");
        }

        return refreshAccessTokenResponse.AccessToken;
    }

    public class RefreshAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}

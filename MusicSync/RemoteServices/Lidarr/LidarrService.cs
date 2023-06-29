using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices.Lidarr.Responses;

namespace MusicSync.RemoteServices.Lidarr;

public class LidarrService : ILidarrService, IRemoteArtistService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LidarrService> _logger;

    public LidarrService(IConfiguration configuration, HttpClient httpClient, ILogger<LidarrService> logger)
    {
        var lidarrConfiguration = configuration.GetSection("Lidarr").Get<LidarrConfiguration>();

        httpClient.DefaultRequestHeaders.Add("X-Api-Key", lidarrConfiguration.ApiKey);
        httpClient.BaseAddress = new Uri(lidarrConfiguration.BaseUrl);

        _httpClient = httpClient;
        _logger = logger;
    }
    public IRemoteService.ServiceType Type()
    {
        return IRemoteService.ServiceType.Lidarr;
    }

    public async Task AddArtist(RemoteArtist artist)
    {
        _logger.LogInformation($"Adding Artist: {artist.Name} to Lidarr");
        
        var serialisedRequestModel = JsonSerializer.Serialize(artist.ToRequestModel());
        var requestContent = new StringContent(serialisedRequestModel, Encoding.UTF8);

        try
        {
            await _httpClient.PostAsync(Constants.Paths.ArtistPath, requestContent);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unable to add artist Name: '{artist.Name}' RemoteId: '{artist.RemoteId}'");
        }
    }

    public async Task<IEnumerable<RemoteArtist>> GetArtists()
    {
        _logger.LogDebug("Fetching all artists");

        var response = await _httpClient.GetAsync(Constants.Paths.ArtistLookupPath);
        var content = await response.Content.ReadAsStringAsync();
        var artistsResponse = JsonSerializer.Deserialize<IList<GetArtistsResponse>>(content);

        _logger.LogDebug("Found '{artistCount}' artists", artistsResponse?.Count());

        return artistsResponse?.Select(artist => artist.ToRemoteArtist()) ?? new List<RemoteArtist>();
    }

    public async Task<RemoteArtist?> SearchArtists(ArtistEntity artist)
    {
        _logger.LogDebug("Searching for Artist: '{artistName}'", artist.Name);

        var url = $"{Constants.Paths.ArtistLookupPath}?term={artist.Name}";

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var artistLookupResponse = JsonSerializer.Deserialize<IList<GetArtistsResponse>>(content) ?? new List<GetArtistsResponse>();
        
        _logger.LogDebug("Found {totalNumberOfResults} results for Artist: '{artistName}'", artistLookupResponse.Count, artist.Name);

        if (!artistLookupResponse.Any())
        {
            return null;
        }

        return artistLookupResponse.First().ToRemoteArtist();
    }
}

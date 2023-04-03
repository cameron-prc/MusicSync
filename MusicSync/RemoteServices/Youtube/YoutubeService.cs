using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicSync.Common;

namespace MusicSync.RemoteServices.Youtube;

public class YoutubeService : IYoutubeService
{
    private readonly ILogger<YoutubeService> _logger;
    private readonly YoutubeConfiguration _configuration;
    private readonly YouTubeService _service;
    
    public YoutubeService(IConfiguration configuration, ILogger<YoutubeService> logger)
    {
        _logger = logger;
        _configuration = configuration.GetSection("Youtube").Get<YoutubeConfiguration>();
        
        var token = new TokenResponse {RefreshToken = _configuration.RefreshToken};
        var credentials = new UserCredential(new AuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer {ClientSecrets = new ClientSecrets
            {
                ClientId = _configuration.ClientId,
                ClientSecret = _configuration.ClientSecret
            }}
        ), "user", token);

        _service = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = "MusicSync"
        });
    }
    
    public Task<IEnumerable<RemoteTrack>> GetPlaylist(string playlistId)
    {
        _logger.LogDebug("Fetching playlist PlaylistId: '{playlistId}'", playlistId);

        var playlistItems = new List<PlaylistItem>();
        var playlistItemsRequest = _service.PlaylistItems.List("snippet");
        PlaylistItemListResponse? response = null;

        playlistItemsRequest.PlaylistId = playlistId;

        do
        {
            _logger.LogDebug("Fetching playlist page {token}", response?.NextPageToken);

            playlistItemsRequest.PageToken = response?.NextPageToken;
            response = playlistItemsRequest.Execute();

            playlistItems = playlistItems.Concat(response.Items).ToList();
        }
        while (response.NextPageToken != null);

        _logger.LogDebug("Playlist retrieved with {totalTracks} track'", playlistItems.Count);

        var remoteTracks = playlistItems
            .Select(playlistItem => playlistItem.ToRemoteTrack());

        return Task.FromResult(remoteTracks);
    }

    public async Task<RemoteTrack?> SearchTracks(TrackEntity track)
    {
        if (track.Title == null || track.ArtistName == null)
        {
            _logger.LogInformation("Unable to search for track Title: '{title}' Artist: '{artistName}', one or more values are empty", track.Title, track.ArtistName);
            return null;
        }

        var request = _service.Search.List("snippet");
        request.Q = $"{track.Title} {track.ArtistName}";
        request.MaxResults = 10;

        var searchResult = await request.ExecuteAsync();
        
        _logger.LogDebug("Found {totalNumberOfResults} results for Title: '{title}' Artist: '{artistName}'", searchResult.Items.Count, track.Title, track.ArtistName);

        return searchResult.Items.FirstOrDefault()?.ToRemoteTrack(track);
    }

    public async Task AddToPlaylist(string playlistId, IList<TrackEntity> tracks)
    {
        var youtubeIds = tracks
            .Select(track => track.YoutubeId)
            .Where(youtubeId => youtubeId != null)
            .Select(youtubeId => youtubeId!)
            .ToList();

        var missingIdCount = tracks.Count - youtubeIds.Count;

        if (missingIdCount > 0)
        {
            _logger.LogInformation("{missingIdCount} track/s have missing Youtube ids", missingIdCount);
        }

        _logger.LogInformation("Adding {newTrackCount} track/s to PlaylistId: '{playlistId}'", youtubeIds.Count, playlistId);

        foreach (var youtubeId in youtubeIds)
        {
            var playlistItem = new PlaylistItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = playlistId,
                    ResourceId = new ResourceId {Kind = "youtube#video", VideoId = youtubeId}
                }
            };

            await _service.PlaylistItems.Insert(playlistItem, "snippet").ExecuteAsync();
        }
    }
}

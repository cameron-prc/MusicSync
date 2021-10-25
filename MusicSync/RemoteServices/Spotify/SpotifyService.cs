using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using SpotifyAPI.Web;

namespace MusicSync.RemoteServices.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly ILogger<SpotifyService> _logger;
    private readonly SpotifyClient _client;

    public SpotifyService(IConfiguration configuration, ILogger<SpotifyService> logger)
    {
        _logger = logger;
        _client = new SpotifyAdapter(configuration).BuildSpotifyClient().Result;
    }

    public async Task<IEnumerable<RemoteTrack>> GetPlaylist(string playlistId)
    {
        _logger.LogDebug("Fetching playlist PlaylistId: '{playlistId}'", playlistId);
        var playlist = await _client.Playlists.Get(playlistId);

        if (playlist == null)
        {
            throw new Exception($"Unable to find playlist with PlaylistId: '${playlistId}'");
        }

        _logger.LogDebug("Playlist retrieved with {totalTracks} track'", playlist.Tracks?.Total);

        if (playlist.Tracks == null)
        {
            return new List<RemoteTrack>();
        }
        
        var tracks = await _client.PaginateAll(playlist.Tracks);
        var remoteTracks = new List<RemoteTrack>();
        
        foreach (var item in tracks)
        {
            if (item.Track is FullTrack track)
            {
                remoteTracks.Add(track.ToRemoteTrack());
            }
        }

        return remoteTracks;
    }

    public async Task<RemoteTrack?> SearchTracks(TrackEntity track)
    {
        if (track.Title == null || track.ArtistName == null)
        {
            _logger.LogInformation("Unable to search for track Title: '{title}' Artist: '{artistName}', one or more values are empty", track.Title, track.ArtistName);
            return null;
        }

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, $"track:${track.Title} artist:${track.ArtistName}");
        var result = await _client.Search.Item(searchRequest);

        _logger.LogDebug("Found {totalNumberOfResults} results for Title: '{title}' Artist: '{artistName}'", result.Tracks.Items?.Count, track.Title, track.ArtistName);

        var firstTrackResult = result.Tracks.Items?.FirstOrDefault();

        return firstTrackResult?.ToRemoteTrack();
    }

    public async Task AddToPlaylist(string playlistId, IList<TrackEntity> tracks)
    {
        var spotifyUris = tracks
            .Select(track => track.GetSpotifyUri())
            .Where(spotifyUri => spotifyUri != null)
            .Select(spotifyUri => spotifyUri!)
            .ToList();

        var missingIdCount = tracks.Count - spotifyUris.Count;

        if (missingIdCount > 0)
        {
            _logger.LogInformation("{missingIdCount} track/s have missing Spotify ids", missingIdCount);
        }

        _logger.LogInformation("Adding {newTrackCount} track/s to PlaylistId: '{playlistId}'", spotifyUris.Count, playlistId);

        var request = new PlaylistAddItemsRequest(spotifyUris);

        await _client.Playlists.AddItems(playlistId, request);
    }

    public Task AddToPlaylist(IList<TrackEntity> tracks)
    {
        throw new NotImplementedException();
    }
}

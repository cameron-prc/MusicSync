using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using SpotifyAPI.Web;

namespace MusicSync.RemoteServices.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly ILogger<SpotifyService> _logger;
    private readonly ISpotifyAdapter _adapter;

    public SpotifyService(ISpotifyAdapter adapter, ILogger<SpotifyService> logger)
    {
        _logger = logger;
        _adapter = adapter;
    }

    public IRemoteService.ServiceType Type()
    {
        return IRemoteService.ServiceType.Spotify;
    }

    public async Task<RemotePlaylist> GetPlaylist(string playlistId)
    {
        var client = await _adapter.Client();

        _logger.LogDebug("Fetching playlist PlaylistId: '{playlistId}'", playlistId);
        var playlist = await client.Playlists.Get(playlistId);
        var remoteTracks = new List<RemoteTrack>();

        if (playlist == null)
        {
            throw new Exception($"Unable to find playlist with PlaylistId: '${playlistId}'");
        }

        _logger.LogDebug("Playlist retrieved with {totalTracks} track'", playlist.Tracks?.Total);

        if (playlist.Tracks != null)
        {
            var tracks = await client.PaginateAll(playlist.Tracks);

            foreach (var item in tracks)
            {
                if (item.Track is FullTrack track)
                {
                    remoteTracks.Add(track.ToRemoteTrack());
                }
            }
        }

        return new RemotePlaylist
        {
            Id = playlistId,
            ServiceType = IRemoteService.ServiceType.Spotify,
            Tracks = remoteTracks
        };
    }

    public async Task<RemoteTrack?> SearchTracks(TrackEntity track)
    {
        var client = await _adapter.Client();

        if (track.Title == null)
        {
            _logger.LogInformation("Unable to search for track Title: '{title}' Artist: '{artistName}'", track.Title, track.Artist.Name);
            return null;
        }

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, $"track:${track.Title} artist:${track.Artist.Name}");
        var result = await client.Search.Item(searchRequest);

        _logger.LogDebug("Found {totalNumberOfResults} results for Title: '{title}' Artist: '{artistName}'", result.Tracks.Items?.Count, track.Title, track.Artist.Name);

        var firstTrackResult = result.Tracks.Items?.FirstOrDefault();

        return firstTrackResult?.ToRemoteTrack();
    }

    public async Task AddToPlaylist(string playlistId, IList<TrackEntity> tracks)
    {
        var client = await _adapter.Client();

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

        await client.Playlists.AddItems(playlistId, request);
    }

    public Task AddToPlaylist(IList<TrackEntity> tracks)
    {
        throw new NotImplementedException();
    }
}

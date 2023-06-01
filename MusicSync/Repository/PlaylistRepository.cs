using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices;

namespace MusicSync.Repository;

public class PlaylistRepository : IRepositoryClient
{
    private readonly IDatabase _database;
    private readonly ILogger<PlaylistRepository> _logger;

    public PlaylistRepository(IDatabase database, ILogger<PlaylistRepository> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<PlaylistEntity> CreatePlaylist(string name)
    {
        _logger.LogInformation("Creating new local playlist PlaylistId: '{playlistId}'", name);

        var playlist = new PlaylistEntity {Id = Guid.NewGuid().ToString(), Name = name, Tracks = new List<TrackEntity>()};

        await using var connection = _database.GetConnection();

        await connection.ExecuteAsync(@"
            INSERT INTO Playlists
            VALUES(@Id, @Name)", playlist
        );

        return playlist;
    }

    public async Task AddToPlaylist(string playlistId, IEnumerable<TrackEntity> newEntries)
    {
        _logger.LogInformation("Adding {newTrackCount} tracks to playlist PlaylistId: '{playlistId}'", newEntries.Count(), playlistId);

        var @params = new List<object>();

        foreach (var entry in newEntries)
        {
            @params.Add(new { PlaylistId = playlistId, TrackId = entry.LocalId });
        }

        await using var connection = _database.GetConnection();

        await connection.ExecuteAsync(@"
            INSERT INTO PlaylistEntries
            VALUES(@PlaylistId, @TrackId);", @params
        );
    }

    public async Task CreateTracks(List<TrackEntity> tracks, IRemoteService.ServiceType remoteServiceType)
    {
        _logger.LogInformation("Persisting {newTrackCount} new tracks", tracks.Count);

        await using var connection = _database.GetConnection();

        await connection.ExecuteAsync(@"
                INSERT INTO Tracks(LocalId, YoutubeId, Title, ArtistName, SpotifyId)
                VALUES(@LocalId, @YoutubeId, @Title, @ArtistName, @SpotifyId)", tracks
        );
    }

    public async Task SetRemoteId(TrackEntity track, IRemoteService.ServiceType remoteServiceType)
    {
        var queryKey = remoteServiceType switch
        {
            IRemoteService.ServiceType.YouTube => "YoutubeId",
            IRemoteService.ServiceType.Spotify => "SpotifyId",
            _ => throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null)
        };
        var query = $"UPDATE Tracks SET {queryKey} = @RemoteId WHERE LocalId = @LocalId";

        _logger.LogDebug("Setting {queryKey} to '{remoteId}' for TrackId: '{Id}'", queryKey, track.GetId(remoteServiceType), track.Id);

        await using var connection = _database.GetConnection();
        await connection.ExecuteAsync(query, new { RemoteId = track.GetId(remoteServiceType), LocalId = track.LocalId});
    }

    async Task<PlaylistEntity?> IRepositoryClient.GetPlaylist(string name)
    {
        _logger.LogInformation("Retrieving local playlist PlaylistId: '{name}'", name);

        await using var connection = _database.GetConnection();

        PlaylistEntity? playlist = null;

        var queryAsync = (await connection.QueryAsync<PlaylistEntity, TrackEntity, PlaylistEntity>(@"
                    SELECT p.*, t.*
                    FROM Playlists p
                    INNER JOIN PlaylistEntries pe
                    ON pe.PlaylistId = p.Id
                    INNER JOIN Tracks t
                    ON t.LocalId = pe.TrackId
                    WHERE p.Name = @Name

                    ", (playlistDto, track) =>
                    {
                        playlist ??= playlistDto;

                        playlist.Tracks.Add(track);
                        return playlist;
                    }, new { Name = name }, splitOn: "LocalId"
                )
            );

        return playlist;
    }
}

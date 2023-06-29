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
            @params.Add(new { PlaylistId = playlistId, TrackId = entry.Id });
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

        var trackDtos = tracks.Select(track => new TrackDto(track));
        
        await using var connection = _database.GetConnection();

        await connection.ExecuteAsync(@"
                INSERT INTO Tracks(Id, YoutubeId, Title, SpotifyId, ArtistId)
                VALUES(@Id, @YoutubeId, @Title, @SpotifyId, @ArtistId)", trackDtos
        );
    }

    public async Task CreateArtists(List<ArtistEntity> artists)
    {
        _logger.LogInformation("Persisting {newArtistCount} new artists", artists.Count);

        await using var connection = _database.GetConnection();

        await connection.ExecuteAsync(@"
                INSERT INTO Artists(Id, Name, SpotifyId, YoutubeId, MusicBrainzId)
                VALUES(@Id, @Name, @SpotifyId, @YoutubeId, @MusicBrainzId)", artists
        );
    }

    public async Task SetRemoteId(ArtistEntity artist, IRemoteService.ServiceType remoteServiceType)
    {
        var queryKey = remoteServiceType switch
        {
            IRemoteService.ServiceType.YouTube => "YoutubeId",
            IRemoteService.ServiceType.Spotify => "SpotifyId",
            IRemoteService.ServiceType.Lidarr => "MusicBrainzId",
            _ => throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null)
        };
        // switch on subset
        var query = $"UPDATE Artists SET {queryKey} = @RemoteId WHERE Id = @Id";

        _logger.LogDebug("Setting {queryKey} to '{remoteId}' for ArtistId: '{Id}'", queryKey, artist.GetId(remoteServiceType), artist.Id);

        await using var connection = _database.GetConnection();
        await connection.ExecuteAsync(query, new { RemoteId = artist.GetId(remoteServiceType), artist.Id});
    }

    public async Task SetRemoteId(TrackEntity track, IRemoteService.ServiceType remoteServiceType)
    {
        var queryKey = remoteServiceType switch
        {
            IRemoteService.ServiceType.YouTube => "YoutubeId",
            IRemoteService.ServiceType.Spotify => "SpotifyId",
            IRemoteService.ServiceType.Lidarr => throw new Exception("Invalid operation. Attempted to set Lidarr source on TrackEntity"),
            _ => throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null)
        };
        var query = $"UPDATE Tracks SET {queryKey} = @RemoteId WHERE Id = @Id";

        _logger.LogDebug("Setting {queryKey} to '{remoteId}' for TrackId: '{Id}'", queryKey, track.GetId(remoteServiceType), track.Id);

        await using var connection = _database.GetConnection();
        await connection.ExecuteAsync(query, new { RemoteId = track.GetId(remoteServiceType), track.Id});
    }

    public async Task<IEnumerable<ArtistEntity>> FetchArtists(IEnumerable<string> artistIds)
    {
        await using var connection = _database.GetConnection();

        return await connection.QueryAsync<ArtistEntity>(@"
                    SELECT *
                    FROM Artists
                    WHERE Id IN @Ids", new { Ids = artistIds }
                );
    }

    public async Task<IEnumerable<ArtistEntity>> FetchArtists()
    {
        await using var connection = _database.GetConnection();

        return await connection.QueryAsync<ArtistEntity>(@"
                    SELECT *
                    FROM Artists"
                );
    }

    async Task<PlaylistEntity?> IRepositoryClient.GetPlaylist(string name)
    {
        _logger.LogInformation("Retrieving local playlist PlaylistId: '{name}'", name);

        await using var connection = _database.GetConnection();

        PlaylistEntity? playlist = null;

        var queryAsync = (await connection.QueryAsync<PlaylistEntity, TrackDto, ArtistEntity, PlaylistEntity>(@"
                    SELECT p.*, t.*, a.*
                    FROM Playlists p
                    INNER JOIN PlaylistEntries pe
                    ON pe.PlaylistId = p.Id
                    INNER JOIN Tracks t
                    ON t.Id = pe.TrackId
                    INNER JOIN Artists a
                    ON t.ArtistId = a.Id
                    WHERE p.Name = @Name

                    ", (playlistEntity, track, artist) =>
                    {
                        playlist ??= playlistEntity;

                        playlist.Tracks.Add(new TrackEntity(track, artist));
                        return playlist;
                    }, new { Name = name }
                )
            );

        return playlist;
    }
}

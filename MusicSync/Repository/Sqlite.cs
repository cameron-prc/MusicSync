using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace MusicSync.Repository;

public class Sqlite : IDatabase
{
    private readonly DatabaseConfig _configuration;

    public Sqlite(IConfiguration configuration)
    {
        _configuration = configuration.Get<DatabaseConfig>();
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_configuration.DatabaseName);
    }

    public void Setup()
    {
        using var connection = new SqliteConnection(_configuration.DatabaseName);

        var playlistTable =
            connection.Query("SELECT name from sqlite_master WHERE type = 'table' AND name = 'Playlists';").FirstOrDefault();

        if (playlistTable == null)
        {
            connection.Execute("CREATE TABLE Playlists(Id VARCHAR(45) PRIMARY KEY, Name VARCHAR(100) NOT NULL);");
        }
        
        var tracksTable =
            connection.Query("SELECT name from sqlite_master WHERE type = 'table' AND name = 'Tracks';").FirstOrDefault();

        if (tracksTable == null)
        {
            connection.Execute("CREATE TABLE Tracks(Id VARCHAR(45) PRIMARY KEY, YoutubeId VARCHAR(45), SpotifyId VARCHAR(45), Title VARCHAR(45), ArtistId VARCHAR(45) REFERENCES Artists(Id));");
        }
        
        var artistsTable =
            connection.Query("SELECT name from sqlite_master WHERE type = 'table' AND name = 'Artists';").FirstOrDefault();

        if (artistsTable == null)
        {
            connection.Execute("CREATE TABLE Artists(Id VARCHAR(45) PRIMARY KEY, Name VARCHAR(45), SpotifyId VARCHAR(45), YoutubeId VARCHAR(45));");
        }
        
        var playlistEntryJoinTable =
            connection.Query("SELECT name from sqlite_master WHERE type = 'table' AND name = 'PlaylistEntries';").FirstOrDefault();

        if (playlistEntryJoinTable == null)
        {
            connection.Execute("CREATE TABLE PlaylistEntries(PlaylistId VARCHAR(45) REFERENCES Playlists(Id), TrackId VARCHAR(45) REFERENCES Tracks(Id));");
        }
    }
}

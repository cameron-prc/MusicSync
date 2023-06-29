using System;
using MusicSync.RemoteServices;

namespace MusicSync.Common;

public class TrackEntity
{
    public string? Title { get; init; }
    public ArtistEntity Artist { get; init; }
    public string? YoutubeId { get; set; }
    public string? SpotifyId { get; set; }
    public string Id { get; set; } = null!;

    public TrackEntity() {}

    public TrackEntity(TrackDto dto, ArtistEntity artist)
    {
        Title = dto.Title;
        Artist = artist;

        Id = dto.Id;
        YoutubeId = dto.YoutubeId;
        SpotifyId = dto.SpotifyId;
    }

    public TrackEntity(RemoteTrack track, ArtistEntity artist)
    {
        Id = Guid.NewGuid().ToString();
        Title = track.TrackName;
        Artist = artist;

        SetRemoteId(track.RemoteServiceType, track.RemoteId);
    }

    public string? GetId(IRemoteService.ServiceType remoteServiceType)
    {
        return remoteServiceType switch
        {
            IRemoteService.ServiceType.YouTube => YoutubeId,
            IRemoteService.ServiceType.Spotify => SpotifyId,
            _ => throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null)
        };
    }

    public void SetRemoteId(IRemoteService.ServiceType remoteServiceType, string remoteId)
    {
        switch (remoteServiceType)
        {
            case IRemoteService.ServiceType.YouTube:
                YoutubeId = remoteId;
                break;
            case IRemoteService.ServiceType.Spotify:
                SpotifyId = remoteId;
                break;
            case IRemoteService.ServiceType.Lidarr:
                throw new Exception("Invalid remote type: Attempted to set Lidarr as source for track");
            default:
                throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null);
        }
    }
}

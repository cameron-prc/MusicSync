using System;
using MusicSync.RemoteServices;

namespace MusicSync.Common;

public class TrackEntity
{
    public TrackEntity() {}

    public TrackEntity(RemoteTrack track)
    {
        LocalId = Guid.NewGuid().ToString();
        Title = track.TrackName;
        ArtistName = track.ArtistName;

        SetRemoteId(track.RemoteServiceType, track.RemoteId);
    }

    public string? Title { get; init; }
    public string? ArtistName { get; init; }
    public string? YoutubeId { get; set; }
    public string? SpotifyId { get; set; }
    public string LocalId { get; set; } = null!;

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
            default:
                throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null);
        }
    }
}

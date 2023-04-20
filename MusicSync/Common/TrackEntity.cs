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

        SetRemoteId(track.RemoteType, track.RemoteId);
    }

    public string? Title { get; init; }
    public string? ArtistName { get; init; }
    public string? YoutubeId { get; set; }
    public string? SpotifyId { get; set; }
    public string LocalId { get; set; } = null!;

    public string? GetId(IRemoteService.Type remoteType)
    {
        return remoteType switch
        {
            IRemoteService.Type.YouTube => YoutubeId,
            IRemoteService.Type.Spotify => SpotifyId,
            _ => throw new ArgumentOutOfRangeException(nameof(remoteType), remoteType, null)
        };
    }

    public void SetRemoteId(IRemoteService.Type remoteType, string remoteId)
    {
        switch (remoteType)
        {
            case IRemoteService.Type.YouTube:
                YoutubeId = remoteId;
                break;
            case IRemoteService.Type.Spotify:
                SpotifyId = remoteId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(remoteType), remoteType, null);
        }
    }
}

using System;
using MusicSync.RemoteServices;

namespace MusicSync.Common;

public class ArtistEntity
{
    public string Id { get; } = null!;
    public string Name { get; }
    public string? SpotifyId { get; set; }
    public string? YoutubeId { get; set; }

    public ArtistEntity() {}
    
    public ArtistEntity(RemoteArtist artist)
    {
        Id = Guid.NewGuid().ToString();
        Name = artist.Name;

        SetRemoteId(artist.RemoteServiceType, artist.RemoteId);
    }
    
    public string? GetId(IRemoteService.ServiceType remoteServiceType)
    {
        return remoteServiceType switch
        {
            IRemoteService.ServiceType.Spotify => SpotifyId,
            IRemoteService.ServiceType.YouTube => YoutubeId,
            _ => throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null)
        };
    }

    public void SetRemoteId(IRemoteService.ServiceType remoteServiceType, string remoteId)
    {
        switch (remoteServiceType)
        {
            case IRemoteService.ServiceType.Spotify:
                SpotifyId = remoteId;
                break;
            case IRemoteService.ServiceType.YouTube:
                YoutubeId = remoteId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(remoteServiceType), remoteServiceType, null);
        }
    }
}

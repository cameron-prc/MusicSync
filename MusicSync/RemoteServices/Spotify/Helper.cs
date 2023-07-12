using System.Linq;
using MusicSync.Common;
using SpotifyAPI.Web;

namespace MusicSync.RemoteServices.Spotify;

public static class Helper
{
    public static RemoteTrack ToRemoteTrack(this FullTrack fullTrack)
    {
        return new RemoteTrack
        {
            RemoteId = fullTrack.Id,
            TrackName = fullTrack.Name,
            Artist = fullTrack.Artists.First().ToRemoteArtist(),
            RemoteServiceType = IRemoteService.ServiceType.Spotify
        };
    }
    
    public static RemoteArtist ToRemoteArtist(this FullArtist artist)
    {
        return new RemoteArtist(IRemoteService.ServiceType.Spotify, artist.Id, artist.Name);
    }

    public static RemoteArtist ToRemoteArtist(this SimpleArtist artist)
    {
        return new RemoteArtist(IRemoteService.ServiceType.Spotify, artist.Id, artist.Name);
    }

    public static string? GetSpotifyUri(this TrackEntity track)
    {
        return track.SpotifyId == null ? null : $"spotify:track:{track.SpotifyId}";
    }
}

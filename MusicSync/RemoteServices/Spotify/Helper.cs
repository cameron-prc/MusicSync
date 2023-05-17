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
            RemoteServiceType = IRemoteService.ServiceType.Spotify,
            TrackName = fullTrack.Name,
            ArtistName = fullTrack.Artists.First().Name // First artist is the primary artist
        };
    }

    public static string? GetSpotifyUri(this TrackEntity track)
    {
        return track.SpotifyId == null ? null : $"spotify:track:{track.SpotifyId}";
    }
}

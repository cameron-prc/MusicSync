using System.Threading.Tasks;
using MusicSync.Common;

namespace MusicSync.RemoteServices;

public interface IRemoteService
{
    public Task<RemoteTrack?> SearchTracks(TrackEntity track);

    public ServiceType Type();
    public enum ServiceType
    {
        YouTube,
        Spotify
    }
}

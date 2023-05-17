using System.Collections.Generic;
using System.Threading.Tasks;
using MusicSync.Common;

namespace MusicSync.RemoteServices;

public interface IRemotePlaylistService : IRemoteService
{
    public Task<RemoteTrack?> SearchTracks(TrackEntity track);
    public Task<IEnumerable<RemoteTrack>> GetPlaylist(string playlistId);
    public Task AddToPlaylist(string playlistId, IList<TrackEntity> tracks);
}

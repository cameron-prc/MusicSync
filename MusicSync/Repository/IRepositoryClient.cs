using System.Collections.Generic;
using System.Threading.Tasks;
using MusicSync.Common;
using MusicSync.RemoteServices;

namespace MusicSync.Repository
{
    public interface IRepositoryClient
    {
        public Task<PlaylistEntity?> GetPlaylist(string name);
        public Task<PlaylistEntity> CreatePlaylist(string name);
        public Task AddToPlaylist(string playlistId, IEnumerable<TrackEntity> tracks);
        Task CreateTracks(List<TrackEntity> tracks, IRemoteService.Type remoteType);
        Task SetRemoteId(TrackEntity track, IRemoteService.Type type);
    }
}

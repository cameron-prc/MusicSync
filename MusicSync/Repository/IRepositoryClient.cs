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
        Task CreateTracks(List<TrackEntity> tracks, IRemoteService.ServiceType remoteServiceType);
        Task CreateArtists(List<ArtistEntity> artists);
        Task SetRemoteId(TrackEntity track, IRemoteService.ServiceType serviceType);
        Task<IEnumerable<ArtistEntity>> FetchArtists(IEnumerable<string> artistIds);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MusicSync.Common;

namespace MusicSync.RemoteServices;

public interface IRemoteArtistService : IRemoteService
{
    public Task AddArtist(RemoteArtist artist);
    public Task<IEnumerable<RemoteArtist>> GetArtists();
    public Task<RemoteArtist?> SearchArtists(ArtistEntity artist);
}

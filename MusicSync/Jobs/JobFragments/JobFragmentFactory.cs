using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class JobFragmentFactory
{
    private readonly IRepositoryClient _repositoryClient;

    public JobFragmentFactory(IRepositoryClient repositoryClient)
    {
        _repositoryClient = repositoryClient;
    }

    public UpdateLocalPlaylistJobFragment BuildFetchRemotePlaylistJobFragment(IRemotePlaylistService remotePlaylistService, string sourceId, string localPlaylistId)
    {
        return new UpdateLocalPlaylistJobFragment(remotePlaylistService, sourceId, localPlaylistId, _repositoryClient);
    }

    public SearchForRemoteIdsJobFragment BuildUpdateRemoteIdsJobFragment(IRemotePlaylistService remotePlaylistService, string localPlaylistId)
    {
        return new SearchForRemoteIdsJobFragment(remotePlaylistService, localPlaylistId, _repositoryClient);
    }

    public UpdateRemotePlaylistJobFragment BuildUpdateRemotePlaylistJobFragment(IRemotePlaylistService remotePlaylistService, string destinationId, string localPlaylistId)
    {
        return new UpdateRemotePlaylistJobFragment(remotePlaylistService, destinationId, localPlaylistId, _repositoryClient);
    }

    public UpdateLocalArtistsFromRemoteJobFragment BuildUpdateLocalArtistsFromRemoteJobFragment(IRemoteArtistService remoteArtistService)
    {
        return new UpdateLocalArtistsFromRemoteJobFragment(remoteArtistService, _repositoryClient);
    }

    public UpdateRemoteArtistsFromLocal BuildUpdateRemoteArtistsFromLocal(IRemoteArtistService remoteArtistService)
    {
        return new UpdateRemoteArtistsFromLocal(remoteArtistService, _repositoryClient);
    }
}

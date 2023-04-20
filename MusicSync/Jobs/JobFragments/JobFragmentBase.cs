using System.Threading.Tasks;
using MusicSync.Common;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public abstract class JobFragmentBase
{
    protected readonly IRepositoryClient RepositoryClient;

    protected JobFragmentBase(IRepositoryClient repositoryClient)
    {
        RepositoryClient = repositoryClient;
    }

    public abstract Task Run();

    protected async Task<PlaylistEntity> FetchLocal(string localName)
    {
        var localPlaylist = await RepositoryClient.GetPlaylist(localName) ??
                            await RepositoryClient.CreatePlaylist(localName);

        return localPlaylist;
    }
}

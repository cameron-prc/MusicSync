using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MusicSync.Common;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class SearchForRemoteIdsJobFragment : JobFragmentBase
{
    public IRemotePlaylistService RemotePlaylistService { get; }
    public string LocalPlaylistId { get; }

    public SearchForRemoteIdsJobFragment(IRemotePlaylistService remotePlaylistService, string localPlaylistId, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        RemotePlaylistService = remotePlaylistService;
        LocalPlaylistId = localPlaylistId;
    }

    public override async Task Run()
    {
        var localPlaylist = await FetchLocal(LocalPlaylistId);
        var tracksWithoutRemoteId = localPlaylist.Tracks.Where(track => track.GetId(RemotePlaylistService.Type()) == null).ToList();

        foreach (var track in tracksWithoutRemoteId)
        {
            var remoteTrack = await RemotePlaylistService.SearchTracks(track);

            if (remoteTrack != null)
            {
                track.SetRemoteId(RemotePlaylistService.Type(), remoteTrack.RemoteId);
                await RepositoryClient.SetRemoteId(track, RemotePlaylistService.Type());
            }
        }
    }
}

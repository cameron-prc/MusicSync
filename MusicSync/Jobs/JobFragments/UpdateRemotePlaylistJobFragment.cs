using System.Linq;
using System.Threading.Tasks;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class UpdateRemotePlaylistJobFragment : JobFragmentBase
{
    public IRemotePlaylistService RemotePlaylistService { get; }
    public string DestinationId { get; }
    public string LocalPlaylistId { get; }

    public UpdateRemotePlaylistJobFragment(IRemotePlaylistService remotePlaylistService, string destinationId, string localPlaylistId, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        DestinationId = destinationId;
        LocalPlaylistId = localPlaylistId;
        RemotePlaylistService = remotePlaylistService;
    }

    public override async Task Run()
    {
        var localPlaylist = await FetchLocal(LocalPlaylistId);
        var remotePlaylist =  await RemotePlaylistService.GetPlaylist(DestinationId);
        var localTracksWithRemoteId = localPlaylist.Tracks.Where(track => track.GetId(RemotePlaylistService.Type()) != null);
        var remoteIds = remotePlaylist.Select(remoteTrack => remoteTrack.RemoteId);
        var missingTracks = localTracksWithRemoteId
            .Where(entity => !remoteIds.Contains(entity.GetId(RemotePlaylistService.Type())))
            .ToList();

        if (missingTracks.Count > 0)
        {
            await RemotePlaylistService.AddToPlaylist(DestinationId, missingTracks);
        }
    }
}

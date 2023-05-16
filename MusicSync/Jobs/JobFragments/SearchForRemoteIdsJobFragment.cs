using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    public override async Task Run(Job context)
    {
        var localPlaylist = await FetchLocal(LocalPlaylistId);
        var tracksWithoutRemoteId = localPlaylist.Tracks.Where(track => track.GetId(RemotePlaylistService.Type()) == null).ToList();
        var updatedTracksCount = 0;

        if (tracksWithoutRemoteId.Count == 0)
        {
            context.Logger.LogDebug("All tracks have matching {remoteType} id", RemotePlaylistService.Type());
            
            return;
        }
        
        context.Logger.LogInformation("Searching {remotePlaylistServiceType} for {tracksWithoutRemoteIdCount} tracks with missing remoteId", RemotePlaylistService.Type(), tracksWithoutRemoteId.Count);

        foreach (var track in tracksWithoutRemoteId)
        {
            var remoteTrack = await RemotePlaylistService.SearchTracks(track);

            if (remoteTrack != null)
            {
                track.SetRemoteId(RemotePlaylistService.Type(), remoteTrack.RemoteId);
                await RepositoryClient.SetRemoteId(track, RemotePlaylistService.Type());

                updatedTracksCount += 1;
            }
        }

        var remainingTracksWithoutRemoteIdCount = tracksWithoutRemoteId.Count - updatedTracksCount;

        if (remainingTracksWithoutRemoteIdCount > 0)
        {
            context.Logger.LogInformation("Unable to find {RemoteType} id for {remainingTracksWithoutRemoteIdCount} tracks", RemotePlaylistService.Type(), remainingTracksWithoutRemoteIdCount);
        }
        else
        {
            context.Logger.LogInformation("Found {RemoteType} id for all tracks", RemotePlaylistService.Type());
        }
    }
}

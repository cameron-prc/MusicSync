using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class SearchForRemoteIdsJobFragment : JobFragmentBase
{
    public IRemotePlaylistService RemoteService { get; }
    public string LocalPlaylistId { get; }

    public SearchForRemoteIdsJobFragment(IRemotePlaylistService remoteService, string localPlaylistId, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        RemoteService = remoteService;
        LocalPlaylistId = localPlaylistId;
    }

    public override async Task Run(Job context)
    {
        var localPlaylist = await FetchLocal(LocalPlaylistId);
        var tracksWithoutRemoteId = localPlaylist.Tracks.Where(track => track.GetId(RemoteService.Type()) == null).ToList();
        var updatedTracksCount = 0;

        if (tracksWithoutRemoteId.Count == 0)
        {
            context.Logger.LogDebug("All tracks have matching {remoteType} id", RemoteService.Type());
            
            return;
        }
        
        context.Logger.LogInformation("Searching {remotePlaylistServiceType} for {tracksWithoutRemoteIdCount} tracks with missing remoteId", RemoteService.Type(), tracksWithoutRemoteId.Count);

        foreach (var track in tracksWithoutRemoteId)
        {
            var remoteTrack = await RemoteService.SearchTracks(track);

            if (remoteTrack != null)
            {
                track.SetRemoteId(RemoteService.Type(), remoteTrack.RemoteId);

                if (string.IsNullOrWhiteSpace(track.Artist.GetId(RemoteService.Type())) && !string.IsNullOrWhiteSpace(remoteTrack.Artist?.RemoteId))
                {
                    track.Artist.SetRemoteId(RemoteService.Type(), remoteTrack.Artist.RemoteId);
                }
                
                await RepositoryClient.SetRemoteId(track, RemoteService.Type());

                updatedTracksCount += 1;
            }
        }

        var remainingTracksWithoutRemoteIdCount = tracksWithoutRemoteId.Count - updatedTracksCount;

        if (remainingTracksWithoutRemoteIdCount > 0)
        {
            context.Logger.LogInformation("Unable to find {RemoteType} id for {remainingTracksWithoutRemoteIdCount} tracks", RemoteService.Type(), remainingTracksWithoutRemoteIdCount);
        }
        else
        {
            context.Logger.LogInformation("Found {RemoteType} id for all tracks", RemoteService.Type());
        }
    }
}

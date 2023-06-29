using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class SearchForRemoteArtistIdsJobFragment : JobFragmentBase
{
    public IRemoteArtistService RemoteService { get; }

    public SearchForRemoteArtistIdsJobFragment(IRemoteArtistService remoteService, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        RemoteService = remoteService;
    }

    public override async Task Run(Job context)
    {
        var artists = await FetchLocalArtists();
        var artistsWithoutRemoteId = artists.Where(artist => artist.GetId(RemoteService.Type()) == null).ToList();
        var updatedArtistCount = 0;

        if (artistsWithoutRemoteId.Count == 0)
        {
            context.Logger.LogDebug("All artists have matching {remoteType} id", RemoteService.Type());
            
            return;
        }
        
        context.Logger.LogInformation("Searching {remotePlaylistServiceType} for {artistsWithoutRemoteId} artists with missing remoteId", RemoteService.Type(), artistsWithoutRemoteId.Count);

        foreach (var artist in artistsWithoutRemoteId)
        {
            var remoteArtist = await RemoteService.SearchArtists(artist);

            if (remoteArtist != null)
            {
                artist.SetRemoteId(RemoteService.Type(), remoteArtist.RemoteId);

                await RepositoryClient.SetRemoteId(artist, RemoteService.Type());

                updatedArtistCount += 1;
            }
        }

        var remainingArtistsWithoutRemoteIdCount = artistsWithoutRemoteId.Count - updatedArtistCount;

        if (remainingArtistsWithoutRemoteIdCount > 0)
        {
            context.Logger.LogInformation("Unable to find {RemoteType} id for {remainingArtistsWithoutRemoteIdCount} artists", RemoteService.Type(), remainingArtistsWithoutRemoteIdCount);
        }
        else
        {
            context.Logger.LogInformation("Found {RemoteType} id for all artists", RemoteService.Type());
        }
    }
}

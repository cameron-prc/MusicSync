using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class UpdateLocalArtistsFromRemoteJobFragment : JobFragmentBase
{
    private IRemoteArtistService RemoteArtistService { get; }
    private IRepositoryClient RepositoryClient { get; }

    public UpdateLocalArtistsFromRemoteJobFragment(IRemoteArtistService remoteArtistService, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        RemoteArtistService = remoteArtistService;
        RepositoryClient = repositoryClient;
    }

    public override async Task Run(Job context)
    {
        var localArtists = await FetchLocalArtists();
        var remoteArtists = await RemoteArtistService.GetArtists();
        var newRemoteArtists = FindNewArtists(localArtists, remoteArtists);

        if (newRemoteArtists.Count > 0)
        {
            context.Logger.LogInformation("Adding {newTracksCount} new artists from {sourceType}", newRemoteArtists.Count, RemoteArtistService.GetType().ToString());
        }
        
        var newArtists = newRemoteArtists
            .Select(remoteArtist => new ArtistEntity(remoteArtist))
            .ToList();

        await RepositoryClient.CreateArtists(newArtists);
    }
    private List<RemoteArtist> FindNewArtists(IEnumerable<ArtistEntity> localArtists, IEnumerable<RemoteArtist> remoteArtists)
    {
        return remoteArtists
            .Where(remoteArtist =>
                !localArtists.Select(artist => artist.GetId(RemoteArtistService.Type())).Contains(remoteArtist.RemoteId)
            )
            .ToList();
    }
}

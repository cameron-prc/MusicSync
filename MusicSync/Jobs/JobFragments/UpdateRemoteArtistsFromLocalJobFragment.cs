using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class UpdateRemoteArtistsFromLocal : JobFragmentBase
{
    private IRemoteArtistService RemoteArtistService { get; }
    private IRepositoryClient RepositoryClient { get; }

    public UpdateRemoteArtistsFromLocal(IRemoteArtistService remoteArtistService, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        RemoteArtistService = remoteArtistService;
        RepositoryClient = repositoryClient;
    }

    public override async Task Run(Job context)
    {
        var localArtists = await FetchLocalArtists();
        var remoteArtists = await RemoteArtistService.GetArtists();
        var newArtists = FindNewArtists(localArtists, remoteArtists);

        if (newArtists.Count > 0)
        {
            context.Logger.LogInformation("Adding {newTracksCount} new artists from {sourceType}", newArtists.Count, RemoteArtistService.GetType().ToString());
        }
        
        var newRemoteArtists = newArtists
            .Select(artist => new RemoteArtist(RemoteArtistService.Type(), artist.Id, artist.Name))
            .ToList();

        foreach (var remoteArtist in newRemoteArtists)
        {
            await RemoteArtistService.AddArtist(remoteArtist);
        }
    }
    private List<ArtistEntity> FindNewArtists(IEnumerable<ArtistEntity> localArtists, IEnumerable<RemoteArtist> remoteArtists)
    {
        return localArtists
            .Where(localArtist => !remoteArtists.Select(artist => artist.RemoteId).Contains(localArtist.GetId(RemoteArtistService.Type()))
            )
            .ToList();
    }
}

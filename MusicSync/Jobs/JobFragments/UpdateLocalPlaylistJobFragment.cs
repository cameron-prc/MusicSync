using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Common;
using MusicSync.RemoteServices;
using MusicSync.Repository;

namespace MusicSync.Jobs.JobFragments;

public class UpdateLocalPlaylistJobFragment : JobFragmentBase
{
    private IRemotePlaylistService RemotePlaylistService { get; }
    private string SourceId { get; }
    private string LocalPlaylistId { get; }

    public UpdateLocalPlaylistJobFragment(IRemotePlaylistService remotePlaylistService, string sourceId, string localPlaylistId, IRepositoryClient repositoryClient) : base(repositoryClient)
    {
        SourceId = sourceId;
        LocalPlaylistId = localPlaylistId;
        RemotePlaylistService = remotePlaylistService;
    }

    public override async Task Run(Job context)
    {
        var local = await FetchLocal(LocalPlaylistId);
        var remote = await FetchRemotePlaylist(SourceId);
        var newRemoteTracks = FindNewTracks(local, remote);

        if (newRemoteTracks.Count > 0)
        {
            context.Logger.LogInformation("Adding {newTracksCount} new tracks from {sourceType} playlist: '{playlistId}'", newRemoteTracks.Count, RemotePlaylistService.GetType().ToString(), SourceId);
        }
        
        var artistIds = newRemoteTracks.Select(track => track.Artist?.RemoteId);
        
        var artists = await RepositoryClient.FetchArtists(artistIds);
        var newArtists = newRemoteTracks
            .Select(track => track.Artist)
            .Where(remoteArtist => artists.All(artist => artist.GetId(RemotePlaylistService.Type()) != remoteArtist.RemoteId))
            .Select(remoteArtist => new ArtistEntity(remoteArtist!))
            .ToList();

        await RepositoryClient.CreateArtists(newArtists);

        artists = artists.Concat(newArtists).ToList();

        var newTracks = new List<TrackEntity>();

        foreach (var newTrack in newRemoteTracks)
        {
            var artist = artists.First(x => x.GetId(RemotePlaylistService.Type()) == newTrack.Artist?.RemoteId);
            var track = new TrackEntity(newTrack, artist);

            newTracks.Add(track);
        }

        await RepositoryClient.CreateTracks(newTracks, RemotePlaylistService.Type());
        await RepositoryClient.AddToPlaylist(local.Id, newTracks);
    }

    private async Task<RemotePlaylist> FetchRemotePlaylist(string remoteId)
    {
        return await RemotePlaylistService.GetPlaylist(remoteId);
    }

    private List<RemoteTrack> FindNewTracks(PlaylistEntity localPlaylistEntity, RemotePlaylist remotePlaylist)
    {
        return remotePlaylist
            .Tracks
            .Where(remoteTrack =>
                !localPlaylistEntity.Tracks.Select(track => track.GetId(RemotePlaylistService.Type())).Contains(remoteTrack.RemoteId)
            )
            .ToList();
    }
}

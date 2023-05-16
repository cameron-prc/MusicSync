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
        var newTracks = FindNewTracks(local, remote);
        
        if (newTracks.Count > 0)
        {
            context.Logger.LogInformation("Adding {newTracksCount} new tracks from {sourceType} playlist: '{playlistId}'", newTracks.Count, remote.GetType().ToString(), SourceId);
        }

        await RepositoryClient.CreateTracks(newTracks, RemotePlaylistService.Type());
        await RepositoryClient.AddToPlaylist(local.Id, newTracks);
    }

    private async Task<IEnumerable<RemoteTrack>> FetchRemotePlaylist(string remoteId)
    {
        return await RemotePlaylistService.GetPlaylist(remoteId);
    }

    private List<TrackEntity> FindNewTracks(PlaylistEntity localPlaylistEntity, IEnumerable<RemoteTrack> remotePlaylist)
    {
        return remotePlaylist
            .Where(remoteTrack =>
                !localPlaylistEntity.Tracks.Select(track => track.GetId(RemotePlaylistService.Type())).Contains(remoteTrack.RemoteId)
            )
            .Select(remoteTrack => new TrackEntity(remoteTrack))
            .ToList();
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public override async Task Run()
    {
        var local = await FetchLocal(LocalPlaylistId);
        var remote = await FetchRemotePlaylist(SourceId);

        await SavePlaylist(local, remote);
    }

    private async Task<IEnumerable<RemoteTrack>> FetchRemotePlaylist(string remoteId)
    {
        return await RemotePlaylistService.GetPlaylist(remoteId);
    }

    private async Task SavePlaylist(PlaylistEntity localPlaylistEntity, IEnumerable<RemoteTrack> remotePlaylist)
    {
        var newTracks = remotePlaylist
            .Where(remoteTrack =>
                !localPlaylistEntity.Tracks.Select(track => track.GetId(RemotePlaylistService.Type())).Contains(remoteTrack.RemoteId)
            )
            .Select(remoteTrack => new TrackEntity(remoteTrack))
            .ToList();

        await RepositoryClient.CreateTracks(newTracks, RemotePlaylistService.Type());
        await RepositoryClient.AddToPlaylist(localPlaylistEntity.Id, newTracks);
    }
}

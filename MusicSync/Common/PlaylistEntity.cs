using System.Collections.Generic;
using System.Linq;

namespace MusicSync.Common;

public class PlaylistEntity
{
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public IList<TrackEntity> Tracks { get; set; } = new List<TrackEntity>();

    public void Add(IEnumerable<TrackEntity> newTracks)
    {
        Tracks = Tracks.Concat(newTracks).ToList();
    }
}
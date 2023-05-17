using System.Collections.Generic;

namespace MusicSync.RemoteServices;

public class RemotePlaylist
{
    public IRemoteService.ServiceType ServiceType;
    public string Id;
    public string Name;
    private IEnumerable<RemoteTrack> Tracks;
}

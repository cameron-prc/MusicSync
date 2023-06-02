namespace MusicSync.RemoteServices;

public class RemoteArtist
{
    public IRemoteService.ServiceType RemoteServiceType { get; }
    public string RemoteId { get; }
    public string Name { get; }

    public RemoteArtist(IRemoteService.ServiceType remoteServiceType, string remoteId, string name)
    {
        RemoteServiceType = remoteServiceType;
        RemoteId = remoteId;
        Name = name;
    }
}
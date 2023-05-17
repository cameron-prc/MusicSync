namespace MusicSync.RemoteServices;

public interface IRemoteService
{
    public ServiceType Type();

    public enum ServiceType
    {
        YouTube,
        Spotify
    }
}

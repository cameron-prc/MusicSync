namespace MusicSync.RemoteServices;

public class RemoteTrack
{
    public IRemoteService.ServiceType RemoteServiceType { get; init; }
    public string RemoteId { get; init; } = null!;
    public string? ArtistName { get; init; }
    public string? TrackName { get; init; }
}
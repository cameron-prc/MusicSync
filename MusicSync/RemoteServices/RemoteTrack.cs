namespace MusicSync.RemoteServices;

public class RemoteTrack
{
    public IRemoteService.Type RemoteType { get; init; }
    public string RemoteId { get; init; } = null!;
    public string? ArtistName { get; init; }
    public string? TrackName { get; init; }
}
using System.Text.Json.Serialization;

namespace MusicSync.RemoteServices.Lidarr.Responses;

public record GetArtistsResponse
{
    [JsonPropertyName("artistName")]
    private string ArtistName;
    
    [JsonPropertyName("mbId")]
    private string MusicBrainzId;

    public RemoteArtist ToRemoteArtist()
    {
        return new RemoteArtist(IRemoteService.ServiceType.Lidarr, MusicBrainzId, ArtistName);
    }
}
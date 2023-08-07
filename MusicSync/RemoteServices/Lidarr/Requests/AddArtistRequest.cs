using System.Text.Json.Serialization;

namespace MusicSync.RemoteServices.Lidarr.Requests;

public record AddArtistRequest
{
    [JsonPropertyName("rootFolderPath")]
    public string RootFolderPath { get; set; }

    [JsonPropertyName("monitor")]
    public string Monitor { get; set; }

    [JsonPropertyName("foreignArtistId")]
    public string ForeignArtistId { get; set; }

    [JsonPropertyName("monitorNewItems")]
    public string MonitorNewItems { get; set; }

    [JsonPropertyName("qualityProfileId")]
    public string QualityProfileId { get; set; }

    [JsonPropertyName("metadataProfileId")]
    public string MetadataProfileId { get; set; }

    [JsonPropertyName("tags")]
    public string Tags { get; set; }

    [JsonPropertyName("searchForMissingAlbums")]
    public string SearchForMissingAlbums { get; set; }
}

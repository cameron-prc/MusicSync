using System.Text.Json.Serialization;

namespace MusicSync.RemoteServices.Lidarr.Requests;

public record AddArtistRequest
{
    [JsonPropertyName("rootFolderPath")]
    private string RootFolderPath { get; set; }

    [JsonPropertyName("monitor")]
    private string Monitor { get; set; }

    [JsonPropertyName("foreignArtistId")]
    private string ForeignArtistId { get; set; }

    [JsonPropertyName("monitorNewItems")]
    private string MonitorNewItems { get; set; }

    [JsonPropertyName("qualityProfileId")]
    private string QualityProfileId { get; set; }

    [JsonPropertyName("metadataProfileId")]
    private string MetadataProfileId { get; set; }

    [JsonPropertyName("tags")]
    private string Tags { get; set; }

    [JsonPropertyName("searchForMissingAlbums")]
    private string SearchForMissingAlbums { get; set; }
}

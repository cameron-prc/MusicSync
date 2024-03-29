using MusicSync.RemoteServices.Lidarr.Requests;

namespace MusicSync.RemoteServices.Lidarr;

public static class Helper
{
    public static AddArtistRequest ToRequestModel(this RemoteArtist artist)
    {
        return new AddArtistRequest
        {
            RootFolderPath = "",
            ForeignArtistId = artist.RemoteId,
            MetadataProfileId = "",
            Monitor = "true",
            MonitorNewItems = "true",
            QualityProfileId = "",
            SearchForMissingAlbums = "false",
            Tags = ""
        };
    }
}

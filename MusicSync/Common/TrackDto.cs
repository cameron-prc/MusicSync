namespace MusicSync.Common;

public record TrackDto
{
    public string? Title { get; init; }
    public string? ArtistId { get; init; }
    public string? YoutubeId { get; init; }
    public string? SpotifyId { get; init; }
    public string Id { get; init; } = null!;

    public TrackDto() {}

    public TrackDto(TrackEntity entity)
    {
        Id = entity.Id;
        Title = entity.Title;
        ArtistId = entity.Artist?.Id;
        YoutubeId = entity.YoutubeId;
        SpotifyId = entity.SpotifyId;
    }
}

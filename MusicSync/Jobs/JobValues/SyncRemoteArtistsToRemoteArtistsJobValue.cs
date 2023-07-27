namespace MusicSync.Jobs.JobValues;

public record SyncRemoteArtistsToRemote(string Name, string SourceType, string DestinationType) : JobValueBase(Name);

namespace MusicSync.Jobs.JobValues;

public record SyncRemoteToRemoteJobValue(string Name, string Id, string SourceType, string SourceId,
    string DestinationType, string DestinationId) : JobValueBase(Name);

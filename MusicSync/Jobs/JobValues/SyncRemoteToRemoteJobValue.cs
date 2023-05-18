namespace MusicSync.Jobs.JobValues;

public record SyncRemoteToRemoteJobValue(string Id, string SourceType, string SourceId,
    string DestinationType, string DestinationId);
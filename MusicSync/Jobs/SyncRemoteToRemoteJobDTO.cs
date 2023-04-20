using MusicSync.RemoteServices;

namespace MusicSync.Jobs;

public record SyncRemoteToRemoteJobDto(string Id, string SourceType, string SourceId,
    string DestinationType, string DestinationId);
namespace MusicSync.Jobs.JobValues;

public record SyncRemoteToLocalJobValue(string LocalPlaylistName, string SourceType, string SourceId);

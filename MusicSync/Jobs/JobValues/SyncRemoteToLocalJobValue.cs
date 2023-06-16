namespace MusicSync.Jobs.JobValues;

public record SyncRemoteToLocalJobValue(string Name, string LocalPlaylistName, string SourceType, string SourceId) : JobValueBase(Name);

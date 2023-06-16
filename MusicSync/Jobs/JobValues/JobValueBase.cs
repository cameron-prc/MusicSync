using System.Text.Json.Serialization;

namespace MusicSync.Jobs.JobValues;

[JsonDerivedType(typeof(JobValueBase), typeDiscriminator: "base")]
[JsonDerivedType(typeof(SyncRemoteToRemoteJobValue), typeDiscriminator: "SyncRemoteToRemote")]
[JsonDerivedType(typeof(SyncRemoteToLocalJobValue), typeDiscriminator: "SyncRemoteToLocal")]
public record JobValueBase(string Name);

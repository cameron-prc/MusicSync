using System.Text.Json.Serialization;

namespace MusicSync.Jobs.JobValues;

[JsonDerivedType(typeof(JobValueBase), typeDiscriminator: "base")]
[JsonDerivedType(typeof(SyncRemoteToRemoteJobValue), typeDiscriminator: "SyncRemoteToRemote")]
[JsonDerivedType(typeof(SyncRemoteToLocalJobValue), typeDiscriminator: "SyncRemoteToLocal")]
[JsonDerivedType(typeof(SyncRemoteArtistsToRemote), typeDiscriminator: "SyncRemoteArtistsToRemote")]
public record JobValueBase(string Name);

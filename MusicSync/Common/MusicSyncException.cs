using System;

namespace MusicSync.Common;

public class MusicSyncException : Exception
{
    public MusicSyncException() {}

    public MusicSyncException(string message) : base(message) {}

    public MusicSyncException(string message, Exception inner) : base(message, inner) {}
}

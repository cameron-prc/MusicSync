using Microsoft.Data.Sqlite;

namespace MusicSync.Repository;

public interface IDatabase
{
    public SqliteConnection GetConnection();

    public void Setup();
}
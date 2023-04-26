using System.Threading.Tasks;
using MusicSync.Repository;
using MusicSync.Services;

namespace MusicSync;

public class PlaylistSyncer
{
    private readonly IDatabase _database;
    private readonly IJobPopulator _jobPopulator;

    public PlaylistSyncer(IDatabase database, IJobPopulator jobPopulator)
    {
        _database = database;
        _jobPopulator = jobPopulator;
    }

    public async Task Run(string[] args)
    {
        var jobs = _jobPopulator.Populate();
        _database.Setup();

        foreach (var job in jobs)
        {
            await job.Run();
        }
    }
}

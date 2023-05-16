using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MusicSync.Jobs.JobFragments;

namespace MusicSync.Jobs;

public class Job
{
    private readonly IEnumerable<JobFragmentBase> _jobFragments;

    public Job(IEnumerable<JobFragmentBase> jobFragments, ILogger logger)
    {
        _jobFragments = jobFragments;
        Logger = logger;
    }

    public async Task Run()
    {
        foreach (var fragment in _jobFragments)
        {
            await fragment.Run(this);
        }
    }

    public ILogger Logger { get; }
}

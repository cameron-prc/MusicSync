using System.Collections.Generic;
using System.Threading.Tasks;
using MusicSync.Jobs.JobFragments;

namespace MusicSync.Jobs;

public class Job
{
    private readonly IEnumerable<JobFragmentBase> _jobFragments;

    public Job(IEnumerable<JobFragmentBase> jobFragments)
    {
        _jobFragments = jobFragments;
    }

    public async Task Run()
    {
        foreach (var fragment in _jobFragments)
        {
            await fragment.Run();
        }
    }
}

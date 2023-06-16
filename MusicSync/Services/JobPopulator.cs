using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using MusicSync.Jobs;
using MusicSync.Jobs.JobValues;

namespace MusicSync.Services;

public class JobPopulator : IJobPopulator
{
    private readonly JobFactory _jobFactory;

    public JobPopulator(JobFactory jobFactory)
    {
        _jobFactory = jobFactory;
    }

    public IEnumerable<Job> Populate()
    {
        using var streamReader = new StreamReader("jobs.json");
        var jsonString = streamReader.ReadToEnd();
        var jobValues = JsonSerializer.Deserialize<IEnumerable<JobValueBase>>(jsonString);

        if (jobValues == null)
        {
            throw new Exception();
        }

        return jobValues.Select(jobValue => _jobFactory.BuildJob(jobValue));
    }
}

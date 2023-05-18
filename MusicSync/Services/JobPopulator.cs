using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using MusicSync.Jobs;

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
        var json = JsonSerializer.Deserialize<JsonNode>(jsonString);
        var jobsArray = json?["Jobs"]?.AsArray() ?? new JsonArray();
        var jobs = new List<Job>();

        foreach (var polymorphicJobNode in jobsArray)
        {
            var jobType = polymorphicJobNode?["Type"]?.ToString();
            var jobName = polymorphicJobNode?["Name"]?.ToString();
            var jobData = polymorphicJobNode?["Data"]?.ToString();

            if (jobType == null || jobData == null || string.IsNullOrWhiteSpace(jobName))
            {
                throw new Exception($"Malformed job: {polymorphicJobNode}");
            }

            var jobDto = new JobDto(Type: jobType, Name: jobName, Data: jobData);

            jobs.Add(_jobFactory.BuildJob(jobDto));
        }

        return jobs;
    }
}

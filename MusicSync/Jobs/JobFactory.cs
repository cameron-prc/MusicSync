using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MusicSync.Jobs.JobFragments;
using MusicSync.Jobs.JobValues;
using MusicSync.RemoteServices;
using MusicSync.RemoteServices.Spotify;
using MusicSync.RemoteServices.Youtube;

namespace MusicSync.Jobs;

public class JobFactory
{
    private readonly ISpotifyService _spotifyService;
    private readonly IYoutubeService _youtubeService;
    private readonly JobFragmentFactory _jobFragmentFactory;
    private readonly ILoggerFactory _loggerFactory;

    public JobFactory(ISpotifyService spotifyService, IYoutubeService youtubeService, JobFragmentFactory jobFragmentFactory, ILoggerFactory loggerFactory)
    {
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _jobFragmentFactory = jobFragmentFactory;
        _loggerFactory = loggerFactory;
    }

    public Job BuildJob(JobDto jobDto)
    {
        var jobType = Enum.Parse<JobType>(jobDto.Type);

        return jobType switch
        {
            JobType.SyncRemoteToRemote => BuildSyncRemoteToRemoteJob(jobDto),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Job BuildSyncRemoteToRemoteJob(JobDto jobDto)
    {
        var concreteJobValue = JsonSerializer.Deserialize<SyncRemoteToRemoteJobValue>(jobDto.Data);

        if (concreteJobValue == null)
        {
            throw new Exception();
        }

        return BuildSyncRemoteToRemoteJob(jobDto.Name, concreteJobValue);
    }

    private Job BuildSyncRemoteToRemoteJob(string name, SyncRemoteToRemoteJobValue concreteJobValue)
    {
        var originRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(concreteJobValue.SourceType);
        var destinationRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(concreteJobValue.DestinationType);
        var originRemoteService = GetRemoteService(originRemoteServiceType);
        var destinationRemoteService = GetRemoteService(destinationRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildFetchRemotePlaylistJobFragment(originRemoteService, concreteJobValue.SourceId, concreteJobValue.Id),
            _jobFragmentFactory.BuildUpdateRemoteIdsJobFragment(destinationRemoteService, concreteJobValue.Id),
            _jobFragmentFactory.BuildUpdateRemotePlaylistJobFragment(destinationRemoteService, concreteJobValue.DestinationId, concreteJobValue.Id)
        };

        return new Job(name, jobFragments, _loggerFactory.CreateLogger($"{typeof(Job)}.{name}"));
    }

    private IRemotePlaylistService GetRemoteService(IRemoteService.ServiceType serviceType)
    {
        return serviceType switch
        {
            IRemoteService.ServiceType.YouTube => _youtubeService,
            IRemoteService.ServiceType.Spotify => _spotifyService,
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null)
        };
    }
}

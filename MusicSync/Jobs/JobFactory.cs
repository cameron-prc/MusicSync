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

    public Job BuildJob(JobValueBase jobValue)
    {
        return jobValue switch
        {
            SyncRemoteToRemoteJobValue syncRemoteToRemoteJobValue => BuildSyncRemoteToRemoteJob(syncRemoteToRemoteJobValue),
            SyncRemoteToLocalJobValue syncRemoteToLocalJobValue => BuildSyncRemoteToLocalJob(syncRemoteToLocalJobValue),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Job BuildSyncRemoteToRemoteJob(SyncRemoteToRemoteJobValue jobValue)
    {
        var originRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.SourceType);
        var destinationRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.DestinationType);
        var originRemoteService = GetRemoteService(originRemoteServiceType);
        var destinationRemoteService = GetRemoteService(destinationRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildFetchRemotePlaylistJobFragment(originRemoteService, jobValue.SourceId, jobValue.Id),
            _jobFragmentFactory.BuildUpdateRemoteIdsJobFragment(destinationRemoteService, jobValue.Id),
            _jobFragmentFactory.BuildUpdateRemotePlaylistJobFragment(destinationRemoteService, jobValue.DestinationId, jobValue.Id)
        };

        return new Job(jobValue.Name, jobFragments, _loggerFactory.CreateLogger($"{typeof(Job)}.{jobValue.Name}"));
    }

    private Job BuildSyncRemoteToLocalJob(SyncRemoteToLocalJobValue jobValue)
    {
        var originRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.SourceType);
        var originRemoteService = GetRemoteService(originRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildFetchRemotePlaylistJobFragment(originRemoteService, jobValue.SourceId, jobValue.LocalPlaylistName),
        };

        return new Job(jobValue.Name, jobFragments, _loggerFactory.CreateLogger($"{typeof(Job)}.{jobValue.Name}"));
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

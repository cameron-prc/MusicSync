using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MusicSync.Jobs.JobFragments;
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
        var concreteDto = JsonSerializer.Deserialize<SyncRemoteToRemoteJobDto>(jobDto.Data);

        if (concreteDto == null)
        {
            throw new Exception();
        }

        return BuildSyncRemoteToRemoteJob(concreteDto);
    }

    private Job BuildSyncRemoteToRemoteJob(SyncRemoteToRemoteJobDto dto)
    {
        var originRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(dto.SourceType);
        var destinationRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(dto.DestinationType);
        var originRemoteService = GetRemoteService(originRemoteServiceType);
        var destinationRemoteService = GetRemoteService(destinationRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildFetchRemotePlaylistJobFragment(originRemoteService, dto.SourceId, dto.Id),
            _jobFragmentFactory.BuildUpdateRemoteIdsJobFragment(destinationRemoteService, dto.Id),
            _jobFragmentFactory.BuildUpdateRemotePlaylistJobFragment(destinationRemoteService, dto.DestinationId, dto.Id)
        };

        return new Job(jobFragments, _loggerFactory.CreateLogger(typeof(Job)));
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

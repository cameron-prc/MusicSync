using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MusicSync.Jobs.JobFragments;
using MusicSync.Jobs.JobValues;
using MusicSync.RemoteServices;
using MusicSync.RemoteServices.Lidarr;
using MusicSync.RemoteServices.Spotify;
using MusicSync.RemoteServices.Youtube;

namespace MusicSync.Jobs;

public class JobFactory
{
    private readonly ISpotifyService _spotifyService;
    private readonly IYoutubeService _youtubeService;
    private readonly ILidarrService _lidarrService;
    private readonly JobFragmentFactory _jobFragmentFactory;
    private readonly ILoggerFactory _loggerFactory;

    public JobFactory(ISpotifyService spotifyService, IYoutubeService youtubeService, ILidarrService lidarrService, JobFragmentFactory jobFragmentFactory, ILoggerFactory loggerFactory)
    {
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _lidarrService = lidarrService;
        _jobFragmentFactory = jobFragmentFactory;
        _loggerFactory = loggerFactory;
    }

    public Job BuildJob(JobValueBase jobValue)
    {
        return jobValue switch
        {
            SyncRemoteToRemoteJobValue syncRemoteToRemoteJobValue => BuildSyncRemoteToRemoteJob(syncRemoteToRemoteJobValue),
            SyncRemoteToLocalJobValue syncRemoteToLocalJobValue => BuildSyncRemoteToLocalJob(syncRemoteToLocalJobValue),
            SyncRemoteArtistsToRemote syncRemoteArtistsToRemote => BuildSyncRemoteArtistsToRemote(syncRemoteArtistsToRemote),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Job BuildSyncRemoteToRemoteJob(SyncRemoteToRemoteJobValue jobValue)
    {
        var originRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.SourceType);
        var destinationRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.DestinationType);
        var originRemoteService = GetRemotePlaylistService(originRemoteServiceType);
        var destinationRemoteService = GetRemotePlaylistService(destinationRemoteServiceType);

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
        var originRemoteService = GetRemotePlaylistService(originRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildFetchRemotePlaylistJobFragment(originRemoteService, jobValue.SourceId, jobValue.LocalPlaylistName),
        };

        return new Job(jobValue.Name, jobFragments, _loggerFactory.CreateLogger($"{typeof(Job)}.{jobValue.Name}"));
    }

    private Job BuildSyncRemoteArtistsToRemote(SyncRemoteArtistsToRemote jobValue)
    {
        var originRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.SourceType);
        var originRemoteService = GetRemoteArtistService(originRemoteServiceType);
        var destinationRemoteServiceType = Enum.Parse<IRemoteService.ServiceType>(jobValue.DestinationType);
        var destinationRemoteService = GetRemoteArtistService(destinationRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildUpdateLocalArtistsFromRemoteJobFragment(originRemoteService),
            _jobFragmentFactory.BuildUpdateRemoteArtistsFromLocal(destinationRemoteService),
        };

        return new Job(jobValue.Name, jobFragments, _loggerFactory.CreateLogger($"{typeof(Job)}.{jobValue.Name}"));
    }

    private IRemotePlaylistService GetRemotePlaylistService(IRemoteService.ServiceType serviceType)
    {
        return GetRemoteService<IRemotePlaylistService>(serviceType);
    }

    private IRemoteArtistService GetRemoteArtistService(IRemoteService.ServiceType serviceType)
    {
        return GetRemoteService<IRemoteArtistService>(serviceType);
    }

    private T GetRemoteService<T>(IRemoteService.ServiceType serviceType) where T: class, IRemoteService
    {
        var service = serviceType switch
        {
            IRemoteService.ServiceType.YouTube => _youtubeService as T,
            IRemoteService.ServiceType.Spotify => _spotifyService as T,
            IRemoteService.ServiceType.Lidarr => _lidarrService as T,
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null)
        };

        if (service == null)
        {
            throw new Exception($"Unable to cast '{service}' to service ");
        }

        return service;
    }
}

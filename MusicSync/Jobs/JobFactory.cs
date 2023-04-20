using System;
using System.Text.Json;
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

    public JobFactory(ISpotifyService spotifyService, IYoutubeService youtubeService, JobFragmentFactory jobFragmentFactory)
    {
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _jobFragmentFactory = jobFragmentFactory;
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
        var originRemoteServiceType = Enum.Parse<IRemoteService.Type>(dto.SourceType);
        var destinationRemoteServiceType = Enum.Parse<IRemoteService.Type>(dto.DestinationType);
        var originRemoteService = GetRemoteService(originRemoteServiceType);
        var destinationRemoteService = GetRemoteService(destinationRemoteServiceType);

        var jobFragments = new JobFragmentBase[]
        {
            _jobFragmentFactory.BuildFetchRemotePlaylistJobFragment(originRemoteService, dto.SourceId, dto.Id),
            _jobFragmentFactory.BuildUpdateRemoteIdsJobFragment(destinationRemoteService, dto.Id),
            _jobFragmentFactory.BuildUpdateRemotePlaylistJobFragment(destinationRemoteService, dto.DestinationId, dto.Id)
        };

        return new Job(jobFragments);
    }

    private IRemotePlaylistService GetRemoteService(IRemoteService.Type type)
    {
        return type switch
        {
            IRemoteService.Type.YouTube => _youtubeService,
            IRemoteService.Type.Spotify => _spotifyService,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

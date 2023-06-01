using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicSync.Jobs;
using MusicSync.Jobs.JobFragments;
using MusicSync.RemoteServices.Spotify;
using MusicSync.RemoteServices.Youtube;
using MusicSync.Repository;
using MusicSync.Services;

namespace MusicSync;

class Program
{
    static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();

        using var scope = host.Services.CreateScope();
        var appRoot = scope.ServiceProvider.GetRequiredService<PlaylistSyncer>();

        await appRoot.Run(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();

                configuration
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging(logging => { logging.AddConsole(); })
            .ConfigureServices((_, services) =>
            {
                services.AddTransient<IRepositoryClient, PlaylistRepository>();
                services.AddSingleton<IDatabase, Sqlite>();
                services.AddTransient<PlaylistSyncer>();
                services.AddSingleton<ISpotifyAdapter, SpotifyAdapter>();
                services.AddSingleton<ISpotifyService, SpotifyService>();
                services.AddSingleton<IYoutubeService, YoutubeService>();
                services.AddSingleton<JobFactory>();
                services.AddSingleton<JobFragmentFactory>();
                services.AddTransient<IJobPopulator, JobPopulator>();
            });
    }
}

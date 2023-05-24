using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace MusicSync.RemoteServices.Spotify;

public interface ISpotifyAdapter
{
    Task<SpotifyClient> Client();
}

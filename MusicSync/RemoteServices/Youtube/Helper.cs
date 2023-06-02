using System;
using System.Text.RegularExpressions;
using Google.Apis.YouTube.v3.Data;
using MusicSync.Common;

namespace MusicSync.RemoteServices.Youtube;

public static class Helper
{
    public static RemoteTrack ToRemoteTrack(this PlaylistItem playlistItem)
    {
        var artistName = string.Empty;
        var trackName = string.Empty;
        var originalTitle = playlistItem.Snippet.Title;
        var trimmedTitle = TrimFeaturing(TrimVideoTagAnnotations(originalTitle));
        var artistTitlePair = trimmedTitle.Split(" - ");

        if (artistTitlePair.Length == 2)
        {
            artistName = artistTitlePair[0].Trim();
            trackName = StripSurroundingQuotes(artistTitlePair[1].Trim());
        }

        var artist = new RemoteArtist(IRemoteService.ServiceType.YouTube, playlistItem.Snippet.VideoOwnerChannelId, artistName);

        return new RemoteTrack
        {
            RemoteId = playlistItem.Snippet.ResourceId.VideoId,
            RemoteServiceType = IRemoteService.ServiceType.YouTube,
            TrackName = trackName,
            Artist = artist
        };
    }

    /**
     * Remove the "(Official Video)" - "(Episode X)" styled annotation at the end of the video title
     *
     * This trim is quite broad and may remove legitimate parts of the title
     */
    private static string TrimVideoTagAnnotations(string title)
    {
        return Regex.Replace(title, @"(\(.*\)|\[.*\])", "");
    }
    
    /**
     * Remove the ft. annotations
     *
     * These can appear either at the end of the track title or at the end of the artist name. In both cases
     * everything after "ft." can be discarded.
     */
    private static string TrimFeaturing(string title)
    {
        return Regex.Replace(title, @" ft\..*", "");
    }
    
    /**
     * Removes any quotes surrounding the title
     *
     * Occasionally song titles are quoted with either single or double quotes. I'm yet to see a song name that
     * is formatted like this so it seems safe to Simply call Trim()
     */
    private static string StripSurroundingQuotes(string title)
    {
        return title.Trim('"').Trim('\'');
    }

    public static RemoteTrack ToRemoteTrack(this SearchResult searchResult)
    {
        var artistName = string.Empty;
        var trackName = string.Empty;
        var originalTitle = searchResult.Snippet.Title;
        var trimmedTitle = TrimFeaturing(TrimVideoTagAnnotations(originalTitle));
        var artistTitlePair = trimmedTitle.Split(" - ");

        if (artistTitlePair.Length == 2)
        {
            artistName = artistTitlePair[0].Trim();
            trackName = StripSurroundingQuotes(artistTitlePair[1].Trim());
        }

        var artist = new RemoteArtist(IRemoteService.ServiceType.YouTube, searchResult.Snippet.ChannelId, artistName);

        return new RemoteTrack
        {
            Artist = artist,
            TrackName = trackName,
            RemoteId = searchResult.Id.VideoId,
            RemoteServiceType = IRemoteService.ServiceType.YouTube
        };
    }
}

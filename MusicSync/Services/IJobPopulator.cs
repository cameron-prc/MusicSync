using System.Collections.Generic;
using MusicSync.Jobs;

namespace MusicSync.Services;

public interface IJobPopulator
{
    IEnumerable<Job> Populate();
}
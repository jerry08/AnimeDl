using AnimeDl;
using AnimeDl.Scrapers;

namespace AnimeDl.DemoConsole
{
    internal class Class1
    {
        Class1()
        {
            
        }

        async void TT()
        {
            var client = new AnimeClient(AnimeSites.GogoAnime);

            var animes = await client.SearchAsync("");
            var episodes = await client.GetEpisodesAsync(animes[0]);
            var servers = await client.GetVideoServersAsync(episodes[0]);
            var videos = await client.GetVideosAsync(servers[0]);
        }
    }
}

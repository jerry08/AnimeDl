# AnimeDl
Anime Scraper

## Example: TCP chat server
Here comes the example of the TCP chat server. It handles multiple TCP client
sessions and multicast received message from any session to all ones. Also it
is possible to send admin message directly from the server.

```c#
using AnimeDl;

AnimeScraper scraper = new AnimeScraper(AnimeSites.GogoAnime);

scraper.OnAnimesLoaded += (s, e) =>
{
    var animes = e.Animes;

    Console.WriteLine("Animes count: " + animes.Count);

    //Second (get episodes from specific anime)
    scraper.GetEpisodes(animes[0]);
};

scraper.OnEpisodesLoaded += (s, e) =>
{
    var episodes = e.Episodes;

    Console.WriteLine("Episodes count: " + episodes.Count);

    //Thrid (get video links from specific episode).
    //Can download video from links
    scraper.GetEpisodeLinks(episodes[0]);
};

//Optional (Only gets all genres from )
//scraper.OnGenresLoaded += (s, e) =>
//{
//    var genres = e.Genres;
//
//    Console.WriteLine("Genres count: " + genres.Count);
//};
//scraper.GetAllGenres();

//First (Search anime by name)
scraper.Search("your lie in april");
```

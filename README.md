# AnimeDl

[![Version](https://img.shields.io/nuget/v/AnimeDl.svg)](https://nuget.org/packages/AnimeDl)
[![Downloads](https://img.shields.io/nuget/dt/AnimeDl.svg)](https://nuget.org/packages/AnimeDl)
<a href="https://discord.gg/mhxsSMy2Nf"><img src="https://img.shields.io/badge/Discord-7289DA?style=for-the-badge&logo=discord&logoColor=white"></a>

**AnimeDl** scrapes animes from sites.

<br>
<a href="https://www.buymeacoffee.com/jerry08"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=jerry08&button_colour=FFDD00&font_colour=000000&font_family=Poppins&outline_colour=000000&coffee_colour=ffffff" /></a>
<br>

### 🌟STAR THIS REPOSITORY TO SUPPORT THE DEVELOPER AND ENCOURAGE THE DEVELOPMENT OF THE PROJECT!

<br>

> Please do not attempt to upload AnimeDl or any of it's forks on Playstore or any other Android appstores on the internet. Doing so, may infringe their terms and conditions. This may result to legal action or immediate take-down of the app.

### Official Discord Server

<p align="center">
 <a href="https://discord.gg/mhxsSMy2Nf">
  <img src="https://invidget.switchblade.xyz/mhxsSMy2Nf">
 </a>
</p>

* **Available Anime sources:-**

| SITE                       | STATUS	| DOWNLOADS |
|:--------------------------:|:--------:|:---------:|
| [Gogo](https://gogoanime.dk/)			| WORKING | SOME |
| [Zoro](https://zoro.to)				| WORKING | SOME |
| [9Anime](https://9anime.id)			| NOT WORKING | NO |
| [Tenshi](https://tenshi.moe)			| WORKING | YES |


## Install

- 📦 [NuGet](https://nuget.org/packages/AnimeDl): `dotnet add package AnimeDl`


## Usage

**AnimeDl** exposes its functionality through a single entry point — the `AnimeClient` class.
Create an instance of this class and use the provided operations to send requests.

### Searching

You can execute a search query and get its results by calling `Search(...)` or `SearchAsync(...)`:

```csharp
using AnimeDl;
using AnimeDl.Scrapers;

var client = new AnimeClient(AnimeSites.GogoAnime);

var animes = await client.SearchAsync("naruto");
```

### Anime

#### Retrieving anime metadata

To retrieve the metadata associated with an anime, execute a search query as mentioned above:

```csharp
using AnimeDl;
using AnimeDl.Scrapers;

var client = new AnimeClient(AnimeSites.GogoAnime);

var animes = await client.SearchAsync("naruto");

var title = animes[0].Title;

//More anime details
var animeInfo = await client.GetAnimeInfoAsync(animes[0].Id);
var image = animeInfo.Image;
var summary = animeInfo.Summary;
```

### Episodes

#### Retrieving anime episodes and episode metadata

```csharp
using AnimeDl;
using AnimeDl.Scrapers;

var client = new AnimeClient(AnimeSites.GogoAnime);

var animes = await client.SearchAsync("naruto");
var episodes = await client.GetEpisodesAsync(animes[0].Id);

var description = episodes[0].Description;
var link = episodes[0].Link;
```

### Video Servers

#### Retrieving an episode video servers

```csharp
using AnimeDl;
using AnimeDl.Scrapers;

var client = new AnimeClient(AnimeSites.GogoAnime);

var animes = await client.SearchAsync("naruto");
var episodes = await client.GetEpisodesAsync(animes[0].Id);
var servers = await client.GetVideoServersAsync(episodes[0].Id);
```

### Videos

#### Retrieving video and metadata

```csharp
using AnimeDl;
using AnimeDl.Scrapers;

var client = new AnimeClient(AnimeSites.GogoAnime);

var animes = await client.SearchAsync("naruto");
var episodes = await client.GetEpisodesAsync(animes[0].Id);
var servers = await client.GetVideoServersAsync(episodes[0].Id);
var videos = await client.GetVideosAsync(servers[0]);

var videoUrl = videos[0].VideoUrl;
var headers = videos[0].Headers;
var resolution = videos[0].Resolution;
var format = videos[0].Format;
```

#### Downloading videos

```csharp
using AnimeDl;
using AnimeDl.Scrapers;

var client = new AnimeClient(AnimeSites.GogoAnime);

var animes = await client.SearchAsync("naruto");
var episodes = await client.GetEpisodesAsync(animes[0].Id);
var servers = await client.GetVideoServersAsync(episodes[0].Id);
var videos = await client.GetVideosAsync(servers[0]);

//NB: Video format must be `Container`
await client.DownloadAsync(videos[0], fileName);
```
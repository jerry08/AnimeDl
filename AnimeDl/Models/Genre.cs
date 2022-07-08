namespace AnimeDl;

public class Genre
{
    public string Name { get; set; } = default!;

    public string Link { get; set; } = default!;

    public Genre()
    {

    }

    public Genre(string name)
    {
        Name = name;
    }

    public Genre(string name, string link)
    {
        Name = name;
        Link = link;
    }
}
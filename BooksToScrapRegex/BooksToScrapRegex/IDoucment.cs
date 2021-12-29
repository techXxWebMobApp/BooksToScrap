using HtmlAgilityPack;

namespace BooksToScrapRegex
{
    public interface IDoucment
    {
        HtmlDocument GetDocument(string url); 
    }
}

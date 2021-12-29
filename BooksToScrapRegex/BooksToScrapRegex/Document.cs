using HtmlAgilityPack;

namespace BooksToScrapRegex
{
    public class Document : IDoucment
    {
        public Document(){}
        public HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load(url);
            return document; 
        }
    }
}

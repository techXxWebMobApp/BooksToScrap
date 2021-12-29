namespace BooksToScrapRegex
{
    class Program
    {
        static void Main(string[] args)
        {
            Service sertvice = new Service();
            sertvice.Drive();
        }
    }

    public class Driver
    {
        protected string getWebsiteUrl => "https://books.toscrape.com/index.html";
        protected string getCategoryTagPath => "//li/ul/li/a";
        protected string getHrefAttribute => "href";
        protected string getBookLinkTag => "//h3/a";
        protected string getTotalPages => "//form/strong";
        protected string getNextPagePath => "//ul[contains(@class,\"pager\")]/li[@class=\"next\"]/a";
        protected string getTitleXPath => "//h1";
        protected string getdescriptionXPath => "//article[contains(@class,\"product_page\")]/p";
    }

}

using System.Collections.Generic;

namespace BooksToScrapRegex
{
    interface IService
    {
        void Drive();
        string GetCategory();
        string GetCategoryFromUser(Dictionary<int, dynamic> categoryLinks);
        List<string> GetBookLinks(string mainUrl);
        List<string> GetPageLinks(List<string> bookLinks, string mainUrl, string url);
        void GetBookDetailsArray(List<string> urls);
        void GetUserInput(string[,] booksArray);
        void PrintArray(string[,] books);
        void ApplyRegex(string[,] bboks);
        void ApplyRegexOnAllBooks(string[,] books);
        void ExportToCSV(string bookName, List<Book> books); 
     }
}
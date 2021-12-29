using CsvHelper;
using HtmlAgilityPack;
using StopWord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BooksToScrapRegex
{
    class Service : Driver, IService
    {
        readonly string websiteUrl;
        HtmlDocument httmlDocumnet;
        Document document;
        HtmlNodeCollection linkNodes;
        RegexPattren regexpattren;
        MatchCollection matchedDecs;

        public Service()
        {
            regexpattren = new RegexPattren();
            websiteUrl = getWebsiteUrl;
            document = new Document();
        }
        public void Drive()
        {
            var categoryUrl = GetCategory();
            if (!String.IsNullOrEmpty(categoryUrl))
            {
                var bookLinks = GetBookLinks(categoryUrl);
                if (bookLinks.Count == 0)
                    Console.WriteLine("Found {0} Books", bookLinks.Count);                
                else
                {
                    Console.WriteLine("Found {0} Books", bookLinks.Count);
                    GetBookDetailsArray(bookLinks);
                }
            }
            else
                Console.WriteLine("Invalid Category");
        }
        public string GetCategory()
        {
            try
            {
                dynamic expandobject = new System.Dynamic.ExpandoObject();
                var categoryLinks = new Dictionary<int, dynamic>();
                httmlDocumnet = document.GetDocument(websiteUrl);
                linkNodes = httmlDocumnet.DocumentNode.SelectNodes(getCategoryTagPath);
                var baseUri = new Uri(websiteUrl);
                int key = 0;
                foreach (var link in linkNodes)
                {
                    categoryLinks[key] = expandobject;
                    string href = link.Attributes[getHrefAttribute].Value;
                    string categoryName = link.InnerText.Trim();
                    var categoryLink = new Uri(new Uri(baseUri, href).AbsoluteUri);
                    string path = categoryLink.Segments.GetValue(4).ToString();
                    string[] categoryPath = path.Split('_');
                    categoryLinks[key++] = new { categoryPath = categoryPath[0], category = categoryName, categoryLink = categoryLink.ToString() };
                }
                Console.WriteLine("Here are the categories");
                foreach (KeyValuePair<int, dynamic> cat in categoryLinks)
                    Console.WriteLine("{0}", cat.Value.category);
                var userInput = GetCategoryFromUser(categoryLinks);
                if (!String.IsNullOrEmpty(userInput)) return userInput;
                return null;
            }
            catch (Exception)
            {
                return "Invalid Request ";
            }
        }
        public string GetCategoryFromUser(Dictionary<int, dynamic> categoryLinks)
        {
            Console.Write("\nEnter any category : ");
            string selectedCat = Console.ReadLine().ToLower().Replace(" ", "-").Trim();
            string categoryUrl = string.Empty;
            foreach (KeyValuePair<int, dynamic> category in categoryLinks)
            {
                if (selectedCat == category.Value.categoryPath)
                {
                    categoryUrl = category.Value.categoryLink;
                    return categoryUrl;
                }
            }
            return categoryUrl;
        }
        public List<string> GetBookLinks(string mainurl)
        {
            var uri = new Uri(mainurl);
            var noOfLastSegemnt = string.Format("{0}://{1}", uri.Scheme, uri.Authority);
            for (int j = 0; j <= uri.Segments.Length - 1; j++)
            {
                noOfLastSegemnt += uri.Segments[j];
            }
            var bookLinks = new List<string>();
            httmlDocumnet = document.GetDocument(mainurl);
            linkNodes = httmlDocumnet.DocumentNode.SelectNodes(getBookLinkTag);
            var baseUri = new Uri(mainurl);
            foreach (var link in linkNodes)
            {
                string href = link.Attributes[getHrefAttribute].Value;
                bookLinks.Add(new Uri(baseUri, href).AbsoluteUri);
            }
            bookLinks = GetPageLinks(bookLinks, mainurl, noOfLastSegemnt);
            return bookLinks;
        }
        public List<string> GetPageLinks(List<string> bookLinks, string mainUrl, string url)
        {
            string val;
            httmlDocumnet = document.GetDocument(mainUrl);
            string x = getTotalPages;
            int page = Convert.ToInt32(httmlDocumnet.DocumentNode.SelectSingleNode(x).InnerText);
            if (page > 20 && bookLinks.Count != page)
            {
                string href = httmlDocumnet.DocumentNode.SelectSingleNode(getNextPagePath).Attributes[getHrefAttribute].Value;
                val = url + href;
                httmlDocumnet = document.GetDocument(val);
                linkNodes = httmlDocumnet.DocumentNode.SelectNodes(getBookLinkTag);
                var baseUri = new Uri(val);
                foreach (var node in linkNodes)
                {
                    string newHref = node.Attributes[getHrefAttribute].Value;
                    bookLinks.Add(new Uri(baseUri, newHref).AbsoluteUri);
                }
                GetPageLinks(bookLinks, val, url);
            }
            return bookLinks;
        }
        public void GetBookDetailsArray(List<string> urls)
        {
            string[,] booksArray = new string[urls.Count, 2];
            int j, i = 0;
            foreach (var url in urls)
            {
                httmlDocumnet = document.GetDocument(url);
                var titleXPath = getTitleXPath;
                var descriptionXPath = getdescriptionXPath;
                for (j = 0; j < 1; j++)
                {
                    booksArray[i, j] = Regex.Replace(httmlDocumnet.DocumentNode.SelectSingleNode(titleXPath).InnerText, regexpattren.detectSpeacialChar, "").ToLower().Replace("39", "'");
                }
                if (httmlDocumnet.DocumentNode.SelectSingleNode(descriptionXPath) == null)
                    booksArray[i, j] = "No description";
                else
                    booksArray[i++, j] = Regex.Replace(httmlDocumnet.DocumentNode.SelectSingleNode(descriptionXPath).InnerText, regexpattren.detectSpeacialChar, "").Replace("39", "'");
            }
            PrintArray(booksArray);
            GetUserInput(booksArray);
        }
        public void GetUserInput(string[,] booksArray)
        {
            Console.WriteLine("Enter 0 to apply regex on all books and 1 for particular book");
            var userInput = Console.ReadLine();
            if (userInput == "1")
                ApplyRegex(booksArray);
            else if (userInput == "0")
                ApplyRegexOnAllBooks(booksArray);
            else
            {
                Console.WriteLine("Invalid Input");
                GetUserInput(booksArray);
            }
        }
        public void PrintArray(string[,] books)
        {
            for (int i = 0; i < books.GetLength(0); i++)
            {
                Console.WriteLine("**************************************************************************{0}**********************************************************************************", i);
                int j;
                for (j = 0; j < 1; j++)
                {
                    Console.WriteLine("Title : " + books[i, j]);
                }
                Console.WriteLine("Description : " + books[i, j]);
            }
        }
        public void ApplyRegex(string[,] books)
        {
            bool bookFound = false;
            var booksRegex = new List<Book>();
            Console.Write("Enter Book Tittle :");
            string bookName = Console.ReadLine().ToLower().Replace("()", " ");
            for (int i = 0; i < books.GetLength(0); i++)
            {
                if (books[i, 0] == bookName)
                {
                    bookFound = true;
                    var newString = bookName.RemoveStopWords("en");
                    string regReplace = Regex.Replace(newString, regexpattren.detectChar, "").Trim();
                    string duplicatesRemoved = string.Join(" ", regReplace.Split(' ').Distinct());
                    string[] tittleSplit = duplicatesRemoved.Split(' ');
                    string pattern;
                    string desc = books[i, 1];
                    for (int j = 0; j < tittleSplit.Length; j++)
                    {
                        pattern = @"[^.]*" + tittleSplit[j] + "[^.]*\\.";
                        matchedDecs = Regex.Matches(desc, pattern, RegexOptions.IgnoreCase);
                        for (int counnt = 0; counnt < matchedDecs.Count; counnt++)
                        {
                            var book = new Book
                            {
                                Word = tittleSplit[j],
                                Sentence = matchedDecs[counnt].Value
                            };
                            booksRegex.Add(book);
                        }
                    }

                }
            }
            if (!bookFound)
                Console.WriteLine("Book not found!");
            else
            {
                if (booksRegex.Count == 0)
                    Console.WriteLine("Tittle not found in description");
                else
                    ExportToCSV(bookName, booksRegex);
            }
        }
        public void ApplyRegexOnAllBooks(string[,] books)
        {
            var booksRegex = new List<Book>();
            for (int i = 0; i < books.GetLength(0); i++)
            {
                string bookName = books[i, 0].Replace("()", " ");
                var newString = bookName.RemoveStopWords("en");
                string regReplace = Regex.Replace(newString, regexpattren.detectChar, "").Trim();
                string duplicatesRemoved = string.Join(" ", regReplace.Split(' ').Distinct());
                string[] tittleSplit = duplicatesRemoved.Split(' ');
                string pattren;
                string desc = books[i, 1];
                for (int j = 0; j < tittleSplit.Length; j++)
                {
                    pattren = @"[^.]*" + tittleSplit[j] + "[^.]*\\.";
                    matchedDecs = Regex.Matches(desc, pattren, RegexOptions.IgnoreCase);
                    for (int count = 0; count < matchedDecs.Count; count++)
                    {
                        var book = new Book
                        {
                            Word = tittleSplit[j],
                            Sentence = matchedDecs[count].Value
                        };
                        booksRegex.Add(book);
                    }
                }
            }
        }
        public void ExportToCSV(string bookName, List<Book> books)
        {
            using (var writer = new StreamWriter("./" + bookName + ".csv"))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(books);
                Console.WriteLine("Open CSV file nameed : " + "./" + bookName + ".csv");
            }
        }
    }
}

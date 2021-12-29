using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksToScrapRegex
{
    public class RegexPattren
    {
        public string detectSpeacialChar => @"[^0-9a-zA-Z-. ']+";
        public string detectChar => @"[():,#0-9]";
    }
}

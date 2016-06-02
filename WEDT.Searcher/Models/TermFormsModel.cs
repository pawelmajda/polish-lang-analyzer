using Nancy.ViewEngines.Razor.HtmlHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WEDT.PolishLangAnalyzer;

namespace WEDT.Searcher.Models
{
    public class TermFormsModel
    {
        public string Term { get; set; }
        public string Form { get; set; }
        public List<SelectListItem> AvailableForms { get; set; }
    }
}
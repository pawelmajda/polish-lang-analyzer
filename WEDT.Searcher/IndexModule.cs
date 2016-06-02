namespace WEDT.Searcher
{
    using Lucene.Net.Analysis;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using Version = Lucene.Net.Util.Version;
    using Directory = Lucene.Net.Store.Directory;
    using Nancy;
    using Nancy.ModelBinding;
    using System;
    using System.Collections.Generic;
    using WEDT.PolishLangAnalyzer;
    using WEDT.Searcher.Models;
    using System.IO;
    using Lucene.Net.Analysis.Standard;
    using Nancy.ViewEngines.Razor.HtmlHelpers;

    public class IndexModule : NancyModule
    {
        private readonly IRootPathProvider _pathProvider;
        private readonly IPolishStemmer _polishStemmer;

        public IndexModule(IRootPathProvider pathProvider, IPolishStemmer polishStemmer)
        {
            _pathProvider = pathProvider;
            _polishStemmer = polishStemmer;

            Get["/"] = parameters =>
                {
                    return View["Index"];
                };

            Post["/forms"] = p =>
                {
                    var queryModel = this.Bind<QueryModel>();
                    var baseFormsToChoose = GetQueryBaseForms(queryModel.Query);
                    ViewBag.QueryText = queryModel.Query;

                    return View["Forms", baseFormsToChoose];
                };

            Post["/search"] = p =>
                {
                    var model = this.Bind<List<TermFormsModel>>();

                    var query = String.Empty;
                    foreach(var term in model)
                    {
                        if(!String.IsNullOrEmpty(term.Form))
                        {
                            query += term.Form + " ";
                        }
                        else
                        {
                            query += term.Term + " ";
                        }
                        
                    }

                    var viewModel = FindDocuments(query);

                    return View["Search", viewModel];
                };
        }

        private IEnumerable<DocumentModel> FindDocuments(string queryText)
        {
            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(Path.Combine(_pathProvider.GetRootPath(), "Content", "Index")));
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            IndexReader indexReader = IndexReader.Open(directory, false);
            IndexSearcher searcher = new IndexSearcher(indexReader);

            QueryParser parser = new QueryParser(Version.LUCENE_30, "Text", analyzer);
            Query query = parser.Parse(queryText);

            TopDocs result = searcher.Search(query, indexReader.MaxDoc);

            var foundDocuments = new List<DocumentModel>();
            foreach (var hit in result.ScoreDocs)
            {
                var document = searcher.Doc(hit.Doc);
                var documentModel = new DocumentModel()
                {
                    Id = Convert.ToInt32(document.Get("Id")),
                    Text = document.Get("Text")
                };
                foundDocuments.Add(documentModel);
            }

            indexReader.Dispose();
            searcher.Dispose();
            analyzer.Dispose();
            directory.Dispose();

            return foundDocuments;
        }

        private List<TermFormsModel> GetQueryBaseForms(string query)
        {
            var tokenStream = new LowerCaseFilter(new StandardFilter(new StandardTokenizer(Version.LUCENE_30, new StringReader(query))));
            var termAttr = tokenStream.GetAttribute<Lucene.Net.Analysis.Tokenattributes.ITermAttribute>();

            var termsBaseForms = new List<TermFormsModel>();
            while(tokenStream.IncrementToken())
            {
                var termBaseForms = new TermFormsModel()
                {
                    Term = termAttr.Term,
                    AvailableForms = ToSelectList(_polishStemmer.GetWordBaseForms(termAttr.Term))
                };

                termsBaseForms.Add(termBaseForms);
            }

            return termsBaseForms;
        }

        private List<SelectListItem> ToSelectList(List<TermBaseForm> termBaseForms)
        {
            var selectList = new List<SelectListItem>();

            foreach (var termBaseForm in termBaseForms)
            {
                selectList.Add(new SelectListItem(TagToFormName(termBaseForm.Tag) + " od " + termBaseForm.Base, termBaseForm.Base));
            }

            return selectList;
        }

        private string TagToFormName(string tag)
        {
            var tagParts = tag.Trim().Split(':');
            var tagName = String.Empty;

            foreach(var tagPart in tagParts)
            {
                if(tagPart == tagParts[0])
                {
                    tagName += TagToGrammarClass(tagPart);
                }
                else
                {
                    var grammarCategory = TagToGrammarCategory(tagPart);
                    if (!String.IsNullOrEmpty(grammarCategory))
                    {
                        tagName += ", " + grammarCategory;
                    }
                }
            }

            return tagName;
        }

        private string TagToGrammarClass(string tag)
        {
            switch (tag)
            {
                case "subst": case "depr":
                    return "Rzeczownik";
                case "num": case "numcol":
                    return "Liczebnik";
                case "adj": case "adja": case "adjp":
                    return "Przymiotnik";
                case "adv":
                    return "Przysłówek";
                case "ppron12": case "ppron3": case "siebie":
                    return "Zaimek";
                case "fin": case "bedzie": case "agit": case "praet": 
                case "impt": case "imps": case "inf": case "pcon": 
                case "pant": case "ger": case "pact": case "ppas": case "winien":
                    return "Czasownik";
                case "pred":
                    return "Predykatyw";
                case "prep":
                    return "Przyimek";
                case "conj":
                    return "Spójnik";
                case "qub":
                    return "Kublik (partykuło-przysłówek)";
                case "xxs": case "xxx":
                    return "Ciało obce";
                default:
                    return String.Empty;
            }
        }

        private string TagToGrammarCategory(string tag)
        {
            switch (tag)
            {
                case "sg":
                    return "liczba pojedyncza";
                case "pl":
                    return "liczba mnoga";
                case "nom":
                    return "przypadek mianownik";
                case "gen":
                    return "przypadek dopełniacz";
                case "dat":
                    return "przypadek celownik";
                case "acc":
                    return "przypadek biernik";
                case "inst":
                    return "przypadek narzędnik";
                case "loc":
                    return "przypadek miejscownik";
                case "voc":
                    return "przypadek wołacz";
                case "m1":
                case "m2":
                case "m3":
                    return "rodzaj męski";
                case "f":
                    return "rodzaj żeński";
                case "n":
                    return "rodzaj nijaki";
                case "pos":
                    return "stopień równy";
                case "comp":
                    return "stopień wyższy";
                case "sup":
                    return "stopień najwyższy";
                default:
                    return String.Empty;
            }
        }
    }
}
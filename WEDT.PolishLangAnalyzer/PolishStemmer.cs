using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Version = Lucene.Net.Util.Version;

namespace WEDT.PolishLangAnalyzer
{
    public interface IPolishStemmer
    {
        List<TermBaseForm> GetWordBaseForms(string term);
    }

    public class PolishStemmer : IPolishStemmer
    {
        private readonly string _dictionaryPath;
        private readonly string _indexPath;

        public PolishStemmer(string dictionaryPath = "")
        {
            if (String.IsNullOrEmpty(dictionaryPath))
                dictionaryPath = Environment.CurrentDirectory + @"\polimorf-20141120.tab"; // @"\polimorfologik.txt";

            _dictionaryPath = dictionaryPath;
            _indexPath = Path.Combine(Path.GetDirectoryName(_dictionaryPath), "DictIndex");

            InitializeDictionary();
        }

        private void InitializeDictionary()
        {
            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(_indexPath));

            if (IndexReader.IndexExists(directory))
            {
                directory.Dispose();
                return;
            }

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var file = new StreamReader(_dictionaryPath);
            var line = String.Empty;

            using (IndexWriter writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();

                while ((line = file.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        var parts = line.Split('\t');
                        if (parts.Length > 2)
                        {
                            Document termDoc = new Document();
                            termDoc.Add(new Field("Original", parts[0].ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                            termDoc.Add(new Field("Base", parts[1].ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                            termDoc.Add(new Field("Tag", parts[2].ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                            writer.AddDocument(termDoc);
                        }
                    }
                }

                writer.Optimize();
                writer.Dispose();
            }

            file.Close();
            analyzer.Dispose();
            directory.Dispose();
        }

        public List<TermBaseForm> GetWordBaseForms(string term)
        {
            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(_indexPath));
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            IndexReader indexReader = IndexReader.Open(directory, false);
            IndexSearcher searcher = new IndexSearcher(indexReader);

            Query query = new TermQuery(new Term("Original", term));
            TopDocs result = searcher.Search(query, indexReader.MaxDoc);

            var foundDocuments = new List<TermBaseForm>();
            foreach (var hit in result.ScoreDocs)
            {
                var document = searcher.Doc(hit.Doc);
                var documentModel = new TermBaseForm()
                {
                    Original = document.Get("Original"),
                    Base = document.Get("Base"),
                    Tag = document.Get("Tag")
                };

                if (documentModel.Original == term 
                    && !foundDocuments.Any(x => x.Base == documentModel.Base))
                {
                    foundDocuments.Add(documentModel);
                }
            }

            indexReader.Dispose();
            searcher.Dispose();
            analyzer.Dispose();
            directory.Dispose();

            return foundDocuments;
        }
    }
}

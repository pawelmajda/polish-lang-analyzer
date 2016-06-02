using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEDT.PolishLangAnalyzer;
using Xunit;

namespace WEDT.Tests
{
    public class polish_analyzer
    {
        [Fact]
        public void finds_kota()
        {
            //Arrange
            Document doc = new Document();
            doc.Add(new Field("Id", "1", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Text", "Trol jest taki słodki", Field.Store.YES, Field.Index.ANALYZED));
            
            Document doc2 = new Document();
            doc2.Add(new Field("Id", "2", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc2.Add(new Field("Text", "Ala ma kota", Field.Store.YES, Field.Index.ANALYZED));
            
            Document doc3 = new Document();
            doc3.Add(new Field("Id", "3", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc3.Add(new Field("Text", "Test", Field.Store.YES, Field.Index.ANALYZED));

            Document doc4 = new Document();
            doc4.Add(new Field("Id", "4", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc4.Add(new Field("Text", "A kot ma alę", Field.Store.YES, Field.Index.ANALYZED));


            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(System.Environment.CurrentDirectory + @"\Index"));
            Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);
            
            using(IndexWriter writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();
                writer.AddDocument(doc);
                writer.AddDocument(doc2);
                writer.AddDocument(doc3);
                writer.AddDocument(doc4);

                writer.Optimize();
                writer.Dispose();
            }

            //Act
            IndexReader indexReader = IndexReader.Open(directory, false);
            IndexSearcher searcher = new IndexSearcher(indexReader);

            QueryParser parser = new QueryParser(Version.LUCENE_30, "Text", analyzer);
            Query query = parser.Parse("kota");

            TopDocs result = searcher.Search(query, indexReader.MaxDoc);

            //Assert
            bool found = false;
            var hits = result.ScoreDocs;
            foreach(var hit in hits)
            {
                Document foundDoc = searcher.Doc(hit.Doc);
                var docId = foundDoc.Get("Id");
                var docBody = foundDoc.Get("Text");
                if(docId == "2")
                {
                    found = true;
                    break;
                }
            }

            analyzer.Close();
            indexReader.Dispose();
            searcher.Dispose();

            Assert.True(found);
        }

        [Fact]
        public void finds_koty()
        {
            //Arrange
            Document doc = new Document();
            doc.Add(new Field("Id", "1", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Text", "Trol jest taki słodki", Field.Store.YES, Field.Index.ANALYZED));

            Document doc2 = new Document();
            doc2.Add(new Field("Id", "2", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc2.Add(new Field("Text", "Ala lubi kota", Field.Store.YES, Field.Index.ANALYZED));

            Document doc3 = new Document();
            doc3.Add(new Field("Id", "3", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc3.Add(new Field("Text", "Test", Field.Store.YES, Field.Index.ANALYZED));

            Document doc4 = new Document();
            doc4.Add(new Field("Id", "4", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc4.Add(new Field("Text", "A kot ma alę", Field.Store.YES, Field.Index.ANALYZED));


            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(System.Environment.CurrentDirectory + @"\Index"));
            Analyzer analyzer = new PolishAnalyzer(Version.LUCENE_30, new PolishStemmer());

            using (IndexWriter writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();
                writer.AddDocument(doc);
                writer.AddDocument(doc2);
                writer.AddDocument(doc3);
                writer.AddDocument(doc4);

                writer.Optimize();
                writer.Dispose();
            }

            //Act
            IndexReader indexReader = IndexReader.Open(directory, false);
            IndexSearcher searcher = new IndexSearcher(indexReader);

            QueryParser parser = new QueryParser(Version.LUCENE_30, "Text", analyzer);
            Query query = parser.Parse("mam kota");

            TopDocs result = searcher.Search(query, indexReader.MaxDoc);

            //Assert
            bool foundKot = false;
            bool fouundKota = false;
            var hits = result.ScoreDocs;

            foreach (var hit in hits)
            {
                Document foundDoc = searcher.Doc(hit.Doc);
                var docId = foundDoc.Get("Id");
                if (docId == "2")
                {
                    fouundKota = true;
                } 
                else if(docId == "4")
                {
                    foundKot = true;
                }
            }

            analyzer.Close();
            indexReader.Dispose();
            searcher.Dispose();

            Assert.True(foundKot && fouundKota);
        }
    }
}

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Nancy;
using Nancy.Bootstrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WEDT.PolishLangAnalyzer;
using Version = Lucene.Net.Util.Version;

namespace WEDT.Searcher
{
    public class AppStartup : IApplicationStartup
    {
        private readonly IPolishStemmer _polishStemmer;
        private readonly IRootPathProvider _pathProvider;
        public AppStartup(IRootPathProvider pathProvider, IPolishStemmer polishStemmer)
        {
            _pathProvider = pathProvider;
            _polishStemmer = polishStemmer;

            InitializeLuceneIndex();
        }

        public void Initialize(IPipelines pipelines)
        {
        }

        private void InitializeLuceneIndex()
        {
            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(Path.Combine(_pathProvider.GetRootPath(), "Content", "Index")));

            if (IndexReader.IndexExists(directory))
            {
                directory.Dispose();
                return;
            }

            var analyzer = new PolishAnalyzer(Version.LUCENE_30, _polishStemmer);

            using (IndexWriter writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();

                Document doc = new Document();
                doc.Add(new Field("Id", "1", Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("Text", "Trol jest taki słodki", Field.Store.YES, Field.Index.ANALYZED));

                Document doc2 = new Document();
                doc2.Add(new Field("Id", "2", Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc2.Add(new Field("Text", "Ala lubi kota", Field.Store.YES, Field.Index.ANALYZED));

                Document doc3 = new Document();
                doc3.Add(new Field("Id", "3", Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc3.Add(new Field("Text", "Za oknem słychać już wystrzały fajerwerków, czas więc podsumować rok 2014. W moim przypadku był bardzo różnorodny. Sporo czasu przeznaczyłem na aktywny odpoczynek i podróże, ale był też czas na aktywność zawodową. Zrealizowałem 8 na 11 zeszłorocznych celów, także not bad. Zawsze mogło być lepiej i będę do tego dążył, ale w zasadzie to jestem zadowolony z tego jak ten rok wyglądał ;-)", Field.Store.YES, Field.Index.ANALYZED));

                Document doc4 = new Document();
                doc4.Add(new Field("Id", "4", Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc4.Add(new Field("Text", "A kot ma alę", Field.Store.YES, Field.Index.ANALYZED));

                Document doc5 = new Document();
                doc5.Add(new Field("Id", "5", Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc5.Add(new Field("Text", "DevDay to świetna konferencja dla pasjonatów tworzenia oprogramowania, którzy cenią wartościowe porady, chcą się rozwijać oraz lubią spotykać podobnych do siebie. Podobnie jak przed rokiem, pojechałem na nią i zapisałem wskazówki, które zaczerpnąłem z sesji. Niedawno pojawiły się filmiki z konferencji, dlatego też postanowiłem opublikować teraz ten post, mimo że konferencja była już prawie miesiąc temu.", Field.Store.YES, Field.Index.ANALYZED));

                Document doc6 = new Document();
                doc6.Add(new Field("Id", "6", Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc6.Add(new Field("Text", "Miało to też swoje odbicie w czasie przerw między sesjami. Na MTS przerwy były zarezerwowane oczywiście dla partnerów Microsoftu, co by oni też mogli swoje sprzedać. W głównym pomieszczeniu było ustawione kilkanaście stoisk firm partnerskich, na których mogliśmy się dowiedzieć o ich ofercie oraz wziąć udział w konkursie na np. Xboxa oczywiście wcześniej wypełniając formularz z danymi osobowymi do celach promocyjnych. Nawiązując do konkursów w czasie przerw, to była to jedna wielka zmora bo przemiły Pan z przemiłym głosem ciągle zakłócał jakikolwiek spokój nadając do mikrofonu o kolejnych konkursach i wygranych - i gdzie tu czas i miejsce na jakąkolwiek dyskusję z uczestnikami.", Field.Store.YES, Field.Index.ANALYZED));


                writer.AddDocument(doc);
                writer.AddDocument(doc2);
                writer.AddDocument(doc3);
                writer.AddDocument(doc4);

                writer.Optimize();
                writer.Dispose();
            }

            analyzer.Dispose();
            directory.Dispose();
        }
    }
}
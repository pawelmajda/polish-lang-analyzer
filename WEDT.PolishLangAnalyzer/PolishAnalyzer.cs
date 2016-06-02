using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEDT.PolishLangAnalyzer
{
    public class PolishAnalyzer : StandardAnalyzer
    {
        private readonly Version _version;
        private readonly IPolishStemmer _polishStemmer;

        public PolishAnalyzer(Version matchVersion, IPolishStemmer polishStemmer) : base(matchVersion)
        {
            _version = matchVersion;
            _polishStemmer = polishStemmer;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(_version, reader);
            var stream = new StandardFilter(tokenizer);
            var lowerCaseStream = new LowerCaseFilter(stream);
            var morfologikStram = new PolishFilter(lowerCaseStream, _polishStemmer);

            return morfologikStram;
        }
    }
}

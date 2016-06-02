using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEDT.PolishLangAnalyzer
{
    class PolishFilter : TokenFilter
    {
        private readonly ITermAttribute _termAttr;
        private readonly IPositionIncrementAttribute _posIncAttr;
        private readonly IPolishStemmer _polishStemmer;

        private State _current;
        private Queue<string> _words = new Queue<string>();

        public PolishFilter(TokenStream input, IPolishStemmer polishStemmer)
            : base(input)
        {
            _termAttr = AddAttribute<ITermAttribute>();
            _posIncAttr = AddAttribute<IPositionIncrementAttribute>();
            _polishStemmer = polishStemmer;
        }

        public override bool IncrementToken()
        {
            if (_words.Count > 0)
            {
                RestoreState(_current);

                _termAttr.SetTermBuffer(_words.Dequeue());
                _posIncAttr.PositionIncrement = 0;
                return true;
            }

            if (!input.IncrementToken())
            {
                return false;
            }

            if (_termAttr.Term != null)
            {
                var termBaseForms = _polishStemmer.GetWordBaseForms(_termAttr.Term);
                foreach(var baseForm in termBaseForms)
                {
                    if (baseForm.Base != _termAttr.Term)
                    {
                        _words.Enqueue(baseForm.Base);
                    }
                }
            }
            
            _current = CaptureState();
            return true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDictInduction.Console
{
    public class KVComparer: IEqualityComparer<KeyValuePair<Word,Word>>
    {

        public bool Equals(KeyValuePair<Word, Word> x, KeyValuePair<Word, Word> y)
        {
            return ((x.Key.Value == y.Key.Value) && (x.Value.Value == y.Value.Value));
        }

        public int GetHashCode(KeyValuePair<Word, Word> obj)
        {
            return string.Format("{0}-{1}",obj.Key.Value,obj.Value.Value).GetHashCode();
        }
    }
}

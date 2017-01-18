using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDictInduction.Console.SAT
{
    public class WordPair:IEquatable<WordPair>, IEqualityComparer<WordPair>
    {
        public Word WordU;
        public Word WordK;
        public List<SPath> Paths;
        public float Prob;
        public float Polysemy;
        public Boolean HasMissingEdge = false;
        private Dictionary<SLink, bool> _links;

        public WordPair()
        {
            Paths = new List<SPath>();
        }
        public WordPair(Word wordU, Word wordK)
        {
            WordU = wordU;
            WordK = wordK;
            Paths = new List<SPath>();
        }

        public bool Equals(WordPair other)
        {
            return WordU == other.WordU && WordK == other.WordK;
        }

        public override string ToString()
        {
            return string.Format("u[{0}] k[{1}]",WordU.Value,WordK.Value);
        }

        public bool NeedsLink(SLink link)
        {
            if (_links == null || _links.Count == 0)
            {
                _links = new Dictionary<SLink, bool>();
                foreach (var item in Paths)
                {
                    _links.Add(item.LinkCU, item.LinkCU.Exists);
                    _links.Add(item.LinkCK, item.LinkCK.Exists);
                }
            }
            return _links.ContainsKey(link);
        }

        public  bool Equals(WordPair x, WordPair y)
        {
            return x.WordU == y.WordU && x.WordK == y.WordK;
        }

        public override int GetHashCode()
        {
            return WordU.GetHashCode() ^ WordK.GetHashCode();
            //int hash1 = WordU.GetHashCode();
            //int hash2 = WordK.GetHashCode();
            //int finalhash = WordU.GetHashCode() ^ WordK.GetHashCode();
            //return finalHash != 0 ? finalHash : hash1;
        }

        public  int GetHashCode(WordPair obj)
        {
            int hash1 = WordU .GetHashCode();
            int hash2 = WordK.GetHashCode();
            int finalHash = hash1 ^ hash2;
            return finalHash != 0 ? finalHash : hash1;
        }
    }
}

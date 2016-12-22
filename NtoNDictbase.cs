using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HDictInduction.Console
{
    [Serializable]
    class NtoNDictbase
    {
        private SortedDictionary<string, List<string>> _AtoBdataset = null;
        private SortedDictionary<string, List<string>> _BtoAdataset = null;

        public SortedDictionary<string, List<string>> AtoBdataset
        {
            get{return _AtoBdataset;}
        }

        public SortedDictionary<string, List<string>> BtoAdataset
        {
            get { return _BtoAdataset; }
        }

        public NtoNDictbase()
        {
            this._AtoBdataset = new SortedDictionary<string, List<string>>();
            this._BtoAdataset = new SortedDictionary<string, List<string>>();
        }
    }


    class CUDictbase
    {
        private Dictionary<int, int[]> _CtoUdataset = null;
        private Dictionary<int, int[]> _UtoCdataset = null;

        public Dictionary<int, int[]> CtoU
        {
            get { return _CtoUdataset; }
        }

        public Dictionary<int, int[]> UtoC
        {
            get { return _UtoCdataset; }
        }

        public CUDictbase()
        {
            this._CtoUdataset = new Dictionary<int, int[]>();
            this._UtoCdataset = new Dictionary<int, int[]>();
        }
    }

    class CKDictbase
    {
        private Dictionary<int, int[]> _CtoKdataset = null;
        private Dictionary<int, int[]> _KtoCdataset = null;

        public Dictionary<int, int[]> CtoK
        {
            get { return _CtoKdataset; }
        }

        public Dictionary<int, int[]> KtoC
        {
            get { return _KtoCdataset; }
        }

        public CKDictbase()
        {
            this._CtoKdataset = new Dictionary<int, int[]>();
            this._KtoCdataset = new Dictionary<int, int[]>();
        }
    }

    class DictBase {

        private string[] _CWords = null;
        private string[] _UWords = null;
        private string[] _KWords = null;

        private CUDictbase _CUDictbase = null;
        private CKDictbase _CKDictbase = null;

        public CUDictbase CUDictbase;
        //{
        //    get { return _CUDictbase; }
        //    set { this._CUDictbase = value; }
        //}
        public CKDictbase CKDictbase;
        //{
        //    //get { return _CKDictbase; }
        //    //set { this._CKDictbase = value; }
        //}

        public string[] CWords
        {
            get { return _CWords; }
            set { this._CWords = value; }
        }

        public string[] UWords
        {
            get { return _UWords; }
            set { this._UWords = value; }
        }

        public string[] KWords
        {
            get { return _KWords; }
            set { this._KWords = value; }
        }

        public DictBase()
        {
            CKDictbase = new CKDictbase();
            CUDictbase = new CUDictbase();
        }

        public Word GetUWordByID(int id)
        {
            return new Word(id, UWords[id], Language.Uyghur);
        }

        public Word GetKWordByID(int id)
        {
            return new Word(id, KWords[id], Language.Kazak);
        }

        public Word GetCWordByID(int id)
        {
            return new Word(id, CWords[id], Language.Chinese);
        }

        public IEnumerable<Word> GetCWordByIDs(IList<int> ids)
        {
            foreach (int item in ids)
            {
                yield return new Word(item, CWords[item], Language.Chinese);
            }
        }


        public IEnumerable<Word> GetUWordByIDs(IList<int> ids)
        {
            foreach (int item in ids)
            {
                yield return new Word(item, UWords[item], Language.Uyghur);
            }
        }

        public IEnumerable<Word> GetKWordByIDs(IList<int> ids)
        {
            foreach (int item in ids)
            {
                yield return new Word(item, KWords[item], Language.Kazak);
            }
        }
    }

    public class Word
    {
        public int ID;
        public string Value;
        public Language Language;

        public Word(int id, string value, Language lang)
        {
            this.ID = id;
            this.Value = value;
            Language = lang;
        }

        public override string ToString()
        {
            if (GlobalStore.Latin)
            {
                if (Language == HDictInduction.Console.Language.Uyghur)
                    return DBHelper.Syn.getUKYFromUy(Value);
                else if (Language == HDictInduction.Console.Language.Kazak)
                    return DBHelper.Syn.getLtFromKz(Value);
                else
                    return Value;
            }
            return Value;
        }

        public static bool operator == (Word lhs, Word rhs)
        {
            //return lhs.Equals(rhs);
            return (lhs.ID == rhs.ID && lhs.Language == rhs.Language);
        }

        public static bool operator !=(Word lhs, Word rhs)
        {
            return !(lhs.Equals(rhs));
        }

        public override bool Equals(object obj)
        {
            return ID == (obj as Word).ID && Language == (obj as Word).Language;
        }

        public override int GetHashCode()
        {

            return ID.GetHashCode()^ Language.GetHashCode();
            //return finalHash != 0 ? finalHash : hash1;
        }
    }
}

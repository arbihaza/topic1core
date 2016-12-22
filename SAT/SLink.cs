﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDictInduction.Console.SAT
{
    public class SLink: IEquatable<SLink>, IEqualityComparer<SLink>
    {
        public Word WordPivot;
        public Word WordNonPivot;
        public bool Exists;
        public float Pr;

        public SLink(Word wordPivot, Word wordx)
        {
            WordPivot = wordPivot;
            WordNonPivot = wordx;
        }

        public bool Equals(SLink other)
        {
            return WordPivot == other.WordPivot && WordNonPivot == other.WordNonPivot;
        }

        public override bool Equals(object obj)
        {
            return WordPivot == (obj as SLink).WordPivot && WordNonPivot == (obj as SLink).WordNonPivot;
        }

        public override string ToString()
        {
            return string.Format("[{0}] C[{1}] X[{2}]",Exists, WordPivot.Value, WordNonPivot.Value);
        }

        public bool Equals(SLink x, SLink y)
        {
            throw new Exception();
            return x.WordPivot.Equals(y.WordPivot) && x.WordNonPivot.Equals(y.WordNonPivot);
        }

        public int GetHashCode(SLink obj)
        {
            throw new Exception();
            int hash1 = obj.WordPivot.GetHashCode();
            int hash2 = obj.WordNonPivot.GetHashCode();
            int finalHash = hash1 ^ hash2;
            return finalHash != 0 ? finalHash : hash1;
        }

        public override int GetHashCode()
        {
            return WordPivot.GetHashCode() ^ WordNonPivot.GetHashCode();
            //int hash1 = WordPivot.GetHashCode();
            //int hash2 = WordNonPivot.GetHashCode();
            //int finalHash = hash1 ^ hash2;
            //return finalHash != 0 ? finalHash : hash1;
        }
    }
}
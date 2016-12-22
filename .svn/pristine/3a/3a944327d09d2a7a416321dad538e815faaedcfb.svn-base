using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDictInduction.Console
{
    public class Accesories
    {

        public float CalculateSimilartyThreshHold()
        {

            string[] uWords = (from u in DBHelper.MyDictBase.UWords
                 let uky = DBHelper.Syn.getUKYFromUy(u)
                 select uky).ToArray();

            string[] kWords = (from k in DBHelper.MyDictBase.KWords
                             let uky = DBHelper.Syn.getLtFromKz(k)
                             select uky).ToArray();


            var maxAverage = (from u in uWords
                          let maxSim = kWords.Max(t => DBHelper.iLD(u, t))
                          select maxSim).Average();

            return maxAverage;
        }

    }
}

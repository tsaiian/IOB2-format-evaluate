using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IOB2_format_evaluate
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader(args[0]);

            List<string> harmonicMeanType = new List<string>();
            for (int i = 1; i < args.Length; i++)
                harmonicMeanType.Add(args[i]);

            Dictionary<Tuple<int, int>, string> answerList = new Dictionary<Tuple<int, int>, string>();
            Dictionary<Tuple<int, int>, string> predictList = new Dictionary<Tuple<int, int>, string>();
            HashSet<string> type = new HashSet<string>();


            int AccSameCount = 0;
            int AccAllCount = 0;

            string nowStatAns = String.Empty, nowStatPre = String.Empty;
            int beginPositionAns = -1, beginPositionPre = -1;

            int instanceLevelTotal = 0, instanceLevelCorrect = 0;
            bool stillCorrect = true;
            for (int loop = 1; !sr.EndOfStream; loop++ )
            {
                string line = sr.ReadLine();

                if (line.Equals(String.Empty) )
                {
                    if (beginPositionAns != -1)
                    {
                        instanceLevelTotal++;
                        if (stillCorrect)
                            instanceLevelCorrect++;

                        answerList.Add(new Tuple<int, int>(beginPositionAns, loop - 1), nowStatAns);
                        answerList.Add(new Tuple<int, int>(beginPositionAns, -1), nowStatAns);
                        answerList.Add(new Tuple<int, int>(-1, loop - 1), nowStatAns);

                        
                    }
                    if (beginPositionPre != -1)
                        predictList.Add(new Tuple<int, int>(beginPositionPre, loop - 1), nowStatPre);

                    //restart
                    stillCorrect = true;
                    beginPositionAns = -1;
                    beginPositionPre = -1;
                    nowStatAns = String.Empty;
                    nowStatPre = String.Empty;

                    continue;
                }

                string[] tokens = line.Split('\t');

                string predict = ExtractType(tokens[tokens.Length - 1]);
                string answer = ExtractType(tokens[tokens.Length - 2]);

                type.Add(predict);
                type.Add(answer);

                //ans
                if (!nowStatAns.Equals(answer) && !nowStatAns.Equals(String.Empty))
                {
                    answerList.Add(new Tuple<int, int>(beginPositionAns, loop - 1), nowStatAns);
                    answerList.Add(new Tuple<int, int>(beginPositionAns, -1), nowStatAns);
                    answerList.Add(new Tuple<int, int>(-1, loop - 1), nowStatAns);
                }
                if (!nowStatAns.Equals(answer) || nowStatAns.Equals(String.Empty))
                    beginPositionAns = loop;

                nowStatAns = answer;

                //predict
                if (!nowStatPre.Equals(answer) && !nowStatPre.Equals(String.Empty))
                    predictList.Add(new Tuple<int, int>(beginPositionPre, loop - 1), nowStatPre);
                if (!nowStatPre.Equals(answer) || nowStatPre.Equals(String.Empty))
                    beginPositionPre = loop;

                nowStatPre = predict;


                if (predict.Equals(answer))
                    AccSameCount++;
                else
                    stillCorrect = false;
                AccAllCount++;

            }

            Console.WriteLine("                                                                                         number(precision/recall/f-score) ");
            Console.WriteLine("+---------------+-----------------------------------+-----------------------------------+-----------------------------------+");
            Console.WriteLine("|\t\t\t\t|\t\t\tcomplete match\t\t\t|\t\tleft boundary match\t\t\t|\t\tright boundary match\t\t|");
            Console.WriteLine("+---------------+-----------------------------------+-----------------------------------+-----------------------------------+");

            int exactMatchTotal = 0;
            int leftMatchTotal = 0;
            int rightMatchTotal = 0;
            double p, r, f;
            string block;
            double eP = 0, eR = 0, eF = 0;
            double lP = 0, lR = 0, lF = 0;
            double rP = 0, rR = 0, rF = 0;

            foreach (string t in type)
            {
                int count_ans = 0;
                int count_pre = 0;
                int tp = 0, left_tp = 0, right_tp = 0;

                foreach (KeyValuePair<Tuple<int, int>, string> l in predictList)
                {
                    if (l.Value == t)
                    {
                        if (l.Key.Item1 != -1 && l.Key.Item2 != -1)
                            count_pre++;

                        if (answerList.Contains(l))
                        {
                            tp++;
                            exactMatchTotal++;
                        }
                        if (answerList.ContainsKey(new Tuple<int, int>(-1, l.Key.Item2)) && answerList[new Tuple<int, int>(-1, l.Key.Item2)].Equals(l.Value))
                        {
                            right_tp++;
                            rightMatchTotal++;
                        }
                        if (answerList.ContainsKey(new Tuple<int, int>(l.Key.Item1, -1)) && answerList[new Tuple<int, int>(l.Key.Item1, -1)].Equals(l.Value))
                        {
                            left_tp++;
                            leftMatchTotal++;
                        }
                    }
                }
                foreach (KeyValuePair<Tuple<int, int>, string> l in answerList)
                    if (l.Value == t && l.Key.Item1 != -1 && l.Key.Item2 != -1)
                        count_ans++;

                //exact match
                p = Math.Round((double)tp / count_pre, 4);
                r = Math.Round((double)tp / count_ans, 4);
                f = Math.Round(2 * p * r / (p + r), 4);

                block = String.Format(String.Format("{0,6}", tp) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.Write("|  " + String.Format("{0,-13}", t) + "|" + block);

                if (harmonicMeanType.Contains(t))
                {
                    eP += 1/p;eR += 1/r;eF += 1/f;
                }

                //right match
                p = Math.Round((double)right_tp / count_pre, 4);
                r = Math.Round((double)right_tp / count_ans, 4);
                f = Math.Round(2 * p * r / (p + r), 4);

                block = String.Format(String.Format("{0,6}", right_tp) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.Write("\t|" + block);

                if (harmonicMeanType.Contains(t))
                {
                    rP += 1/p;rR += 1/r;rF += 1/f;
                }

                //left match
                p = Math.Round((double)left_tp / count_pre, 4);
                r = Math.Round((double)left_tp / count_ans, 4);
                f = Math.Round(2 * p * r / (p + r), 4);

                block = String.Format(String.Format("{0,6}", left_tp) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.WriteLine("\t|" + block + "   |");
                Console.WriteLine("+---------------+-----------------------------------+-----------------------------------+-----------------------------------+");

                if (harmonicMeanType.Contains(t))
                {
                    lP += 1/p;lR += 1/r;lF += 1/f;
                }
            }

            Console.Write("|  " + String.Format("{0,-13}", "[-ALL-]"));

            p = Math.Round((double)exactMatchTotal / predictList.Count, 4);
            r = Math.Round((double)exactMatchTotal / answerList.Count * 3, 4);
            f = Math.Round(2 * p * r / (p + r), 4);

            block = String.Format(String.Format("{0,6}", exactMatchTotal) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
            block = String.Format("{0,32}", block);
            Console.Write("|" + block + "   |");


            p = Math.Round((double)leftMatchTotal / predictList.Count, 4);
            r = Math.Round((double)leftMatchTotal / answerList.Count * 3, 4);
            f = Math.Round(2 * p * r / (p + r), 4);

            block = String.Format(String.Format("{0,6}", leftMatchTotal) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
            block = String.Format("{0,32}", block);
            Console.Write(block + "   |");

            p = Math.Round((double)rightMatchTotal / predictList.Count, 4);
            r = Math.Round((double)rightMatchTotal / answerList.Count * 3, 4);
            f = Math.Round(2 * p * r / (p + r), 4);

            block = String.Format(String.Format("{0,6}", rightMatchTotal) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
            block = String.Format("{0,32}", block);
            Console.WriteLine( block + "   |");


            Console.WriteLine("+---------------+-----------------------------------+-----------------------------------+-----------------------------------+");

            //Harmonic Mean

            if (harmonicMeanType.Count > 0)
            {
                Console.Write("| " + String.Format("{0,-13}", "Harmonic Mean*"));

                p = Math.Round((double)harmonicMeanType.Count / eP, 4);
                r = Math.Round((double)(harmonicMeanType.Count / eR), 4);
                f = Math.Round((double)(harmonicMeanType.Count / eF), 4);

                block = String.Format("(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.Write("|" + block + "   |");

                p = Math.Round((double)harmonicMeanType.Count / lP, 4);
                r = Math.Round((double)(harmonicMeanType.Count / lR), 4);
                f = Math.Round((double)(harmonicMeanType.Count / lF), 4);

                block = String.Format("(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.Write(block + "   |");

                p = Math.Round((double)harmonicMeanType.Count / rP, 4);
                r = Math.Round((double)(harmonicMeanType.Count / rR), 4);
                f = Math.Round((double)(harmonicMeanType.Count / rF), 4);

                block = String.Format("(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.WriteLine(block + "   |");

                Console.WriteLine("+---------------+-----------------------------------+-----------------------------------+-----------------------------------+");
            }
            Console.WriteLine("Token-level Accuracy\t" + (double)AccSameCount / AccAllCount);
            Console.WriteLine("Instance-level Accuracy\t" + (double)instanceLevelCorrect / instanceLevelTotal);

            if (harmonicMeanType.Count > 0)
            {
                Console.Write("\n\n*Harmonic Mean is calculate by following type: ");
                foreach (string s in harmonicMeanType)
                    Console.Write(s + " ");
                Console.WriteLine();
            }
        }
        static string ExtractType(string IOB2)
        {
            return IOB2.Contains('-') ? IOB2.Substring(IOB2.IndexOf('-') + 1) : IOB2;
        }
    }
}

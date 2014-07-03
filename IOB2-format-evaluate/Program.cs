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

            List<Tuple<string, int, int>> answerList = new List<Tuple<string, int, int>>(); //type, beginIndex, endIndex
            List<Tuple<string, int, int>> predictList = new List<Tuple<string, int, int>>();
            HashSet<string> type = new HashSet<string>();

            int AccSameCount = 0;
            int AccAllCount = 0;

            string nowStatAns = String.Empty, nowStatPre = String.Empty;
            int beginPositionAns = -1, beginPositionPre = -1;
            
            for (int loop = 1; !sr.EndOfStream; loop++ )
            {
                string line = sr.ReadLine();

                if (line.Equals(String.Empty) )
                {
                    if (beginPositionAns != -1)
                    {
                        answerList.Add(new Tuple<string, int, int>(nowStatAns, beginPositionAns, loop - 1));
                        answerList.Add(new Tuple<string, int, int>(nowStatAns, beginPositionAns, -1));
                        answerList.Add(new Tuple<string, int, int>(nowStatAns, -1, loop - 1));
                    }
                    if (beginPositionPre != -1)
                        predictList.Add(new Tuple<string, int, int>(nowStatPre, beginPositionPre, loop - 1));

                    //restart
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
                    answerList.Add(new Tuple<string, int, int>(nowStatAns, beginPositionAns, loop - 1));
                    answerList.Add(new Tuple<string, int, int>(nowStatAns, beginPositionAns, -1));
                    answerList.Add(new Tuple<string, int, int>(nowStatAns, -1, loop - 1));
                }
                if (!nowStatAns.Equals(answer) || nowStatAns.Equals(String.Empty))
                    beginPositionAns = loop;

                nowStatAns = answer;

                //predict
                if (!nowStatPre.Equals(answer) && !nowStatPre.Equals(String.Empty))
                    predictList.Add(new Tuple<string, int, int>(nowStatPre, beginPositionPre, loop - 1));
                if (!nowStatPre.Equals(answer) || nowStatPre.Equals(String.Empty))
                    beginPositionPre = loop;

                nowStatPre = predict;


                if (predict.Equals(answer))
                    AccSameCount++;
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
            foreach (string t in type)
            {
                int count_ans = 0;
                int count_pre = 0;
                int tp = 0, left_tp = 0, right_tp = 0;

                foreach (Tuple<string, int, int> l in predictList)
                {
                    if (l.Item1 == t)
                    {
                        if (l.Item2 != -1 && l.Item3 != -1)
                            count_pre++;

                        if (answerList.Contains(l))
                        {
                            tp++;
                            exactMatchTotal++;

                        }
                        if (answerList.Contains(new Tuple<string, int, int>(l.Item1, -1, l.Item3)))
                        {
                            right_tp++;
                            rightMatchTotal++;
                        }
                        if (answerList.Contains(new Tuple<string, int, int>(l.Item1, l.Item2, -1)))
                        {
                            left_tp++;
                            leftMatchTotal++;
                        }
                    }
                }
                foreach (Tuple<string, int, int> l in answerList)
                    if (l.Item1 == t && l.Item2 != -1 && l.Item3 != -1)
                        count_ans++;

                //exact match
                p = Math.Round((double)tp / count_pre, 4);
                r = Math.Round((double)tp / count_ans, 4);
                f = Math.Round(2 * p * r / (p + r), 4);

                block = String.Format(String.Format("{0,6}", tp) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.Write("|  " + String.Format("{0,-13}", t) + "|" + block);

                //right match
                p = Math.Round((double)right_tp / count_pre, 4);
                r = Math.Round((double)right_tp / count_ans, 4);
                f = Math.Round(2 * p * r / (p + r), 4);

                block = String.Format(String.Format("{0,6}", right_tp) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.Write("\t|" + block);

                //left match
                p = Math.Round((double)left_tp / count_pre, 4);
                r = Math.Round((double)left_tp / count_ans, 4);
                f = Math.Round(2 * p * r / (p + r), 4);

                block = String.Format(String.Format("{0,6}", left_tp) + "(" + (Double.IsNaN(p) ? "X" : "{0:0.0000}") + " / " + (Double.IsNaN(r) ? "X" : "{1:0.0000}") + " / " + (Double.IsNaN(f) ? "X" : "{2:0.0000}") + ")", p, r, f);
                block = String.Format("{0,32}", block);
                Console.WriteLine("\t|" + block + "   |");
                Console.WriteLine("+---------------+-----------------------------------+-----------------------------------+-----------------------------------+");

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



            Console.WriteLine("Token-level Accuracy\t" + (double)AccSameCount / AccAllCount);
            
        }
        static string ExtractType(string IOB2)
        {
            return IOB2.Contains('-') ? IOB2.Substring(IOB2.IndexOf('-') + 1) : IOB2;
        }
    }
}

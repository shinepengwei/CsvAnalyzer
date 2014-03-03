using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace CsvAnalyzer
{
    class Analyzer
    {
        private ArrayList title;
        private int titleCount;
        private Dictionary<string, ArrayList> content;
        private String dealWithQuotation(string str)
        {
            //将str中可能存在两个双引号，其实表示一个双引号
            string returnStr = "";
            int beginIndex = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '"' && i < str.Length - 1 && str[i + 1] == '"')
                {
                    returnStr += new string(str.ToCharArray(), beginIndex, i - beginIndex)+'"';
                    beginIndex = i + 2;
                }
            }
            if (beginIndex<str.Length)
            {
                returnStr += new string(str.ToCharArray(), beginIndex, str.Length - beginIndex);
            }
            return returnStr;
        }
        private void printError(int curIndex)
        {
            Console.WriteLine("ERROR:无法解析");
            Environment.Exit(2);
        }
        private String getElementFromString(ref int curIndex,  ref string str)
        {
            //解析输入文件的字符串str，根据当前解析指针curIndex，输出解析到的字符串，并将解析指针移动到下一个字符串开始部分
            int beginIndex = curIndex;
            bool hasQuotation = false;
            if (curIndex < str.Length && str[curIndex] == '"')
            {
                hasQuotation = true;
                curIndex++;
                beginIndex++;
            }
            bool hasDQuotation=false;
            while (true)
            {
                if (hasQuotation)
                {
                    //在双引号内
                    if (str[curIndex]=='"')
                    {
                        if (curIndex<str.Length&&str[curIndex + 1]=='"')
                        {
                            curIndex +=2;
                            hasDQuotation = true;
                            continue;
                        }
                        else
                        {
                            curIndex++;
                            break;
                        }

                    }
                    curIndex++;
                }
                else
                {
                    //不在双引号内
                    if (str[curIndex]=='"')
                    {
                       printError(curIndex);
                    }
                    if (str[curIndex]==','||str[curIndex]=='\n'||curIndex>=str.Length)
                    {
                        break;
                    }else{
                        curIndex++;
                    }
                }
            }
            
            if (!hasQuotation)
            {
                return new string(str.ToCharArray(), beginIndex, curIndex - beginIndex);
            }
            else
            {
                //字段包含在双引号内，可能存在有两个双引号代表一个双引号的情况
                if (hasDQuotation)
                {
                    return dealWithQuotation(new string(str.ToCharArray(), beginIndex, curIndex - beginIndex - 1));
                }
                return new string(str.ToCharArray(), beginIndex, curIndex - beginIndex - 1);
            }
        }
        private void PrintArray(ArrayList al)
        {
            Console.Write(al.Count+" [");
            for(int i=0;i<al.Count;i++){
                if ((string)al[i]==string.Empty)
                {
                    Console.Write("NULL,| ");
                }
                else
                    Console.Write(al[i]+",| ");
            }
            Console.Write("]\n");
        }
        private string ConvertStringToPythonElement(string inStr)
        {
            string str = "";
            int outNumNonsence;
            if (int.TryParse(inStr, out outNumNonsence))
            {
                str += inStr;
                return str;
            }
            float outFNumNonsence;
            if (float.TryParse(inStr, out outFNumNonsence))
            {
                str += inStr;
                return str;
            }
            str += "'";
            for (int i = 0; i < inStr.Length; i++)
            {
                switch (inStr[i])
                {
                    case '\\':
                        {
                            str += '\\';
                            str += '\\';
                            break;
                        }
                    case '\'':
                        {
                            str += '\\';
                            str += '\'';
                            break;
                        }
                    case '\"':
                        {
                            str += '\\';
                            str += '\"';
                            break;
                        }
                    default:
                        {
                            str += inStr[i];
                            break;
                        }
                }
            }
            str += "'";
            return str;
        }
        private string ConvertContentLineToString(string key, ArrayList contentLine,ArrayList title)
        {
            string str = key+":{";
            for (int i = 1; i < title.Count;i++ )
            {
                if ((string)contentLine[i-1]!=string.Empty)
                    str += "'" + title[i] + "':" + ConvertStringToPythonElement((string)contentLine[i-1]) + ",";
            }
            str += "},";
            return str;
        }
        public Analyzer()
        {
            title = new ArrayList();
            content = new Dictionary<string, ArrayList>();
        }
        public int AnalyzeHeader(ref string str)
        {
            //解析第一行字符串，将其存到一个ArrayList中，如'技能编号,技能名称,伤害,负面效果,消耗魔法值,字串1,字串2'
            int curIndex=0;
            while (true)
            {
                title.Add(getElementFromString(ref curIndex, ref str));
                this.titleCount++;
                if (str[curIndex]==',')
                {
                    curIndex++;
                    continue;
                }
                if (str[curIndex]=='\n')
                {
                    //解析头部结束
                    curIndex++;
                    break;
                }
                printError(curIndex);
            }
            /*
#if DEBUG
            Console.WriteLine("解析表格头部为：");
            PrintArray(title);
#endif
             * */
            return curIndex;
        }
        public int AnalyzeAContent(int curIndex,ref string str)
        {
            //解析每一行字符串，将其存到一个Dictionary中，其中key为cxv每一行第一列数据中的key，其他内容存在Dictionary中的value中，value的格式为ArrayList
            string key = getElementFromString(ref curIndex,ref  str);
            if (str[curIndex] != ',' && str[curIndex] != '\n'&&curIndex>=str.Length) printError(curIndex);
            curIndex++;
            ArrayList aContentLine = new ArrayList();
            while (true)
            {
                aContentLine.Add(getElementFromString(ref curIndex,ref str));
                if (str[curIndex] != ',' && str[curIndex] != '\n' && curIndex >= str.Length) printError(curIndex);
                if (str[curIndex]==',')
                {
                    curIndex++;
                    continue;
                }else if (str[curIndex]=='\n')
                {
                    curIndex++;
                    if (aContentLine.ToArray().Length!=(this.titleCount-1))
                    {
                        printError(curIndex);
                    }
                    content.Add(key, aContentLine);
                    /*
                    #if DEBUG
                        Console.WriteLine("解析表格内容为：[" + key + ']');
                        PrintArray(content[key]);
                    #endif
                     * */
                    return curIndex;
                }
                else if (curIndex<str.Length)
                {
                    printError(curIndex);
                }
            }
        }
        public string ConvertToPythonData()
        {
            string str = "# -*- coding: gb2312 -*- \ndata = {\n";
            foreach (string key in content.Keys)
            {
                str += ConvertContentLineToString(key, content[key], title)+"\n";
            }
            str += "\n}";
            /*
            #if DEBUG
            Console.WriteLine(str);
            #endif
             * */
            return str;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile;
            string outputFile;
            if (args.Length!=2)
            {
                inputFile = "C:\\data.csv";
                outputFile = "C:\\data.py";
                Console.WriteLine("需要两个参数");
                //Environment.Exit(2);
            }
            else
            {
                inputFile = args[0];
                outputFile = args[1];

            }
           // StreamReader sr=new StreamReader
            string[] strLines = File.ReadAllLines(inputFile, Encoding.GetEncoding("gb2312"));
            string str = "";
            foreach (string strLine in strLines)
            {
                str = str + strLine + '\n';
            }
            int curIndex = 0;
            Analyzer csvAnalyzer = new Analyzer();
            curIndex=csvAnalyzer.AnalyzeHeader(ref str);
            while(true)
            {
                curIndex=csvAnalyzer.AnalyzeAContent(curIndex,ref  str);
                if (curIndex>=str.Length)
                {
                    break;
                }
            }
            string pythonStr=csvAnalyzer.ConvertToPythonData();
            StreamWriter sw = new StreamWriter(outputFile, false, Encoding.GetEncoding("gb2312"));
            sw.Write(pythonStr);
            sw.Flush();
            sw.Close();
           
        }
    }
}

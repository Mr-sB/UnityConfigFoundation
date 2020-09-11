using System;
using System.Collections.Generic;
using System.Text;

namespace GameUtil.Config
{
    public static class CSVDataHelper
    {
        public const char DoubleQuoteCharacter = '\"';
        public const char CommaCharacter = ',';
        
        public static string[] GetCSVRows(this string csvContent)
        {
            //替换回车换行为\r
            string csvText = csvContent.Replace(Environment.NewLine, "\r");
            //去除前后的\r字符
            csvText = csvText.Trim('\r');
            //替换字符
            csvText = csvText.Replace ("\r\r","\r");
            //分割所有行
            return csvText.Split('\r');
        }
        
        /// <summary>
        /// 一行信息解码
        /// </summary>
        public static List<string> GetCSVDecodeRow(this string rowContent, int capacity = 0)
        {
            List<string> cellValues = new List<string>(capacity);
            StringBuilder cellValueBuilder = new StringBuilder();
            bool isStartChar = true;
            bool cellNeedParaphrase = false;
            bool doubleQuoteCanAdd = false;
            foreach (var ch in rowContent)
            {
                switch (ch)
                {
                    case DoubleQuoteCharacter:
                        if (isStartChar) //字段开始有\",整个字段的\"都需要转义(首尾的\"去除外，其余的\"\"变为\")
                            cellNeedParaphrase = true;
                        else if(cellNeedParaphrase)
                        {
                            if (doubleQuoteCanAdd)
                            {
                                cellValueBuilder.Append(ch);
                            }
                            doubleQuoteCanAdd = !doubleQuoteCanAdd;
                        }
                        else
                        {
                            cellValueBuilder.Append(ch);
                        }
                        break;
                    case CommaCharacter:
                        //不转义或者能添加\"的时候碰到，代表字段结束。
                        //能添加\"的时候代表已经经过了偶数个\"（doubleQuoteCanAdd从false变为true代表经过了奇数次变化，加上字段起始的\",所以是偶数个\"）
                        //csv字段内的\"必定是偶数个，而需要转义的情况下\"变为\"\"，所以尾部必定有个落单的\"与起始的\"形成一对，所以而包含在字段内的\,前面必定有奇数个\"
                        if (!cellNeedParaphrase || doubleQuoteCanAdd)
                        {
                            cellValues.Add(cellValueBuilder.ToString());
                            cellValueBuilder.Clear();
                            isStartChar = true;
                            cellNeedParaphrase = false;
                            doubleQuoteCanAdd = false;
                            continue;
                        }
                        cellValueBuilder.Append(ch);
                        break;
                    default:
                        cellValueBuilder.Append(ch);
                        break;
                }
                isStartChar = false;
            }
            //最后一个cell没有\,分隔符
            cellValues.Add(cellValueBuilder.ToString());
            cellValueBuilder.Clear();
            return cellValues;
        }

        public static string GetCSVEncodeRow(this List<string> cellList)
        {
            if (cellList == null || cellList.Count == 0) return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            Stack<int> doubleQuoteInsertIndices = new Stack<int>();
            for (int i = 0, count = cellList.Count; i < count; i++)
            {
                var cell = cellList[i];
                bool cellNeedParaphrase = false;
                int cellStartCharIndex = stringBuilder.Length;
                for (int j = 0, len = cell.Length; j < len; j++)
                {
                    char ch = cell[j];
                    switch (ch)
                    {
                        case DoubleQuoteCharacter:
                            if (j == 0)
                            {
                                cellNeedParaphrase = true;
                                stringBuilder.Append(DoubleQuoteCharacter);//首部加\"
                            }
                            stringBuilder.Append(ch);
                            if (cellNeedParaphrase)
                                stringBuilder.Append(ch);//\"转为\"\"
                            else
                                doubleQuoteInsertIndices.Push(stringBuilder.Length);
                            break;
                        case CommaCharacter:
                            if (!cellNeedParaphrase)
                            {
                                cellNeedParaphrase = true;
                                //内部含有\,把前面的\"转义为\"\"并且在首部添加\"
                                while (doubleQuoteInsertIndices.Count > 0)
                                    stringBuilder.Insert(doubleQuoteInsertIndices.Pop(), DoubleQuoteCharacter);
                                stringBuilder.Insert(cellStartCharIndex, DoubleQuoteCharacter);
                            }
                            stringBuilder.Append(ch);
                            break;
                        default:
                            stringBuilder.Append(ch);
                            break;
                    }
                }
                doubleQuoteInsertIndices.Clear();
                if(cellNeedParaphrase)
                    stringBuilder.Append(DoubleQuoteCharacter);//尾部加\"
                if(i != count - 1)
                    stringBuilder.Append(CommaCharacter);//不是最后一个，添加\,分隔符
            }
            return stringBuilder.ToString();
        }
    }
}
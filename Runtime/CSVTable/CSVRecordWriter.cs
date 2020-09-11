using System.Collections.Generic;

namespace GameUtil.Config
{
    public class CSVRecordWriter
    {
        public readonly List<string> Cells;

        public CSVRecordWriter()
        {
            Cells = new List<string>();
        }

        public void AddCell(string cell)
        {
            Cells.Add(cell);
        }
        
        public override string ToString()
        {
            return Cells.GetCSVEncodeRow();
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtil.Config.Example
{
    public class ExampleTestData
    {
        public int Data1;
        public Color Data2;
        public int Data3;
        public string Data4;
        public string Data5;
        public Vector3[] Data6;
        public ValueTuple<Color, string> Data7;
        public List<ValueTuple<Color, string>> Data8;

        public override string ToString()
        {
            string str = nameof(Data1) + ":" + Data1 + ";" + nameof(Data2) + ":" + Data2 + ";" + nameof(Data3) + ":" +
                         Data3 + ";" + nameof(Data4) + ":" + Data4 + ";" + nameof(Data5) + ":" + Data5 + ";" + nameof(Data6) + ":";
            if (Data6 == null)
                str += "null;";
            else if(Data6.Length > 0)
            {
                str += "{";
                foreach (var data in Data6)
                    str += string.Format("({0},{1},{2})", data.x, data.y, data.z);
                str += "};";
            }

            str += nameof(Data7) + ":" + Data7 + ";" + nameof(Data8) + ":";
            if (Data8 == null)
                str += "null";
            else if(Data8.Count > 0)
            {
                str += "{";
                foreach (var data in Data8)
                    str += data;
                str += "}";
            }
            return str;
        }
    }
}
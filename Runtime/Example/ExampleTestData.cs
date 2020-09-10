using UnityEngine;

namespace GameUtil.Config.Example
{
    public class ExampleTestData
    {
        public int Data1;
        public Color Data2;
        public int Data3;
        public Vector3[] Data4;

        public override string ToString()
        {
            string str = nameof(Data1) + ":" + Data1 + ";" + nameof(Data2) + ":" + Data2 + ";" + nameof(Data3) + ":" +
                         Data3 + ";" + nameof(Data4) + ":";
            if (Data4 == null)
                str += "null";
            else if(Data4.Length > 0)
            {
                str += "{";
                foreach (var data in Data4)
                    str += string.Format("({0},{1},{2})", data.x, data.y, data.z);
                str += "}";
            }

            return str;
        }
    }
}
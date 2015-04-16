using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Misc
{
    static class MyConverters
    {

        public static int[] StringToKey(string key)
        {
            string[] tokens = key.Split(',');

            return Array.ConvertAll<string, int>(tokens, int.Parse);
        }

        public static string KeyToString(int[] key)
        {
            string ret = key[0].ToString();

            for (int i = 1; i < key.Length; i++)
            {
                ret += "," + key[i].ToString();
            }

            return ret;
        }

        public static int BytesToInt(byte fByte, byte sByte)
        {
            return BitConverter.ToInt32(new byte[] { sByte, fByte, 0, 0, }, 0);
        }

        public static bool IsKeyMatch(int[] first, int[] second)
        {
            if (first.Count() != second.Count())
            {
                return false;
            }

            for (int i = 0; i < first.Count(); i++)
            {
                if (Math.Abs(first[i] - second[i]) > 5)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

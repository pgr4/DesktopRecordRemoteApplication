using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Misc
{
    static class MyConverters
    {
        /// <summary>
        /// Convers a csv string of ints to int array
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int[] StringToKey(string key)
        {
            string[] tokens = key.Split(',');

            return Array.ConvertAll<string, int>(tokens, int.Parse);
        }

        /// <summary>
        /// Converts int array to csv of ints
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string KeyToString(int[] key)
        {
            string ret = key[0].ToString();

            for (int i = 1; i < key.Length; i++)
            {
                ret += "," + key[i].ToString();
            }

            return ret;
        }

        /// <summary>
        /// Convers two bytes to an int value
        /// </summary>
        /// <param name="fByte"></param>
        /// <param name="sByte"></param>
        /// <returns></returns>
        public static int BytesToInt(byte fByte, byte sByte)
        {
            return BitConverter.ToInt32(new byte[] { sByte, fByte, 0, 0, }, 0);
        }

        /// <summary>
        /// Matches to int arrays based on their proximity
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
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

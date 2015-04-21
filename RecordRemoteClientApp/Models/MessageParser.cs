using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RecordRemoteClientApp.Models
{
    public struct NewAlbum
    {
        public int Breaks;
        public int[] Key;
    }

    /// <summary>
    /// Class for parsing a UDP message
    /// </summary>
    public class MessageParser
    {
        /// <summary>
        /// Method for parsing the Header
        /// Return null if it is bad
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static MessageHeader ParseHeader(byte[] b, ref int pointer)
        {
            MessageHeader mh = new MessageHeader();
            mh.SourceAddress = GetIP(b, ref pointer);
            mh.DestinationAddress = GetIP(b, ref pointer);
            mh.Command = GetCommand(b, ref pointer);
            if (!IsEndOfHeader(b, ref pointer))
            {
                return null;
            }
            return mh;
        }

        /// <summary>
        /// Method for parsing a New Album Message
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static NewAlbum ParseNewAlbum(byte[] b, ref int pointer)
        {
            NewAlbum na = new NewAlbum();
            na.Key = ParseKey(b, ref pointer);
            na.Breaks = na.Key.Length;
            return na;
        }

        /// <summary>
        /// Get byte increase pointer
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static byte GetByte(byte[] b, ref int pointer)
        {
            pointer += 1;
            return b[pointer - 1];
        }

        /// <summary>
        /// Get the type of Message Command
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static MessageCommand GetCommand(byte[] b, ref int pointer)
        {
            pointer += 1;
            return (MessageCommand)b[pointer - 1];
        }

        /// <summary>
        /// Parse the IP
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static IPAddress GetIP(byte[] b, ref int pointer)
        {
            byte[] arr = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                arr[i] = b[pointer + i];
            }

            IPAddress s = new IPAddress(arr);
            pointer += 4;
            return s;
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static int GetInt(byte[] b, ref int pointer)
        {
            pointer += 1;
            return (int)b[pointer - 1];
        }

        /// <summary>
        /// Check to see if we are at the end of the header 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static bool IsEndOfHeader(byte[] b, ref int pointer)
        {
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    if (b[pointer + i] != 111)
                    {
                        pointer += 6;
                        return false;
                    }
                }

                pointer += 6;
                return true;

            }
            catch
            {
                pointer += 6;
                return false;
            }
        }

        /// <summary>
        /// Method for getting the key
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static int[] ParseKey(byte[] bytes, ref int pointer)
        {
            int endingPoint = 0;
            for (int i = pointer; i < bytes.Length; i++)
            {
                if (bytes[i] == 111 && bytes[i + 1] == 111 && bytes[i + 2] == 111 &&
                    bytes[i + 3] == 111 && bytes[i + 4] == 111 && bytes[i + 5] == 111)
                {
                    endingPoint = i - pointer;
                    break;
                }
            }

            int[] ret = new int[endingPoint / 2];
            for (int i = 0; i < endingPoint / 2; i++)
            {
                byte firstByte = bytes[pointer++];
                byte secondByte = bytes[pointer++];
                ret[i] = BitConverter.ToInt32(new byte[] { secondByte,firstByte, 0, 0, }, 0);
            }

            return ret;
        }
    }
}

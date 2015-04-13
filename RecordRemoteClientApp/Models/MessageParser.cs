using RecordRemoteClientApp.Enumerations;
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

    public class MessageParser
    {
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

        public static NewAlbum ParseNewAlbum(byte[] b, ref int pointer)
        {
            NewAlbum na = new NewAlbum();
            //na.Key = ParseKey(b, ref pointer);
            //na.Breaks = na.Key.Length;
            na.Key = ParseIntKey(b, ref pointer);
            na.Breaks = na.Key.Length;
            return na;
        }

        public static byte GetByte(byte[] b, ref int pointer)
        {
            pointer += 1;
            return b[pointer - 1];
        }

        public static MessageCommand GetCommand(byte[] b, ref int pointer)
        {
            pointer += 1;
            return (MessageCommand)b[pointer - 1];
        }

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

        public static int GetInt(byte[] b, ref int pointer)
        {
            pointer += 1;
            return (int)b[pointer - 1];
        }

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

        public static int[] ParseIntKey(byte[] bytes, ref int pointer)
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

            int[] ret = new int[endingPoint/2];
            for (int i = 0; i < endingPoint/2; i++)
            {
                ret[i] = BitConverter.ToInt32(new byte[] { bytes[pointer++], bytes[pointer++], 0, 0 }, 0);
            }

            return ret;
        }

        public static byte[] ParseKey(byte[] bytes, ref int pointer)
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

            byte[] ret = new byte[endingPoint];
            for (int i = 0; i < endingPoint; i++)
            {
                ret[i] = bytes[pointer++];
            }

            return ret;
        }
    }
}

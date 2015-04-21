using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Misc;
using System;
using System.Net;
using System.Net.Sockets;

namespace RecordRemoteClientApp.Data
{
    /// <summary>
    /// Purpose is to send out UDP messages
    /// </summary>
    public static class Sender
    {
        /// <summary>
        /// Creates generic header with the command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static byte[] MakeHeader(byte command)
        {
            byte[] ret = new byte[15];
            int i = 0;

            byte[] ip = Listener.ThisIpAddress.GetAddressBytes();

            //Sender IP
            ret[i++] = ip[0];
            ret[i++] = ip[1];
            ret[i++] = ip[2];
            ret[i++] = ip[3];

            //Destination IP
            ret[i++] = 192;
            ret[i++] = 168;
            ret[i++] = 1;
            ret[i++] = 5;

            //Command
            ret[i++] = command;

            //Signal end of Header Info
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;

            return ret;
        }

        /// <summary>
        /// Sends a message based on the BusyStatus given
        /// </summary>
        /// <param name="bs"></param>
        public static void SendStatusMessage(BusyStatus bs)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader((byte)bs);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Purpose is to send out a message with the current key value for other apps to update
        /// </summary>
        /// <param name="b"></param>
        public static void SendSyncMessage(int[] b)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader(5);

                byte[] message = new byte[header.Length + (b.Length * 2) + 6];

                header.CopyTo(message, 0);

                byte[] newKey = new byte[b.Length * 2];
                int index = -1;

                for (int i = 0; i < b.Length; i++)
                {
                    var x = BitConverter.GetBytes(b[i]);
                    newKey[++index] = x[1];
                    newKey[++index] = x[0];
                }

                newKey.CopyTo(message, header.Length);

                for (int i = header.Length + newKey.Length; i < message.Length; i++)
                {
                    message[i] = 111;
                }

                s.SendTo(message, ep);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// For all commands that dont require a message body
        /// </summary>
        /// <param name="mc"></param>
        public static void SendGenericMessage(MessageCommand mc)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader((byte)mc);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Sends out a Play message to the arduino
        /// </summary>
        public static void PlayMessage()
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader((byte)MessageCommand.MediaPlay);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Sends out a Pause message to the arduino
        /// </summary>
        public static void StopMessage()
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader((byte)MessageCommand.MediaStop);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Sends out message for the arduino to go to the currentlocation given in 2 byte int
        /// </summary>
        public static void GoToTrackMessage(byte msb, byte lsb)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader((byte)MessageCommand.GoToTrack);

                byte[] message = new byte[header.Length + 2];
                header.CopyTo(message, 0);
                message[message.Length - 2] = msb;
                message[message.Length - 1] = lsb;

                s.SendTo(message, ep);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Sends out message for the arduino to go to the currentlocation given in 2 byte int
        /// Also updates all other apps
        /// </summary>
        public static void QueueGoToTrackMessage(byte msb, byte lsb)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = MakeHeader((byte)MessageCommand.QueueGoToTrack);

                byte[] message = new byte[header.Length + 2];
                header.CopyTo(message, 0);

                message[message.Length - 2] = msb;
                message[message.Length - 1] = lsb;

                s.SendTo(message, ep);
            }
            catch (Exception e)
            {

            }
        }

    }
}

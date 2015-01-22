using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RecordRemoteClientApp.Data
{
    public class Listener
    {
        #region Members

        private static Listener instance;

        private Listener() { }

        public static Listener Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Listener();
                }
                return instance;
            }
        }

        #endregion

        #region Events

        public delegate void NewAlbumDetected(NewAlbum na);
        public event NewAlbumDetected NewAlbumEvent;

        public delegate void NewCurrentAlbumDetected(NewAlbum na);
        public event NewCurrentAlbumDetected NewCurrentAlbum;
        #endregion

        private const int listenPort = 30003;

        public void Speak()
        {
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            try
            {
                while (true)
                {
                    int pointer = 0;

                    byte[] bytes = listener.Receive(ref groupEP);

                    MessageHeader mh = MessageParser.ParseHeader(bytes, ref pointer);

                    if (mh != null)
                    {
                        switch (mh.Command)
                        {
                            case MessageCommand.None:
                                break;
                            case MessageCommand.NewAlbum:
                                {
                                    NewAlbum na = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                    if (NewAlbumEvent != null)
                                    {
                                        NewAlbumEvent(na);
                                    }
                                    break;
                                }
                            case MessageCommand.CurrentAlbum:
                                {
                                    //Todo: Change to use CurrentAlbum methods and members later
                                    NewAlbum na = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                    if (NewCurrentAlbum != null)
                                    {
                                        NewCurrentAlbum(na);
                                    }
                                    break;
                                }
                        }
                    }
                    else
                    {
                        //Message was not parsed correctly
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                listener.Close();
            }

        }

    }
}

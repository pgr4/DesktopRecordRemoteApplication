using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using RecordRemoteClientApp.Enumerations;

namespace RecordRemoteClientApp.Models
{
    /// <summary>
    /// Standard Message Header layout
    /// </summary>
    public class MessageHeader
    {
        public MessageHeader()
        {

        }

        private IPAddress sourceAddress;

        public IPAddress SourceAddress
        {
            get { return sourceAddress; }
            set { sourceAddress = value; }
        }

        private IPAddress destinationAddress;

        public IPAddress DestinationAddress
        {
            get { return destinationAddress; }
            set { destinationAddress = value; }
        }

        private MessageCommand command;

        public MessageCommand Command
        {
            get { return command; }
            set { command = value; }
        }
        
    }
}

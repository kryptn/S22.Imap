using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace S22.Imap.Test
{
    class EventDispatcherOverrideTest
    {
    }

    public class ImapClient : S22.Imap.ImapClient
    {
        public override event EventHandler<IdleMessageEventArgs> newMessageEvent;
        public override event EventHandler<IdleMessageEventArgs> messageDeleteEvent;


        protected override void EventDispatcher()
        {
            uint lastUid = 0;
            while (true)
            {
                string response = idleEvents.Dequeue();
                Match m = Regex.Match(response, @"\*\s+(\d+)\s+(\w+)");
                if (!m.Success)
                    continue;
                try
                {
                    uint numberOfMessages = Convert.ToUInt32(m.Groups[1].Value),
                        uid = GetHighestUID();
                    switch (m.Groups[2].Value.ToUpper())
                    {
                        case "EXISTS":
                            if (lastUid != uid)
                            {
                                newMessageEvent.Raise(this,
                                    new IdleMessageEventArgs(numberOfMessages, uid, this));
                            }
                            break;
                        case "EXPUNGE":
                            messageDeleteEvent.Raise(
                                this, new IdleMessageEventArgs(numberOfMessages, uid, this));
                            break;
                    }
                    lastUid = uid;
                }
                catch
                {
                    // Fall through.
                }
            }
        }

        public ImapClient(string hostname, int port = 143, bool ssl = false,
            RemoteCertificateValidationCallback validate = null)
            : base(hostname, port, ssl, validate)
        {
        }

        public ImapClient(string hostname, int port, string username, string password,
            AuthMethod method = AuthMethod.Auto, bool ssl = false, RemoteCertificateValidationCallback validate = null)
            : base(hostname, port, username, password, method, ssl, validate)
        {
        }
    }
}

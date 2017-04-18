using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Boggle
{
    public class BoggleServer
    {
        
        /// <summary>
        /// Launches a BoggleServer on port 6000.  Keeps the main
        /// thread active so we can send output to the console.
        /// </summary>
        static void Main(string[] args)
        {
            
            new BoggleServer(60000);
             Console.ReadLine();
        }

        // Listens for incoming connection requests
        private TcpListener server;

        /// <summary>
        /// Creates a BoggleServer that listens for connection requests on port 6000.
        /// </summary>
        public BoggleServer(int port)
        {
            // A TcpListener listens for incoming connection requests
            server = new TcpListener(IPAddress.Any, port);
            // Start the TcpListener
            server.Start();

            // Ask the server to call ConnectionRequested at some point in the future when 
            // a connection request arrives.  It could be a very long time until this happens.
            // The waiting and the calling will happen on another thread.  BeginAcceptSocket 
            // returns immediately, and the constructor returns to Main.
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        /// <summary>
        /// This is the callback method that is passed to BeginAcceptSocket.  It is called
        /// when a connection request has arrived at the server.
        /// </summary>
        private void ConnectionRequested(IAsyncResult result)
        {
            // We obtain the socket corresonding to the connection request.  Notice that we
            // are passing back the IAsyncResult object.
            Socket s = server.EndAcceptSocket(result);

            // We ask the server to listen for another connection request.  As before, this
            // will happen on another thread.
            server.BeginAcceptSocket(ConnectionRequested, null);

            // We create a new ClientConnection, which will take care of communicating with
            // the remote client.
            new ClientConnection(s);
        }
    }

    /// <summary>
    /// Represents a connection with a remote client.  Takes care of receiving and sending
    /// information to that client according to the protocol.
    /// </summary>
    class ClientConnection
    {
        private BoggleService myServer = new BoggleService();

        // Incoming/outgoing is UTF8-encoded.  This is a multi-byte encoding.  The first 128 Unicode characters
        // (which corresponds to the old ASCII character set and contains the common keyboard characters) are
        // encoded into a single byte.  The rest of the Unicode characters can take from 2 to 4 bytes to encode.
        private static System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

        // Buffer size for reading incoming bytes
        private const int BUFFER_SIZE = 1024;

        // The socket through which we communicate with the remote client
        private Socket socket;

        // Text that has been received from the client but not yet dealt with
        private StringBuilder incoming;

        // Text that needs to be sent to the client but which we have not yet started sending
        private StringBuilder outgoing;

        // For decoding incoming UTF8-encoded byte streams.
        private Decoder decoder = encoding.GetDecoder();

        // Buffers that will contain incoming bytes and characters
        private byte[] incomingBytes = new byte[BUFFER_SIZE];
        private char[] incomingChars = new char[BUFFER_SIZE];

        // Records whether an asynchronous send attempt is ongoing
        private bool sendIsOngoing = false;

        // For synchronizing sends
        private readonly object sendSync = new object();

        // Bytes that we are actively trying to send, along with the
        // index of the leftmost byte whose send has not yet been completed
        private byte[] pendingBytes = new byte[0];
        private int pendingIndex = 0;

        /// <summary>
        /// Creates a ClientConnection from the socket, then begins communicating with it.
        /// </summary>
        public ClientConnection(Socket s)
        {
            // Record the socket and clear incoming
            socket = s;
            incoming = new StringBuilder();
            outgoing = new StringBuilder();


            // Ask the socket to call MessageReceive as soon as up to 1024 bytes arrive.
            socket.BeginReceive(incomingBytes, 0, incomingBytes.Length,
                                SocketFlags.None, MessageReceived, null);
        }

        /// <summary>
        /// Called when some data has been received.
        /// </summary>
        private void MessageReceived(IAsyncResult result)
        {
            // Figure out how many bytes have come in
            int bytesRead = socket.EndReceive(result);

            // If no bytes were received, it means the client closed its side of the socket.
            // Report that to the console and close our socket.
            if (bytesRead == 0)
            {
                Console.WriteLine("Socket closed");
                socket.Close();
            }

            // Otherwise, decode and display the incoming bytes.  Then request more bytes.
            else
            {
                // Convert the bytes into characters and appending to incoming
                int charsRead = decoder.GetChars(incomingBytes, 0, bytesRead, incomingChars, 0, false);
                incoming.Append(incomingChars, 0, charsRead);

                bool finish = false;
                string httpMethod = null;
                string urlParam = null;
                string urlCall= null;
                int bodyLength = 0;
                string jsonThing = null;
                //Needs to be a parameter?
                string brief = "yes";
                Regex contentLine = new Regex(@"Content-Length:\s(?<bodyLength>\d+)");
                Regex urlLine = new Regex(@"^(?<httpMethod>.+)\s/BoggleService.svc/(?<urlCall>.*)/?(?<urlParam>.*)?\sHTTP/1.1");
                int lastNewline = -1;
                int start = 0;
                string myString = incoming.ToString();
                bool hasBody = false;
                for (int i = 0; i < incoming.Length; i++)
                {
                    if (!finish)
                    {
                        if (incoming[i] == '\n')
                        {
                            if (urlLine.IsMatch(incoming.ToString(start, i - start)))
                            {
                                Match match = urlLine.Match(incoming.ToString(start, i));
                                httpMethod = match.Groups["httpMethod"].Value;
                                urlParam = match.Groups["urlParam"].Value;
                                urlCall = match.Groups["urlCall"].Value;
                            }
                            else if (contentLine.IsMatch(incoming.ToString(start, i - start)))
                            {
                                Match match = contentLine.Match(incoming.ToString(start, i - start));
                                bodyLength = int.Parse(match.Groups["bodyLength"].Value);
                                hasBody = true;
                            }
                            

                            else if (hasBody && incoming[i+1] == '\r' && incoming[i + 2] == '\n')
                            {
                                if (incoming.Length == i + 4 + bodyLength)
                                {
                                    jsonThing = incoming.ToString(i + 4, bodyLength);
                                    finish = true;
                                }
                            }
                            lastNewline = i;
                            start = i + 1;
                        }
                    }
                }
                incoming.Remove(0, lastNewline + 1);

                //TEMP CODE FOR TESTING CREATE USER, DELETE WHEN PARSING WORKS
                //httpMethod = "POST";
                //urlCall = "users";
                //jsonThing = "{\"Nickname\": \"qwfp\"}";

                if (jsonThing == "" )
                {

                }
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonThing);
                string returnString = null;
                HttpStatusCode status = HttpStatusCode.Forbidden;
                
                if (finish)
                {
                    socket.BeginReceive(incomingBytes, 0, incomingBytes.Length,
                        SocketFlags.None, MessageReceived, null);
                }
                if (httpMethod == "POST")
                {
                    if (urlCall == "users")
                    {
                        UserInfo userInfo = new UserInfo();
                        userInfo.Nickname = json.Nickname;
                        returnString = Newtonsoft.Json.JsonConvert.SerializeObject(myServer.CreateUser(userInfo, out status));
                    }
                    else if (urlCall == "games")
                    {
                        JoinGameInfo gameInfo = new JoinGameInfo();
                        gameInfo.UserToken = json.UserToken;
                        gameInfo.TimeLimit = json.TimeLimit;
                        returnString = Newtonsoft.Json.JsonConvert.SerializeObject(myServer.JoinGame(gameInfo, out status));
                    }
                }
                else if (httpMethod == "PUT")
                {
                    if (urlCall == "games")
                    {
                        if (urlParam == null)
                        {
                            UserID user = new UserID();
                            user.UserToken = json.UserToken;
                            myServer.CancelJoinRequest(user, out status);
                        }
                        else
                        {
                            UserIDandPlayWord idAndWord = new UserIDandPlayWord();
                            idAndWord.UserToken = json.UserToken;
                            idAndWord.Word = json.Word;
                            returnString = Newtonsoft.Json.JsonConvert.SerializeObject(myServer.PlayWord(idAndWord, urlParam, out status));
                        }
                    }
                }
                else if (httpMethod == "GET" && urlCall == "games" && urlParam != null)
                {
                    returnString = Newtonsoft.Json.JsonConvert.SerializeObject(myServer.GameStatus(brief, urlParam, out status));
                }

                byte[] returnStringBytes = Encoding.ASCII.GetBytes(returnString);

                string returnHeader = "HTTP/1.1 " + (int)status + " " + status +
                    "\r\nContent-Length: " + returnStringBytes.Length +
                    "\r\nContent-Type: application / json; charset=utf-8\r\n";

                //pendingBytes = Encoding.ASCII.GetBytes(returnHeader + "\r\n" + returnString);
                string notherString = returnHeader + "\r\n" + returnString;
                SendMessage(notherString);
            }
        }
            
        

        /// <summary>
        /// Sends a string to the client
        /// </summary>
        private void SendMessage(string lines)
        {
            // Get exclusive access to send mechanism
            lock (sendSync)
            {
                // Append the message to the outgoing lines
                outgoing.Append(lines);

                // If there's not a send ongoing, start one.
                if (!sendIsOngoing)
                {
                    Console.WriteLine("Appending a " + lines.Length + " char line, starting send mechanism");
                    sendIsOngoing = true;
                    SendBytes();
                }
                else
                {
                    Console.WriteLine("\tAppending a " + lines.Length + " char line, send mechanism already running");
                }
            }
        }

        /// <summary>
        /// Attempts to send the entire outgoing string.
        /// This method should not be called unless sendSync has been acquired.
        /// </summary>
        private void SendBytes()
        {
            // If we're in the middle of the process of sending out a block of bytes,
            // keep doing that.
            if (pendingIndex < pendingBytes.Length)
            {
                Console.WriteLine("\tSending " + (pendingBytes.Length - pendingIndex) + " bytes");
                socket.BeginSend(pendingBytes, pendingIndex, pendingBytes.Length - pendingIndex,
                                 SocketFlags.None, MessageSent, null);
            }

            // If we're not currently dealing with a block of bytes, make a new block of bytes
            // out of outgoing and start sending that.
            else if (outgoing.Length > 0)
            {
                pendingBytes = encoding.GetBytes(outgoing.ToString());
                pendingIndex = 0;
                Console.WriteLine("\tConverting " + outgoing.Length + " chars into " + pendingBytes.Length + " bytes, sending them");
                outgoing.Clear();
                socket.BeginSend(pendingBytes, 0, pendingBytes.Length,
                                 SocketFlags.None, MessageSent, null);
            }

            // If there's nothing to send, shut down for the time being.
            else
            {
                Console.WriteLine("Shutting down send mechanism\n");
                sendIsOngoing = false;
            }
        }

        /// <summary>
        /// Called when a message has been successfully sent
        /// </summary>
        private void MessageSent(IAsyncResult result)
        {
            // Find out how many bytes were actually sent
            int bytesSent = socket.EndSend(result);
            Console.WriteLine("\t" + bytesSent + " bytes were successfully sent");

            // Get exclusive access to send mechanism
            lock (sendSync)
            {
                // The socket has been closed
                if (bytesSent == 0)
                {
                    socket.Close();
                    Console.WriteLine("Socket closed");
                }

                // Update the pendingIndex and keep trying
                else
                {
                    pendingIndex += bytesSent;
                    SendBytes();
                }
            }
        }
    }
}


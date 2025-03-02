using PasswordCrackerServer.Models;
using ServerFramework.TCPServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PasswordCrackerServer
{
    public class CrackerServer : AbstractTcpServer
    {
        private int receiveBuffer = 1048576; // 1 MB
        private int sendBuffer = 1048576; // 1 MB
        private UncrackedLoginInfoDatabase loginInfoDatabase = new UncrackedLoginInfoDatabase();
        private WordDatabase wordDatabase = new WordDatabase();
        private CrackedPasswordsDatabase crackedPasswordsDatabase = new CrackedPasswordsDatabase();
        int _wordCount = 0;
        int _wordsPerTask = 10000;
        object _wordCountLock = new object();
        object _writeCrackedPsLock = new object();
        public CrackerServer(string configPath)
        {
            try
            {
                this.LoadConfig(configPath);
            }
            catch (Exception ex)
            {
                ServerLogger.Instance.TraceEvent(TraceEventType.Error, _port, $"Failed to parse configuration file at {configPath}, using default values. Raw Error: {ex.Message}");
            }

            foreach (var h in loginInfoDatabase.GetAll().Keys)
            {
                using (StreamWriter c = File.AppendText("./test.txt"))
                {
                    c.Write(Convert.ToHexString(h.Hash) + "\n");
                }
            }
        }
        protected override void TcpServerWork(TcpClient incomingClient)
        {
            incomingClient.ReceiveBufferSize = receiveBuffer;
            incomingClient.SendBufferSize = sendBuffer;
            using (NetworkStream stream = incomingClient.GetStream())
            {
                byte[] data = null; // the buffer to use to store bytes before they are sent to the networkstream
                while(!_stop) 
                {
                    ServerConnectionStateFlag stateFlag;
                    try
                    {
                        stateFlag = (ServerConnectionStateFlag)stream.ReadByte();
                    }
                    catch (SocketException s)
                    {
                        stateFlag = ServerConnectionStateFlag.Disconnected;
                    }
                    switch (stateFlag)
                    {
                        case ServerConnectionStateFlag.Disconnected:
                            incomingClient.Close();
                            return;
                        case ServerConnectionStateFlag.RequestTask:
                            ServerLogger.Instance.TraceEvent(TraceEventType.Information, _port, $"Task requested from Endpoint {incomingClient.Client.RemoteEndPoint}");
                            // send words to be hashed and compared to see if it matches one of the existing hashes
                            IList<byte[]> allWords = wordDatabase.GetAllWordBytes();
                            // lock the checking and assignment of _wordCount, to synchronize all connections
                            lock(_wordCountLock)
                            {
                                if(_wordCount >= allWords.Count)
                                {
                                    ServerLogger.Instance.TraceEvent(TraceEventType.Information, _port, "All words has been processed, refusing all incoming requests for additional tasks");
                                    stream.WriteByte((byte)ClientConnectionStateFlag.NoTasksAvailable);
                                    break;
                                }
                                int numOfWordsToProcess = Math.Min(allWords.Count - _wordCount, _wordsPerTask);
                                ArraySegment<byte[]> wordSlice = new ArraySegment<byte[]>(allWords.ToArray(), _wordCount, numOfWordsToProcess);
                                data = NetworkSerializer.SerializeWordBytesToNetwork(wordSlice);
                                _wordCount = _wordCount + _wordsPerTask;
                                ServerLogger.Instance.TraceEvent(TraceEventType.Information, _port, $"Sending {numOfWordsToProcess} words to {incomingClient.Client.RemoteEndPoint} for processing");
                            }
                            stream.Write(data);            
                            break;
                        case ServerConnectionStateFlag.SendCrackedPasswords:
                            // receive passwords and accompanying usernames cracked by the slave
                            IDictionary<SHA1Hash, string> crackedPasswords = NetworkSerializer.DeserializeCrackedPasswordsFromNetwork(stream);
                            ServerLogger.Instance.TraceEvent(TraceEventType.Information, _port, $"Received {crackedPasswords.Count} cracked passwords from {incomingClient.Client.RemoteEndPoint}");
                            foreach (var kvp  in crackedPasswords)
                            {
                                lock(_writeCrackedPsLock)
                                {
                                        
                                    crackedPasswordsDatabase.AddOrUpdatePassword(kvp.Key, kvp.Value);
                                    if(loginInfoDatabase.GetAll().TryGetValue(kvp.Key, out List<LoginIdentifier> login))
                                    {
                                        foreach (var userID in login)
                                        {
                                            if (!crackedPasswordsDatabase.ContainsUser(userID))
                                            {
                                                FileSerializer.WriteCrackedPasswordToFile("./crackedpasswords.txt", kvp.Key, kvp.Value, userID);
                                                crackedPasswordsDatabase.AddUser(userID);
                                            }
                                            else
                                            {
                                                ServerLogger.Instance.TraceEvent(TraceEventType.Warning, _port, $"Skipping writing user idenfitier {userID} as it already exists in the database");
                                            }
                                                    
                                        }
                                                
                                    }
                                    else
                                    {
                                        ServerLogger.Instance.TraceEvent(TraceEventType.Warning, _port, $"Cracked password {kvp.Value} could not be mapped to any known hashes on the server, is the client using the same reference hashes?");
                                    }
                                }
                                    
                            }

                            break;
                        case ServerConnectionStateFlag.RequestHashes:
                            // send the hashes to be compared against
                            var hashes = loginInfoDatabase.GetAll().Keys.ToArray();
                            data = NetworkSerializer.SerializeHashesInfoToNetwork(hashes);
                            stream.Write(data);
                            ServerLogger.Instance.TraceEvent(TraceEventType.Information, _port, $"Sent {hashes.Length} hashes to {incomingClient.Client.RemoteEndPoint} for comparison");
                            break;
                        default:
                            ServerLogger.Instance.TraceEvent(TraceEventType.Warning, _port, $"Invalid data detected from {incomingClient.Client.RemoteEndPoint}, clearing buffer.");
                            ClearBuffer(stream);
                            break;
                    }
                   
                }
                
            }
            
        }

        private void ClearBuffer(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            while(stream.DataAvailable)
            {
                stream.Read(buffer, 0, buffer.Length);
            }
        }
        public override XmlDocument LoadConfig(string configPath)
        {
            XmlDocument config = base.LoadConfig(configPath);
            XmlNode? receiveBufferNode = config.DocumentElement.SelectSingleNode("ReceiveBuffer");
            if (receiveBufferNode != null)
            {
                string xxStr = receiveBufferNode.InnerText.Trim();
                receiveBuffer = int.Parse(xxStr);
            }
            XmlNode? sendBufferNode = config.DocumentElement.SelectSingleNode("SendBuffer");
            if (sendBufferNode != null)
            {
                string xxStr = sendBufferNode.InnerText.Trim();
                sendBuffer = int.Parse(xxStr);
            }
            XmlNode? wordsPerTaskNode = config.DocumentElement.SelectSingleNode("WordsPerTask");
            if (wordsPerTaskNode != null)
            {
                string xxStr = wordsPerTaskNode.InnerText.Trim();
                _wordsPerTask = int.Parse(xxStr);
            }
            XmlNodeList? dictionaries = config.DocumentElement.SelectNodes("Dictionary");
            if(dictionaries != null)
            {
                foreach(XmlNode dictionariesNode in dictionaries)
                {
                    string xxStr = dictionariesNode.InnerText;
                    try
                    {
                        wordDatabase.LoadDictionary(xxStr);
                    }
                    catch (Exception ex)
                    {
                        ServerLogger.Instance.TraceEvent(TraceEventType.Error, _port, $"Failed to load word dictionary at {xxStr}. Full error: {ex.Message}");
                    }
                }
            }
            XmlNodeList? userInfos = config.DocumentElement.SelectNodes("UserInfo");
            if (userInfos != null)
            {
                foreach (XmlNode userInfo in userInfos)
                {
                    string xxStr = userInfo.InnerText;
                    try
                    {
                        loginInfoDatabase.LoadData(xxStr);
                    }
                    catch (Exception ex)
                    {
                        ServerLogger.Instance.TraceEvent(TraceEventType.Error, _port, $"Failed to load user info file at {xxStr}. Full error: {ex.Message}");
                    }
                }
            }
            return config;
        }

        
    }
}

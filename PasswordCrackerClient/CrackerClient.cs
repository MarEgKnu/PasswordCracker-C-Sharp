using ClientFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerClient
{
    public class CrackerClient : AbstractTcpClient
    {
        int _recBuffSize;
        int _sendBuffSize;
        int _theadsToUse;
        PasswordCracker cracker;
        public CrackerClient(string hostname, int port, int recBuffSize, int sendBuffSize, int threadsToUse) : base(hostname, port)
        {
            _recBuffSize = recBuffSize;
            _sendBuffSize = sendBuffSize;
            _theadsToUse = threadsToUse;
        }

        protected override void DoClientWork(TcpClient client)
        {
            client.ReceiveBufferSize = _recBuffSize;
            client.SendBufferSize = _sendBuffSize;
            using (NetworkStream stream = client.GetStream())
            {
                HashSet<SHA1Hash> hashes = GetHashesFromServer(stream);
                cracker = new PasswordCracker(hashes);
                while(true)
                {
                    List<string> words = GetWordsFromServer(stream, client);
                    var passwords = cracker.ProcessWords(words, _theadsToUse);
                    SendCrackedPasswordsToServer(stream,passwords);

                }
            }
           
        }

        private HashSet<SHA1Hash> GetHashesFromServer(NetworkStream stream)
        {
            while (true)
            {
                // send flag to reqest passwords
                stream.WriteByte((byte)ServerConnectionStateFlag.RequestHashes);
                // read header
                ClientConnectionStateFlag flag = (ClientConnectionStateFlag)stream.ReadByte();
                switch(flag)
                {
                    case ClientConnectionStateFlag.ReceivingHashes:
                        return NetworkSerializer.DeserializeHashesFromNetwork(stream);
                    default:
                        ClearBuffer(stream);
                        break;
                }
                Thread.Sleep(150);
            }
        }
        private List<string> GetWordsFromServer(NetworkStream stream, TcpClient client)
        {
            while (true)
            {
                // send flag to reqest words to process
                stream.WriteByte((byte)ServerConnectionStateFlag.RequestTask);
                // read header
                ClientConnectionStateFlag flag = (ClientConnectionStateFlag)stream.ReadByte();
                switch(flag)
                {
                    case ClientConnectionStateFlag.ReceivingWords:
                        return NetworkSerializer.DeserializeWordsFromNetwork(stream);
                    case ClientConnectionStateFlag.NoTasksAvailable:
                        client.Close();
                        Environment.Exit(0);
                        break;
                    default:
                        // clear buffer
                        ClearBuffer(stream);
                        break;
                }
                Thread.Sleep(150);
            }
        }
        private void SendCrackedPasswordsToServer(NetworkStream stream, IDictionary<SHA1Hash, string> passwords)
        {
            if(passwords.Count == 0)
            {
                return;
            }
            stream.Write(NetworkSerializer.SerializeCrackedPasswordsToNetwork(passwords));
        }
        private void ClearBuffer(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            while (stream.DataAvailable)
            {
                stream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}

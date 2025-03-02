using PasswordCrackerServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    public static class NetworkSerializer
    {
        private const int MAX_TRIES = 200;
        public static byte[] SerializeWordBytesToNetwork(IList<byte[]> wordBytes)
        {
            if (wordBytes.Count == 0)
            {
                return new byte[0];
            }
            List<byte> result = new List<byte>(5) { 0, 0, 0, 0, 0 }; // initalize flag&size bytes
            for (int word = 0; word < wordBytes.Count; word++)
            {
                for (int letter = 0; letter < wordBytes[word].Length; letter++)
                {
                    result.Add(wordBytes[word][letter]);
                }
                // add null terminator after each word
                result.Add(0);
            }


            int dataLength = result.Count - 5;

            //put connectionState and length bytes at the start
            result[0] = (byte)ClientConnectionStateFlag.ReceivingWords; // flag
            result[1] = (byte)((dataLength) >> 24);

            result[2] = (byte)((dataLength) >> 16);

            result[3] = (byte)((dataLength) >> 8);

            result[4] = (byte)((dataLength) /*>> 0*/);

            return result.ToArray();
        }
        public static byte[] SerializeLoginInfoToNetwork(Dictionary<LoginIdentifier, byte[]> loginInfos)
        {
            List<byte> result = new List<byte>(5)
            {
                1, 0, 0, 0,0 // allocate storage for flags and length
            };
            foreach (var loginInfo in loginInfos)
            {
                result.AddRange(Encoding.UTF8.GetBytes(loginInfo.Key.Username));
                result.Add(0); // null byte to terminate the string
                result.AddRange(loginInfo.Value); // add hash 
            }
            int length = result.Count;
            //put connectionState and length bytes at the start
            result[0] = 1; // flag
            result[1] = (byte)((length) >> 24);

            result[2] = (byte)((length) >> 16);

            result[3] = (byte)((length) >> 8);

            result[4] = (byte)((length) /*>> 0*/);

            return result.ToArray();
        }
        public static byte[] SerializeHashesInfoToNetwork(SHA1Hash[] hashes)
        {
            // SHA1 hashes are always 20 bytes in size, plus 5 for flag/size bytes
            byte[] result = new byte[hashes.Length * 20 + 5];
            int index = 5;
            for (int i = 0; i < hashes.Length; i++)
            {
                for (int j = 0; j < hashes[i].Hash.Length; j++)
                {
                    result[index] = hashes[i][j];
                    index++;
                }
            }
            int length = result.Length - 5; // dont include the flag byte or the length bytes in the total length
            //put connectionState and length bytes at the start
            result[0] = (byte)ClientConnectionStateFlag.ReceivingHashes; // flag
            result[1] = (byte)((length) >> 24);

            result[2] = (byte)((length) >> 16);

            result[3] = (byte)((length) >> 8);

            result[4] = (byte)((length) /*>> 0*/);

            return result;
        }
        public static IDictionary<SHA1Hash, string> DeserializeCrackedPasswordsFromNetwork(NetworkStream stream)
        {
            Dictionary<SHA1Hash, string> result = new Dictionary<SHA1Hash, string>();
            // read data length from the input
            byte[] data = TryReadData(stream);
            int i = 0;
            int stringStart = 0;
            while (i < data.Length)
            {
                // read hash first
                byte[] hash = new byte[20];
                Array.Copy(data, i, hash, 0, 20);
                i = i + 20;
                stringStart = i;
                string plaintextPs = null;
                // then read in word
                while ( i < data.Length)
                {
                    // if its a null terminator, save the word in variable for adding to result dict later
                    if (data[i] == 0x0)
                    {
                        byte[] word = new byte[i - stringStart];
                        Array.Copy(data, stringStart, word, 0, word.Length);
                        plaintextPs = Encoding.UTF8.GetString(word);
                        i++;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                if(plaintextPs == null)
                {
                    throw new InvalidDataException("No null terminator after end of string detected");
                }
                result.TryAdd(new SHA1Hash(hash), plaintextPs);

            }

            return result;
        }
        private static int TryReadDataLength(NetworkStream stream)
        {
            byte[] dataLength = new byte[4];
            int bytesRead = 0;
            int tries = 0;
            while (bytesRead != dataLength.Length)
            {
                if (stream.DataAvailable)
                {
                    // read data length from the input              
                    bytesRead += stream.Read(dataLength, bytesRead, dataLength.Length - bytesRead);
                }
                else
                {
                    tries++;
                    Thread.Sleep(10);
                }
                if(tries > MAX_TRIES)
                {
                    throw new TimeoutException($"Timed out trying to read data length header of {dataLength.Length} bytes, but only got {bytesRead} after {tries} tries.");
                }
            }
            // reverse array if little endian, before conversion to int
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataLength);
            return BitConverter.ToInt32(dataLength);
        }
        private static byte[] TryReadData(NetworkStream stream)
        {
            int dataLength = TryReadDataLength(stream);
            byte[] data = TryReadPayload(stream, dataLength);
            return data;

        }
        private static byte[] TryReadPayload(NetworkStream stream, int dataLength)
        {
            byte[] data = new byte[dataLength];
            int bytesRead = 0;
            int tries = 0;
            while (bytesRead != data.Length)
            {
                if (stream.DataAvailable)
                {
                    bytesRead += stream.Read(data, bytesRead, data.Length - bytesRead);
                }
                else
                {
                    tries++;
                    Thread.Sleep(10);
                }
                if (tries > MAX_TRIES)
                {
                    throw new TimeoutException($"Timed out trying to read {data.Length} bytes of data, but only got {bytesRead} after {tries} tries.");
                }

            }
            return data;
        }
    }
}

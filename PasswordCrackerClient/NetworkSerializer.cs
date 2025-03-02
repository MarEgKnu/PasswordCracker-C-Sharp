using PasswordCrackerClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerClient
{
    public static class NetworkSerializer
    {
        public static Dictionary<LoginIdentifier, byte[]> DeserializeLoginInfoFromNetwork(byte[] rawData)
        {
            Dictionary<LoginIdentifier, byte[]> result = new Dictionary<LoginIdentifier, byte[]>();
            int stringStart = 0;
            int index = 0;
            while(index < rawData.Length)
            {
                if (rawData[index] == 0)
                {
                    LoginIdentifier identifier = new LoginIdentifier(Encoding.UTF8.GetString(rawData, stringStart, (index) - stringStart));
                    index++;
                    if (index + 20 > rawData.Length)
                    {
                        throw new IndexOutOfRangeException("Expected 20 bytes hash after identifier, but not enough space");
                    }
                    byte[] hash = new byte[20];
                    Array.Copy(rawData, index, hash, 0, 20);
                    result[identifier] = hash;
                    index = index + 20;
                    stringStart = index;
                }
                else
                {
                    index++;
                }
            }
            //for(int index = 0; index < rawData.Length;index++)
            //{

            //    // if reaches the end of the null-terminated string
            //    if(rawData[index] == 0)
            //    {
            //        LoginIdentifier identifier = new LoginIdentifier(Encoding.UTF8.GetString(rawData, stringStart, (index) - stringStart));
            //        index++;
            //        if(index+20 >= rawData.Length)
            //        {
            //            throw new IndexOutOfRangeException("Expected hash after identifier, but not enough space");
            //        }
            //        byte[] hash = new byte[20];
            //        Array.Copy(rawData, index, hash, 0, 20);
            //        result[identifier] = hash;
            //        index = index + 20;
            //        stringStart = index;
            //    }


            //}
            return result;
        }


        public static HashSet<SHA1Hash> DeserializeHashesFromNetwork(Stream stream)
        {
            HashSet<SHA1Hash> hashes = new HashSet<SHA1Hash>();
            // read data length from the input
            byte[] dataLength = new byte[4];
            stream.Read(dataLength);
            // reverse array if little endian, before conversion to int
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataLength);
            byte[] data = new byte[BitConverter.ToInt32(dataLength)];
            // if the length of the data isnt perfectly divisible by 20, something is wrong, as each SHA1 hash is exacly 20 bytes
            if(data.Length % 20  != 0)
            {
                throw new InvalidDataException("Expected 20 byte hashes, but the number of bytes is not divisible by 20");
            }
            int bytesRead = stream.Read(data);
            if (bytesRead != data.Length)
            {
                throw new InvalidDataException($"Expected {data.Length} bytes of data, but only {bytesRead} could be read");
            }
            for (int i = 0;i < data.Length;i = i + 20)
            {
                byte[] hash = new byte[20];
                Array.Copy(data,i, hash, 0, 20);
                hashes.Add(new SHA1Hash(hash));
            }

            return hashes;
        }





        public static List<byte[]> DeserializeWordBytesFromNetwork(Stream stream)
        {
            List<byte[]> words = new List<byte[]>();
            // read data length from the input
            byte[] dataLength = new byte[4];
            stream.Read(dataLength);
            // reverse array if little endian, before conversion to int
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataLength);
            byte[] data = new byte[BitConverter.ToInt32(dataLength)];
            int bytesRead = stream.Read(data);
            if (bytesRead != data.Length)
            {
                throw new InvalidDataException($"Expected {data.Length} bytes of data, but only {bytesRead} could be read");
            } 
            int stringStart = 0;
            for(int i = 0; i < data.Length;i++)
            {
                // if its a null byte, split out the word into the bytearray list
                if (data[i] == 0x0)
                {
                    byte[] word = new byte[i - stringStart];
                    Array.Copy(data,stringStart,word, 0, word.Length);
                    words.Add(word);
                    stringStart = i + 1;
                }
            }
            
            return words;
        }

        public static List<string> DeserializeWordsFromNetwork(Stream stream)
        {
            List<string> words = new List<string>();
            // read data length from the input
            byte[] dataLength = new byte[4];
            stream.Read(dataLength);
            // reverse array if little endian, before conversion to int
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataLength);
            byte[] data = new byte[BitConverter.ToInt32(dataLength)];
            int bytesRead = stream.Read(data);
            if (bytesRead != data.Length)
            {
                throw new InvalidDataException($"Expected {data.Length} bytes of data, but only {bytesRead} could be read");
            }
            int stringStart = 0;
            for (int i = 0; i < data.Length; i++)
            {
                // if its a null byte, split out the word into the bytearray list
                if (data[i] == 0x0)
                {
                    byte[] word = new byte[i - stringStart];
                    Array.Copy(data, stringStart, word, 0, word.Length);
                    words.Add(Encoding.UTF8.GetString(word));
                    stringStart = i + 1;
                }
            }

            return words;
        }

        public static byte[] SerializeCrackedPasswordsToNetwork(IDictionary<SHA1Hash, string> input)
        {
            List<byte> result = new List<byte>(5)
            {
                1, 0, 0, 0,0 // allocate storage for flags and length
            };
            foreach (var kvp in input)
            {
                result.AddRange(kvp.Key.Hash); // add hash
                result.AddRange(Encoding.UTF8.GetBytes(kvp.Value)); // add plaintext password
                result.Add(0); // null byte to terminate the string
            }
            int length = result.Count - 5;
            //put connectionState and length bytes at the start
            result[0] = (byte)ServerConnectionStateFlag.SendCrackedPasswords; // flag
            result[1] = (byte)((length) >> 24);

            result[2] = (byte)((length) >> 16);

            result[3] = (byte)((length) >> 8);

            result[4] = (byte)((length) /*>> 0*/);

            return result.ToArray();
        }




    }
}

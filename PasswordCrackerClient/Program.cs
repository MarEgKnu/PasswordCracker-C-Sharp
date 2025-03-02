using PasswordCrackerClient;
using System.Text;
ClientConnectionStateFlag c = (ClientConnectionStateFlag)10;

CrackerClient client = new CrackerClient("localhost", 5665, 1048576, 1048576, 10);
client.Start();

//byte[] words = Encoding.UTF8.GetBytes("Haha\0Hoho\0LOL\0xd\0");
//List<byte> words2 = new List<byte>(4) {0,0,0,0x11 };
//words2.AddRange(words);

//byte[] data = Encoding.UTF8.GetBytes("\0\0\0(aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
//MemoryStream c = new MemoryStream();
//c.Write(words2.ToArray());
//c.Seek(0, SeekOrigin.Begin);
//NetworkSerializer.DeserializeWordBytesFromNetwork(c);

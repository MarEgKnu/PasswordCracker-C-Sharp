﻿using PasswordCrackerServer;
using PasswordCrackerServer.Models;
using System.Text;
//Convert.ToBase64String(SHA1.HashData(Encoding.UTF8.GetBytes(("Password"))));
CrackerServer server = new CrackerServer("./serverConfig.xml");
server.Start();
//byte[] words = Encoding.UTF8.GetBytes("AADFGHBVFGTGHYHJUKIOPassword123\0AADFGHBVFGTGHYHJUKICpassword123\0");
//List<byte> words2 = new List<byte>(4) { 0, 0, 0, 0x40 };
//words2.AddRange(words);
//MemoryStream c = new MemoryStream();
//c.Write(words2.ToArray());
//c.Seek(0, SeekOrigin.Begin);
//NetworkSerializer.DeserializeCrackedPasswordsFromNetwork(c);
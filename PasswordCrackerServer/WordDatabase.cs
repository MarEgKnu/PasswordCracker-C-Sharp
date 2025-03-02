using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    public class WordDatabase
    {
        public WordDatabase(string path)
        {
            LoadByteData(path);
            LoadStringData(path);
        }
        public WordDatabase()
        {
            _words = new string[0];
            _wordsAsBytes = new List<byte[]>();
        }
        string[] _words;
        List<byte[]> _wordsAsBytes;

        public List<byte[]> GetAllWordBytes()
        {
            return _wordsAsBytes;   
        }
        public string[] GetAllWords()
        {
            return _words;
        }
        public void LoadDictionary(string path)
        {
            LoadByteData(path);
            LoadStringData(path);
        }
        private void LoadByteData(string path)
        {
            _wordsAsBytes.AddRange(FileSerializer.GetWordsBytesFromFile(path));
        }
        private void LoadStringData(string path)
        {
            _words = _words.ConcatArray(FileSerializer.GetWordsFromFile(path));
        }

    }
}

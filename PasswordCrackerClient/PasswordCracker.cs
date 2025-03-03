using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerClient
{
    public class PasswordCracker
    {
        public ImmutableHashSet<SHA1Hash> Hashes { get; set; }
        public PasswordCracker(HashSet<SHA1Hash> hashes)
        {
            Hashes = ImmutableHashSet.CreateRange(hashes);
            result = new Dictionary<SHA1Hash, string>();
        }
        private Dictionary<SHA1Hash, string> result;
        public IDictionary<SHA1Hash, string> ProcessWords(IEnumerable<string> words, int noOfThreads)
        {
            result.Clear();
            //foreach (var word in words)
            //{
            //    CheckMultipleVariations(word);
            //}
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = noOfThreads };
            Parallel.ForEach(words, parallelOptions, word =>
            {
                CheckMultipleVariations(word);


            });
            return result;
        }
        private void CheckMultipleVariations(string word, SkipWordVariations skips = SkipWordVariations.SkipNone)
        {
            if(!skips.HasFlag(SkipWordVariations.SkipItself))
            {
                CheckSingleVariation(word);
            } 
            if(!skips.HasFlag(SkipWordVariations.SkipUpper))
            {
                CheckMultipleVariations(word.ToUpper(), skips | SkipWordVariations.SkipUpper | SkipWordVariations.SkipLower | SkipWordVariations.SkipCapitalizeStartLetters);
            }
            if(!skips.HasFlag(SkipWordVariations.SkipLower))
            {
                CheckMultipleVariations(word.ToLower(), skips | SkipWordVariations.SkipUpper | SkipWordVariations.SkipLower | SkipWordVariations.SkipCapitalizeStartLetters);
            }
            if(!skips.HasFlag(SkipWordVariations.SkipReverse))
            {
                CheckMultipleVariations(word.Reverse(), skips | SkipWordVariations.SkipReverse);

            }
            if(!skips.HasFlag(SkipWordVariations.SkipAddDigitsEnd))
            {
                for(int i = 0; i < 5;i++)
                {
                    CheckMultipleVariations(string.Concat(word, i), skips | SkipWordVariations.SkipAddDigitsEnd);
                }
                CheckMultipleVariations(string.Concat(word, "1"), skips | SkipWordVariations.SkipAddDigitsEnd);
                CheckMultipleVariations(string.Concat(word, "12"), skips | SkipWordVariations.SkipAddDigitsEnd);
                CheckMultipleVariations(string.Concat(word, "123"), skips | SkipWordVariations.SkipAddDigitsEnd);
                CheckMultipleVariations(string.Concat(word, "1234"), skips | SkipWordVariations.SkipAddDigitsEnd);
                //CheckNumberedVariantsFront(word);
            }    
            if(!skips.HasFlag(SkipWordVariations.SkipAddDigitsBeginning))
            {
                for (int i = 0; i < 5; i++)
                {
                    CheckMultipleVariations(string.Concat(word, i), skips | SkipWordVariations.SkipAddDigitsBeginning);
                }
                CheckMultipleVariations(string.Concat("1", word), skips | SkipWordVariations.SkipAddDigitsBeginning);
                CheckMultipleVariations(string.Concat("12", word), skips | SkipWordVariations.SkipAddDigitsBeginning);
                CheckMultipleVariations(string.Concat("123", word), skips | SkipWordVariations.SkipAddDigitsBeginning);
                CheckMultipleVariations(string.Concat("1234", word), skips | SkipWordVariations.SkipAddDigitsBeginning);
                //CheckNumberedVariantsBack(word);
            }
            if(!skips.HasFlag(SkipWordVariations.SkipCapitalizeStartLetters))
            {
                CheckMultipleVariations(word.CapitalizeLettersFromStart(3), skips | SkipWordVariations.SkipCapitalizeStartLetters | SkipWordVariations.SkipUpper | SkipWordVariations.SkipLower);
            }

        }
        private void CheckNumberedVariantsFront(string word)
        {
            // could expand this to include all numbers from 0000-9999 or so
            for(int i = 0; i < 10; i++)
            {
                CheckSingleVariation(string.Concat(word, i.ToString()));
            }
            CheckSingleVariation(string.Concat(word, "12"));
            CheckSingleVariation(string.Concat(word, "123") );
            CheckSingleVariation(string.Concat(word, "1234"));
            CheckSingleVariation(string.Concat(word, "12345"));
            CheckSingleVariation(string.Concat(word, "123456"));
            CheckSingleVariation(string.Concat(word, "1234567"));
            CheckSingleVariation(string.Concat(word, "12345678"));
            CheckSingleVariation(string.Concat(word, "123456789"));


        }
        private void CheckNumberedVariantsBack(string word)
        {
            // could expand this to include all numbers from 0000-9999 or so
            for (int i = 0; i < 10; i++)
            {
                CheckSingleVariation(string.Concat(i.ToString(), word));
            }
            CheckSingleVariation(string.Concat("12",word));
            CheckSingleVariation(string.Concat("123", word));
            CheckSingleVariation(string.Concat("1234",word));
            CheckSingleVariation(string.Concat("12345",word));
            CheckSingleVariation(string.Concat("123456",word));
            CheckSingleVariation(string.Concat("1234567", word));
            CheckSingleVariation(string.Concat("12345678", word));
            CheckSingleVariation(string.Concat("123456789", word));


        }
        private void CapitalizeLettersFromStart(string word, SHA1Hash buffer, int count)
        {
            StringBuilder stringBuilder = new StringBuilder(word);
            for(int i = 0; i < word.Length && i < count; i++)
            {
                stringBuilder.Insert(i, Char.ToUpper(word[i]));
            }
        }
        private void CheckSingleVariation(string word)
        {
            SHA1Hash sha1 = new SHA1Hash(SHA1.HashData(Encoding.UTF8.GetBytes(word)));
            
            if (Hashes.Contains(sha1))
            {
                lock (result) 
                {
                    result[sha1] = word;
                }                 
            }
                    
        }
    }
}

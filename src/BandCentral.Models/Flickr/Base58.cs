using System;

namespace BandCentral.Models.Flickr
{
    public class Base58
    {
        public static string ShortenImageUrl(uint photoID)
        {
            const string root = "flic.kr/p/";

            return $"https://{root}{EncodeBase58(photoID)}";
        }

        private static string EncodeBase58(uint photoID)
        {
            const string sBase58Alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
            var sConverted = "";
            var iAlphabetLength = sBase58Alphabet.Length;

            while (photoID > 0)
            {
                long lNumberRemainder = (photoID % iAlphabetLength);
                photoID = Convert.ToUInt32(photoID / iAlphabetLength);
                sConverted = sBase58Alphabet[Convert.ToInt32(lNumberRemainder)] + sConverted;
            }

            return sConverted;
        }

        private static long DecodeBase58(string base58StringToExpand)
        {
            const string sBase58Alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
            long lConverted = 0;
            long lTemporaryNumberConverter = 1;

            while (base58StringToExpand.Length > 0)
            {
                var sCurrentCharacter = base58StringToExpand.Substring(base58StringToExpand.Length - 1);
                lConverted += (lTemporaryNumberConverter * sBase58Alphabet.IndexOf(sCurrentCharacter, StringComparison.Ordinal));
                lTemporaryNumberConverter *= sBase58Alphabet.Length;
                base58StringToExpand = base58StringToExpand.Substring(0, base58StringToExpand.Length - 1);
            }

            return lConverted;
        }
    }
}

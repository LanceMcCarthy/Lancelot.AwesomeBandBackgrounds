using System;

//namespace BandCentralBase.Common
//{
//    public class Base58
//    {
//        public static string ShortenImageUrl(UInt32 photoID)
//        {
//            var root = "flic.kr/p/";

//            return string.Format("https://{0}{1}", root, EncodeBase58(photoID));
//        }

//        private static String EncodeBase58(UInt32 photoID)
//        {
//            var sBase58Alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
//            String sConverted = "";
//            Int32 iAlphabetLength = sBase58Alphabet.Length;

//            while(photoID > 0)
//            {
//                long lNumberRemainder = (photoID % iAlphabetLength);
//                photoID = Convert.ToUInt32(photoID / iAlphabetLength);
//                sConverted = sBase58Alphabet[Convert.ToInt32(lNumberRemainder)] + sConverted;
//            }

//            return sConverted;
//        }

//        private static long DecodeBase58(String base58StringToExpand)
//        {
//            var sBase58Alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
//            long lConverted = 0;
//            long lTemporaryNumberConverter = 1;

//            while(base58StringToExpand.Length > 0)
//            {
//                String sCurrentCharacter = base58StringToExpand.Substring(base58StringToExpand.Length - 1);
//                lConverted = lConverted + (lTemporaryNumberConverter * sBase58Alphabet.IndexOf(sCurrentCharacter, StringComparison.Ordinal));
//                lTemporaryNumberConverter = lTemporaryNumberConverter * sBase58Alphabet.Length;
//                base58StringToExpand = base58StringToExpand.Substring(0, base58StringToExpand.Length - 1);
//            }

//            return lConverted;
//        }
//    }
//}

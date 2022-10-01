// Lance McCarthy 2013-2023 MIT
// Free to use, maintain attribution to original
// https://github.com/LanceMcCarthy/Lancelot.AwesomeBandBackgrounds

using System.Collections.Generic;

namespace BandCentral.Uwp.Controls.CustomColorPicker
{
    public static class Palettes
    {
        public static SwatchColor GrayRow = new SwatchColor(new[] { "FFFFFF", "F2F2F2", "D8D8D8", "BFBFBF", "A5A5A5", "7F7F7F" });
        public static SwatchColor BlackRow = new SwatchColor(new[] { "000000", "7F7F7F", "595959", "3F3F3F", "262626", "0C0C0C" });
        public static SwatchColor Row = new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" });
        
        //--------------swatch template------------------/
        //public static Swatch sw = new Swatch(
        //    "Test",
        //    new[] { "", "", "", "" },
        //    new List<SwatchColor>
        //    {
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    new SwatchColor(new[] { "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF", "FFFFFF" }),
        //    GrayRow,
        //    BlackRow
        //});
        //------------------------------------------------/

        public static readonly Swatch ApexSwatch = new Swatch(
            "Apex",
            new[] { "635672", "EAE2C0", "DDD9E2", "746425" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "C9C2D1", "F4F2F5", "E9E6EC", "DEDAE3", "9688A5", "635672" }),
                new SwatchColor(new[] { "69676D", "E0E0E2", "C2C1C5", "A4A3A8", "4E4D51", "343336" }),
                new SwatchColor(new[] { "CEB966", "F5F1E0", "EBE3C1", "E1D5A3", "E1D5A3", "746425" }),
                new SwatchColor(new[] { "9CB084", "EBEFE6", "D7DFCD", "C3CFB5", "758C5A", "4E5D3C" }),
                new SwatchColor(new[] { "6BB1C9", "E1EFF4", "C3DFE9", "A6D0DE", "3D8DA9", "295E70" }),
                new SwatchColor(new[] { "6585CF", "E0E6F5", "C1CEEB", "A2B5E2", "365BB0", "243C75" }),
                new SwatchColor(new[] { "7E6BC9", "E5E1F4", "CBC3E9", "B1A6DE", "533DA9", "372970" }),
                new SwatchColor(new[] { "A379BB", "ECE4F1", "DAC9E3", "C7AED6", "7D4D99", "533366" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch HardcoverSwatch = new Swatch(
            "Hardcover",
            new[] { "D3735E", "FAC6BB", "EFCFC8", "F25838" }, new List<SwatchColor>()
            {
                new SwatchColor(new[] { "ECE9C6", "E1DCA5", "D0C974", "A29A36", "514D1B", "201E0A" }),
                new SwatchColor(new[] { "895D1D", "F2E0C6", "E6C28D", "DAA454", "664515", "442E0E" }),
                new SwatchColor(new[] { "873624", "F0D0C9", "E2A293", "D4735E", "65281A", "431B11" }),
                new SwatchColor(new[] { "D6862D", "F6E6D5", "EECEAA", "E6B681", "A2641F", "6C4315" }),
                new SwatchColor(new[] { "D0BE40", "F5F2D8", "ECE5B2", "E2D88C", "A39428", "6D621A" }),
                new SwatchColor(new[] { "877F6C", "E7E5E1", "CFCCC3", "B7B2A5", "655F50", "433F35" }),
                new SwatchColor(new[] { "972109", "FBC7BC", "F78F7A", "F35838", "711806", "4B1004" }),
                new SwatchColor(new[] { "AEB795", "EEF0E9", "DEE2D4", "CED3BF", "879464", "5A6243" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch MetroSwatch = new Swatch(
            "Metro", new[] { "5EA126", "FDD36B", "CAEBAF", "FDB70A" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "D6ECFF", "A7D6FF", "60B5FF", "007DEA", "003E75", "00192E" }),
                new SwatchColor(new[] { "4E5B6F", "D9DDE4", "B3BCCA", "8D9BAF", "3A4453", "272D37" }),
                new SwatchColor(new[] { "7FD13B", "E5F5D7", "CBECB0", "B2E389", "5EA226", "3F6C19" }),
                new SwatchColor(new[] { "EA157A", "FAD0E4", "F6A1C9", "F272AF", "AF0F5B", "750A3D" }),
                new SwatchColor(new[] { "FEB80A", "FEF0CD", "FEE29C", "FED46B", "C58C00", "835D00" }),
                new SwatchColor(new[] { "00ADDC", "C5F2FF", "8BE6FF", "51D9FF", "0081A5", "00566E" }),
                new SwatchColor(new[] { "738AC8", "E2E7F4", "C7D0E9", "AAB8DE", "425EA9", "2C3F71" }),
                new SwatchColor(new[] { "1AB39F", "C9F7F1", "94EFE3", "5FE7D5", "138677", "0C594F" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch ModuleSwatch = new Swatch(
            "Module",
            new[] { "E78551", "9ED1DF", "F0B595", "60B4CB" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "D4D4D6", "BEBEC1", "9D9DA2", "67676D", "333336", "141415" }),
                new SwatchColor(new[] { "5A6378", "DCDFE5", "BABFCB", "989FB1", "434A59", "2D313C" }),
                new SwatchColor(new[] { "F0AD00", "FFEFC9", "FFE093", "FFD15D", "B48100", "785600" }),
                new SwatchColor(new[] { "60B5CC", "DFF0F4", "BFE1EA", "9FD2E0", "3691AA", "246171" }),
                new SwatchColor(new[] { "E66C7D", "FAE1E4", "F5C4CB", "F0A6B1", "D8243D", "901829" }),
                new SwatchColor(new[] { "6BB76D", "E1F0E1", "C3E2C4", "A6D3A7", "479249", "2F6130" }),
                new SwatchColor(new[] { "E88651", "FAE6DC", "F5CEB9", "F1B696", "CF5A1B", "8A3C12" }),
                new SwatchColor(new[] { "C64847", "F3DADA", "E8B5B5", "DC9190", "9A302F", "66201F" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch OfficeSwatch = new Swatch(
            "Office",
            new[] { "F69546", "D89593", "F9BF8E", "BF504D" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "EEECE1", "DDD9C3", "C4BD97", "938953", "494429", "1D1B10" }),
                new SwatchColor(new[] { "1F497D", "C6D9F0", "8DB3E2", "548DD4", "17365D", "0F243E" }),
                new SwatchColor(new[] { "4F81BD", "DBE5F1", "B8CCE4", "95B3D7", "366092", "244061" }),
                new SwatchColor(new[] { "C0504D", "F2DCDB", "E5B9B7", "D99694", "953734", "632423" }),
                new SwatchColor(new[] { "9BBB59", "EBF1DD", "D7E3BC", "C3D69B", "76923C", "4F6128" }),
                new SwatchColor(new[] { "8064A2", "E5E0EC", "CCC1D9", "B2A2C7", "5F497A", "3F3151" }),
                new SwatchColor(new[] { "4BACC6", "DBEEF3", "B7DDE8", "92CDDC", "31859B", "205867" }),
                new SwatchColor(new[] { "F79646", "FDEADA", "FBD5B5", "FAC08F", "E36C09", "974806" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch PaperSwatch = new Swatch(
            "Paper",
            new[] { "DECD04", "FAEE59", "C2B4D8", "71539F" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "FEFAC9", "FDF59C", "FBEF59", "DFCE04", "6F6702", "2C2900" }),
                new SwatchColor(new[] { "444D26", "DEE4CA", "BECA95", "9EB060", "33391C", "222613" }),
                new SwatchColor(new[] { "A5B592", "EDF0E9", "DBE1D3", "C9D2BD", "7C9263", "536142" }),
                new SwatchColor(new[] { "F3A447", "FCECDA", "FADAB5", "F7C890", "DD7E0E", "935409" }),
                new SwatchColor(new[] { "E7BC29", "FAF1D4", "F5E4A9", "F0D67E", "B79214", "7A610D" }),
                new SwatchColor(new[] { "D092A7", "F5E9ED", "ECD3DB", "E2BDCA", "B55475", "7B354D" }),
                new SwatchColor(new[] { "9C85C0", "EBE6F2", "D7CEE5", "C3B5D9", "7153A0", "4B376B" }),
                new SwatchColor(new[] { "809EC2", "E5EBF2", "CCD8E6", "B2C4DA", "4E74A3", "344D6C" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch PushpinSwatch = new Swatch(
            "Pushpin",
            new[] { "5C650C", "DEEA69", "FCC57A", "D57B01" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "CCDDEA", "ACC8DD", "7EA9CA", "3F739B", "1F394D", "0C171F" }),
                new SwatchColor(new[] { "465E9C", "D7DDED", "B0BCDB", "899BCA", "344675", "232F4E" }),
                new SwatchColor(new[] { "FDA023", "FEECD2", "FED9A7", "FDC67A", "D67B01", "8E5201" }),
                new SwatchColor(new[] { "AA2B1E", "F5CECA", "EC9D95", "E36C60", "7F2016", "55150E" }),
                new SwatchColor(new[] { "71685C", "E3E0DD", "C8C2BB", "ACA49A", "544D44", "38342E" }),
                new SwatchColor(new[] { "64A73B", "DFF0D5", "BFE1AB", "9FD281", "4A7D2C", "31531D" }),
                new SwatchColor(new[] { "EB5605", "FDDCCA", "FCB995", "FB9760", "B04003", "752B02" }),
                new SwatchColor(new[] { "B9CA1A", "F4F8CD", "E9F29B", "DFEB69", "8A9713", "5C650C" }),
                GrayRow,
                BlackRow
            });
        public static readonly Swatch Solstice = new Swatch(
            "Solstice",
            new[] { "3890A6", "DF7B7C", "BEBEBE", "000000" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "E7DEC9", "D9CBAB", "C5B07E", "957C42", "4A3E21", "1D180D" }),
                new SwatchColor(new[] { "4F271C", "E9CBC3", "D49887", "BF654C", "3B1D14", "27130D" }),
                new SwatchColor(new[] { "3891A7", "D4EAF0", "A9D6E2", "7EC2D3", "296C7D", "1B4853" }),
                new SwatchColor(new[] { "FEB80A", "FEF0CD", "FEE29C", "FED46B", "C58C00", "835D00" }),
                new SwatchColor(new[] { "C32D2E", "F4D3D3", "EAA7A7", "E07B7C", "922122", "611617" }),
                new SwatchColor(new[] { "84AA33", "E7F1D2", "D0E4A6", "B9D679", "637F26", "425519" }),
                new SwatchColor(new[] { "964305", "FCD6BA", "FAAE75", "F88630", "703203", "4B2102" }),
                new SwatchColor(new[] { "475A8D", "D7DCEB", "AFBAD7", "8898C3", "354369", "232D46" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch UrbanSwatch = new Swatch(
            "Urban",
            new[] { "78397A", "9192BC", "68452D", "9192BC" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "DEDEDE", "C7C7C7", "A6A6A6", "6F6F6F", "373737", "161616" }),
                new SwatchColor(new[] { "424456", "D6D7DF", "ADAFC0", "8588A1", "313340", "21222B" }),
                new SwatchColor(new[] { "53548A", "DADBE9", "B6B7D3", "9293BD", "3E3F67", "292945" }),
                new SwatchColor(new[] { "438086", "D5E8EA", "ACD2D5", "83BBC1", "326064", "214043" }),
                new SwatchColor(new[] { "A04DA3", "EDDAED", "DBB5DC", "C990CB", "78397A", "502651" }),
                new SwatchColor(new[] { "C4652D", "F4DFD3", "EAC0A7", "E0A17B", "934B21", "623216" }),
                new SwatchColor(new[] { "8B5D3D", "ECDDD3", "D9BCA8", "C69B7D", "68452D", "452E1E" }),
                new SwatchColor(new[] { "5C92B5", "DEE9F0", "BDD3E1", "9DBDD2", "3F6E8C", "2A495D" }),
                GrayRow,
                BlackRow
            });

        public static readonly Swatch WaveformSwatch = new Swatch(
            "Waveform",
            new[] { "2861A8", "8EB4E3", "9AD4F8", "31B5FC" },
            new List<SwatchColor>
            {
                new SwatchColor(new[] { "C6E7FC", "9BD5F9", "5BBAF6", "0B87D5", "05436A", "05436A" }),
                new SwatchColor(new[] { "073E87", "B9D5FB", "73ACF7", "2D82F4", "052E65", "031E43" }),
                new SwatchColor(new[] { "31B6FD", "D5F0FE", "ACE1FE", "83D3FD", "0293E0", "016295" }),
                new SwatchColor(new[] { "4584D3", "D9E6F6", "B4CDED", "8FB5E4", "2861A9", "1A4171" }),
                new SwatchColor(new[] { "5BD078", "DEF5E4", "BDECC8", "9CE2AD", "31AE50", "217435" }),
                new SwatchColor(new[] { "A5D028", "EDF6D3", "EDF6D3", "CAE57B", "7B9C1D", "526813" }),
                new SwatchColor(new[] { "F5C040", "FDF2D8", "FBE5B2", "F9D98C", "DC9F0B", "926A07" }),
                new SwatchColor(new[] { "05E0DB", "C8FDFC", "91FCFA", "5AFBF7", "03A7A4", "02706D" }),
                GrayRow,
                BlackRow
            });
    }
}
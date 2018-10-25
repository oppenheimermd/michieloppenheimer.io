using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Blog.Models.Helpers
{
    public static class PostHelpers
    {
        public static string CreateSlug(string title, int maxLength = 0)
        {
            // Return empty value if text is null
            if (title == null) return "";

            var normalizedString = title
                // Make lowercase
                .ToLowerInvariant()
                // Normalize the text
                .Normalize(NormalizationForm.FormD);

            //  Remove reserved Url Characters
            normalizedString = RemoveReservedUrlCharacters(normalizedString);

            var stringBuilder = new StringBuilder();
            var stringLength = normalizedString.Length;
            var prevdash = false;
            var trueLength = 0;
            char c;

            for (int i = 0; i < stringLength; i++)
            {
                c = normalizedString[i];
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    // Check if the character is a letter or a digit if the character is a
                    // international character remap it to an ascii valid character
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (c < 128)
                            stringBuilder.Append(c);
                        else
                            stringBuilder.Append(ConstHelper.RemapInternationalCharToAscii(c));
                        prevdash = false;
                        trueLength = stringBuilder.Length;
                        break;
                    // Check if the character is to be replaced by a hyphen but only if the last character wasn't
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.OtherPunctuation:
                    case UnicodeCategory.MathSymbol:
                        if (!prevdash)
                        {
                            stringBuilder.Append('-');
                            prevdash = true;
                            trueLength = stringBuilder.Length;
                        }
                        break;
                    case UnicodeCategory.ClosePunctuation:
                        break;
                    case UnicodeCategory.Control:
                        break;
                    case UnicodeCategory.CurrencySymbol:
                        break;
                    case UnicodeCategory.EnclosingMark:
                        break;
                    case UnicodeCategory.FinalQuotePunctuation:
                        break;
                    case UnicodeCategory.Format:
                        break;
                    case UnicodeCategory.InitialQuotePunctuation:
                        break;
                    case UnicodeCategory.LetterNumber:
                        break;
                    case UnicodeCategory.LineSeparator:
                        break;
                    case UnicodeCategory.ModifierLetter:
                        break;
                    case UnicodeCategory.ModifierSymbol:
                        break;
                    case UnicodeCategory.NonSpacingMark:
                        break;
                    case UnicodeCategory.OpenPunctuation:
                        break;
                    case UnicodeCategory.OtherLetter:
                        break;
                    case UnicodeCategory.OtherNotAssigned:
                        break;
                    case UnicodeCategory.OtherNumber:
                        break;
                    case UnicodeCategory.OtherSymbol:
                        break;
                    case UnicodeCategory.ParagraphSeparator:
                        break;
                    case UnicodeCategory.PrivateUse:
                        break;
                    case UnicodeCategory.SpacingCombiningMark:
                        break;
                    case UnicodeCategory.Surrogate:
                        break;
                    case UnicodeCategory.TitlecaseLetter:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // If we are at max length, stop parsing
                if (maxLength > 0 && trueLength >= maxLength)
                    break;
            }

            // Trim excess hyphens
            var result = stringBuilder.ToString().Trim('-');

            // Remove any excess character to meet maxlength criteria
            return maxLength <= 0 || result.Length <= maxLength ? result : result.Substring(0, maxLength);
        }

        static string RemoveReservedUrlCharacters(string text)
        {
            var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

            foreach (var chr in reservedCharacters)
            {
                text = text.Replace(chr, "");
            }

            return text;
        }

        public static string FormatHeading(string heading)
        {
            // Creates a TextInfo based on the "en-US" culture.
            var text = new CultureInfo("en-GB", false).TextInfo;

            // Changes a string to title case.
            return text.ToTitleCase(heading);
        }

        //  See:    http://www.chinhdo.com/20080515/chunking/
        /// <summary>
        /// Break a <see cref="List{T}"/> into multiple chunks. The <paramref name="list="/> is cleared out and the 
        /// items are moved into the returned chunks.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be chunked.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>A list of chunks.</returns>
        public static List<List<T>> BreakIntoChunks<T>(List<T> list, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("chunkSize must be greater than 0.");
            }

            var retVal = new List<List<T>>();

            while (list.Count > 0)
            {
                var count = list.Count > chunkSize ? chunkSize : list.Count;
                retVal.Add(list.GetRange(0, count));
                list.RemoveRange(0, count);
            }

            return retVal;
        }

        public static string ToTitleCase(string input)
        {
            var textInfo = new CultureInfo("en-GB", false).TextInfo;
            return textInfo.ToTitleCase(input);
        }

        public static string ShortenAndFormatText(string val, int maxlen)
        {
            const string postFix = "...";
            //var postFixLength = postFix.Length;

            if (string.IsNullOrEmpty(val)) return val;
            //var maxLength = (val.Length - maxlen);

            if (val.Length > maxlen)
            {
                var truncateFirst = Truncate(val, (maxlen - postFix.Length));
                var truncateLast = truncateFirst + postFix;
                return truncateLast;
            }
            else
            {
                return val;
            }
        }

        /// <summary>
        /// Truncate a string to a set size. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        /// public static string Truncate(this string value, int maxLength)
        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    public static class ConstHelper
    {
        /// <summary>
        /// Remaps international characters to ascii compatible ones
        /// based of: https://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        /// </summary>
        /// <param name="c">Charcter to remap</param>
        /// <returns>Remapped character</returns>
        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }
    }

}

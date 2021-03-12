using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JoyLib.Code.Helpers
{
    public static class FileNameExtractor
    {
        public static string ExtractName(string pathRef, int extensionLength = 2)
        {
            List<char> chars = new List<char>();
            int j = pathRef.Length - 1;
            while (true)
            {
                //Search backwards through the string to find out if it's a letter or a dot
                if (char.IsLetterOrDigit(pathRef[j]) || pathRef[j] == '.')
                    chars.Add(pathRef[j]);
                else
                    break;

                j -= 1;
            }

            StringBuilder builder = new StringBuilder();
            for (int k = chars.Count - 1; k > extensionLength; k--)
            {
                builder.Append(chars[k]);
            }
            string className = builder.ToString();

            return className;
        }
    }
}

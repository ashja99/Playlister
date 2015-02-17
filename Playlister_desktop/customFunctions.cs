using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Playlister_desktop
{
    public class customFunctions
    {
        public static bool isNumeric(string input)
        {
            int v;
            return Int32.TryParse(input.Trim(), out v);
        }

        public static string GetHash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public struct myParamArray { 

            public string type;
            public string scope;
            public string comparison;
            public string playcount;
            public string playedby;
            public string[] tags;

            public myParamArray(string t, string s, string c, string pc, string pb, string[] ts) {
                type = t;
                scope = s;
                comparison = c;
                playcount = pc;
                playedby = pb;
                tags = ts;
            }

        }

    }
}

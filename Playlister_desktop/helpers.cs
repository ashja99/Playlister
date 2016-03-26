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
    public class helpers
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

        public static string getSig(string core)
        {
            MD5 getMd5 = MD5.Create();
            return GetHash(getMd5, core + "e40049bcff4ef495115924cb5a6fce76");
        }

        public struct myParamArray { 

            public string type;
            public bool? scope;
            public string comparison;
            public int playcount;
            public string playedby;
            public string[] tags;

            public myParamArray(string t, string s, string c, string pc, string pb, string[] ts, bool? useS, bool? useP, bool? useT)
            {
                type = t;
                if (useS.HasValue && useS.Value)
                {
                    if (s == "is") scope = true;
                    else scope = false;
                }
                else scope = null;
                if (useP.HasValue && useP.Value)
                {
                    comparison = c;
                    playcount = Int32.Parse(pc);
                    playedby = pb;
                }
                else
                {
                    comparison = null;
                    playcount = 0;
                    playedby = null;
                }

                if (useT.HasValue && useT.Value) tags = ts;
                else tags = null;
            }

        }

    }
}

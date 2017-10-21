using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Playlister
{

    public enum compare { exact, less, greater, none}

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
            return GetHash(getMd5, core + Properties.Settings.Default.apiSecret);
        }

        public struct myParamArray { 

            public string type;
            public bool? scope;
            public compare compMode;
            public int playcount;
            public string playedby;
            public List<myLastFm.Tag> tags;

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
                    this.compMode = compare.none;

                    switch(c){
                        case "exactly": this.compMode = compare.exact;
                            break;
                        case "less than": this.compMode = compare.less;
                            break;
                        case "more than": this.compMode = compare.greater;
                            break;
                    }

                    playcount = Int32.Parse(pc);
                    playedby = pb;
                }
                else
                {
                    compMode = compare.none;
                    playcount = 0;
                    playedby = null;
                }

                if (useT.HasValue && useT.Value)
                {
                    tags = new List<myLastFm.Tag>();
                    foreach (string tg in ts) {
                        tags.Add(new myLastFm.Tag(tg));
                    }
                }
                else tags = null;

            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Playlister_desktop
{
    public static class myLastFm
    {
        public static XmlDocument lastFmReq(string reqString)
        {
            Console.WriteLine("Request String: " + reqString);
            WebRequest lastfm = WebRequest.Create(reqString);
            lastfm.Method = "GET";
            // Get the response.
            try
            {
                HttpWebResponse response = (HttpWebResponse)lastfm.GetResponse();
                // Display the status.
                Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content. 
                string apiResponse = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine(apiResponse);
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();

                // Parse string as xml
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(apiResponse);

                return xDoc;
            }
            catch (WebException webExcp)
            {
                // If you reach this point, an exception has been caught.
                Console.WriteLine("A WebException has been caught.");
                // Write out the WebException message.
                Console.WriteLine(webExcp.ToString());
                // Get the WebException status code.
                WebExceptionStatus status = webExcp.Status;
                // If status is WebExceptionStatus.ProtocolError, 
                //   there has been a protocol error and a WebResponse 
                //   should exist. Display the protocol error.
                if (status == WebExceptionStatus.ProtocolError)
                {
                    Console.Write("The server returned protocol error ");
                    // Get HttpWebResponse so that you can check the HTTP status code.
                    HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                    Console.WriteLine((int)httpResponse.StatusCode + " - "
                       + httpResponse.StatusCode);
                }

                return null;
            }

        }


    }
}

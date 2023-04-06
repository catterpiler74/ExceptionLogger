using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Utilities
{
  /// <summary>Logs errors via a HTTP POST to a webpage</summary>
  public class WebsiteLogger : LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public override void LogError(string error)
    {
      if (string.IsNullOrEmpty(Url))
        throw new ArgumentException("Url has not been set");
      if (string.IsNullOrEmpty(QueryString))
        throw new ArgumentException("QueryString has not been set");

      var uri = new Uri(Url);
      var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
      httpWebRequest.Method = "POST";
      httpWebRequest.ContentType = "application/x-www-form-urlencoded";

      var encoding = Encoding.Default;

      var parameters = string.Format(QueryString, HttpUtility.UrlEncode(error));

      // get length of request (may well be a better way to do this)
      var memStream = new MemoryStream();
      var streamWriter = new StreamWriter(memStream, encoding);
      streamWriter.Write(parameters);
      streamWriter.Flush();
      httpWebRequest.ContentLength = memStream.Length;
      streamWriter.Close();

      var stream = httpWebRequest.GetRequestStream();
      streamWriter = new StreamWriter(stream, encoding);
      streamWriter.Write(parameters);
      streamWriter.Close();

      using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
      using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
      {
        streamReader.ReadToEnd();
      }
    }

    /// <summary>
    /// Gets or sets the URL that will be used when posting an error to a website.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the format of the query string that will be used when posting an error to a website. 
    /// e.g error={0}
    /// </summary>
    public string QueryString { get; set; }
  }
}

using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Utilities
{
  /// <summary>
  /// This logger will log to a text file called 'BugReport.txt'
  /// </summary>
  public class TextFileLogger : LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public override void LogError(string error)
    {
      var filename = Path.GetDirectoryName(Application.ExecutablePath);
      filename += "\\BugReport.txt";

      var data = new List<string>();

      lock (this)
      {
        if (File.Exists(filename))
        {
          using (var reader = new StreamReader(filename))
          {
            string line;
            do
            {
              line = reader.ReadLine();
              data.Add(line);
            }
            while (line != null);
          }
        }

        // truncate the file if it's too long
        var writeStart = 0;
        if (data.Count > 500)
          writeStart = data.Count - 500;

        using (var stream = new StreamWriter(filename, false))
        {
          for (var i = writeStart; i < data.Count; i++)
          {
            stream.WriteLine(data[i]);
          }

          stream.Write(error);
        }
      }
    }
  }
}

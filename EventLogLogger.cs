﻿using System.Diagnostics;
using System.Reflection;

namespace Utilities
{
  /// <summary>Logs errors to the application event log</summary>
  public sealed class EventLogLogger : LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public override void LogError(string error)
    {
      var log = new EventLog("Application");
      log.Source = Assembly.GetExecutingAssembly().ToString();
      log.WriteEntry(error, EventLogEntryType.Error);
    }
  }
}

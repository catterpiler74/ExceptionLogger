﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Utilities
{
  /// <summary>Enumerated type that defines how users will be notified of exceptions</summary>
  public enum NotificationType
  {
    /// <summary>Users will not be notified, exceptions will be automatically logged to the registered loggers</summary>
    Silent,
    /// <summary>Users will be notified an exception has occurred, exceptions will be automatically logged to the registered loggers</summary>
    Inform,
    /// <summary>Users will be notified an exception has occurred and will be asked if they want the exception logged</summary>
    Ask
  }

  /// <summary>
  /// Abstract class for logging errors to different output devices, primarily for use in Windows Forms applications
  /// </summary>
  public abstract class LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public abstract void LogError(string error);
  }

  /// <summary>
  /// Class to log unhandled exceptions
  /// </summary>
  public sealed class ExceptionLogger
  {
    /// <summary>
    /// Creates a new instance of the ExceptionLogger class
    /// </summary>
    public ExceptionLogger()
    {
      Application.ThreadException += OnThreadException;
      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
      loggers = new List<LoggerImplementation>();
    }

    private readonly List<LoggerImplementation> loggers;
    /// <summary>
    /// Adds a logger implementation to the list of used loggers.
    /// </summary>
    /// <param name="logger">The logger to add.</param>
    public void AddLogger(LoggerImplementation logger)
    {
      loggers.Add(logger);
    }

    /// <summary>
    /// Gets or sets the type of the notification shown to the end user.
    /// </summary>
    public NotificationType NotificationType { get; set; } = NotificationType.Ask;

    delegate void LogExceptionDelegate(Exception e);
    private void HandleException(Exception e)
    {
      switch (NotificationType)
      {
        case NotificationType.Ask :
          if (MessageBox.Show("An unexpected error occurred - " + e.Message +
          ". Do you wish to log the error?", "Error", MessageBoxButtons.YesNo) == DialogResult.No)
            return;
          break;
        case NotificationType.Inform :
          MessageBox.Show("An unexpected error occurred - " + e.Message);
          break;
        case NotificationType.Silent :
          break;
      }
      
      Task.Run(() =>
      {
        LogException(e);
      });
    }

    // Event handler that will be called when an unhandled
    // exception is caught
    private void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
      // Log the exception to a file
      HandleException(e.Exception);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      HandleException((Exception)e.ExceptionObject);
    }

    private static string GetExceptionTypeStack(Exception e)
    {
      if (e.InnerException != null)
      {
        var message = new StringBuilder();
        message.AppendLine(GetExceptionTypeStack(e.InnerException));
        message.AppendLine("   " + e.GetType().ToString());
        return (message.ToString());
      }

      return "   " + e.GetType().ToString();
    }

    private static string GetExceptionMessageStack(Exception e)
    {
      if (e.InnerException != null)
      {
        var message = new StringBuilder();
        message.AppendLine(GetExceptionMessageStack(e.InnerException));
        message.AppendLine("   " + e.Message);
        return (message.ToString());
      }

      return "   " + e.Message;
    }

    private static string GetExceptionCallStack(Exception e)
    {
      if (e.InnerException != null)
      {
        var message = new StringBuilder();
        message.AppendLine(GetExceptionCallStack(e.InnerException));
        message.AppendLine("--- Next Call Stack:");
        message.AppendLine(e.StackTrace);
        return (message.ToString());
      }

      return e.StackTrace;
    }

    private static TimeSpan GetSystemUpTime()
    {
      var upTime = new PerformanceCounter("System", "System Up Time");
      upTime.NextValue();
      return TimeSpan.FromSeconds(upTime.NextValue());
    }

    // use to get memory available
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX 
    { 
      public uint dwLength; 
      public uint dwMemoryLoad; 
      public ulong ullTotalPhys; 
      public ulong ullAvailPhys; 
      public ulong ullTotalPageFile; 
      public ulong ullAvailPageFile; 
      public ulong ullTotalVirtual; 
      public ulong ullAvailVirtual; 
      public ulong ullAvailExtendedVirtual; 
      
      public MEMORYSTATUSEX() 
      { 
        dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); 
      } 
    }
    
    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    /// <summary>writes exception details to the registered loggers</summary>
    /// <param name="exception">The exception to log.</param>
    public void LogException(Exception exception)
    {
      var error = new StringBuilder();

      error.AppendLine("Application:       " + Application.ProductName);
      error.AppendLine("Version:           " + Application.ProductVersion);
      error.AppendLine("Date:              " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
      error.AppendLine("Computer name:     " + SystemInformation.ComputerName);
      error.AppendLine("User name:         " + SystemInformation.UserName);
      error.AppendLine("OS:                " + Environment.OSVersion.ToString());
      error.AppendLine("Culture:           " + CultureInfo.CurrentCulture.Name);
      error.AppendLine("Resolution:        " + SystemInformation.PrimaryMonitorSize.ToString());
      error.AppendLine("System up time:    " + GetSystemUpTime());
      error.AppendLine("App up time:       " +
        (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString());

      var memStatus = new MEMORYSTATUSEX(); 
      if (GlobalMemoryStatusEx(memStatus)) 
      {
        error.AppendLine("Total memory:      " + memStatus.ullTotalPhys / (1024 * 1024) + "Mb");
        error.AppendLine("Available memory:  " + memStatus.ullAvailPhys / (1024 * 1024) + "Mb");
      }

      error.AppendLine("");

      error.AppendLine("Exception classes:   ");
      error.Append(GetExceptionTypeStack(exception));
      error.AppendLine("");
      error.AppendLine("Exception messages: ");
      error.Append(GetExceptionMessageStack(exception));

      error.AppendLine("");
      error.AppendLine("Stack Traces:");
      error.Append(GetExceptionCallStack(exception));
      error.AppendLine("");
      error.AppendLine("Loaded Modules:");
      var thisProcess = Process.GetCurrentProcess();
      foreach (ProcessModule module in thisProcess.Modules)
      {
        error.AppendLine(module.FileName + " " + module.FileVersionInfo.FileVersion);
      }

      for (var i = 0; i < loggers.Count; i++)
      {
        loggers[i].LogError(error.ToString());
      }
    }
  }
}



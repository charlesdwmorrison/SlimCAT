using System;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// A Logging class implementing the Singleton pattern and an internal Queue to be flushed perdiodically.
/// </summary>
public class LogWriter
{
    private static LogWriter instance;
    private static Queue<Log> logQueue;
    private static string logDir = "C:\\log\\";
    private static string logFile = "ResponseLog.log";
    private static int maxLogAge = int.Parse("1");
    private static int queueSize = int.Parse("1");
    private static DateTime LastFlushed = DateTime.Now;
    string logPath = logDir + DateTime.Now.Year + "-" +
     DateTime.Now.Month + "-" +
     DateTime.Now.Day + "-" +
     DateTime.Now.Hour + "_" +
     DateTime.Now.Minute + "_" +
     logFile; //

    /// <summary>
    /// Private constructor to prevent instance creation
    /// </summary>
    private LogWriter() { }

    /// <summary>
    /// An LogWriter instance that exposes a single instance
    /// </summary>
    public static LogWriter Instance
    {
        get
        {
            // If the instance is null then create one and init the Queue
            if (instance == null)
            {
                instance = new LogWriter();
                logQueue = new Queue<Log>();
            }
            return instance;
        }
    }

    /// <summary>
    /// The single instance method that writes to the log file
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    public void WriteToLog(string message)
    {

        // set up the path and filename
        if (!Directory.Exists(logDir)) System.IO.Directory.CreateDirectory(logDir);

        // Lock the queue while writing to prevent contention for the log file
        lock (logQueue)
        {
            // Create the entry and push to the Queue
            Log logEntry = new Log(message);
            logQueue.Enqueue(logEntry);

            // If we have reached the Queue Size then flush the Queue
            if (logQueue.Count >= queueSize || DoPeriodicFlush())
            {
                FlushLog();
            }
        }
    }

    private bool DoPeriodicFlush()
    {
        TimeSpan logAge = DateTime.Now - LastFlushed;
        if (logAge.TotalSeconds >= maxLogAge)
        {
            LastFlushed = DateTime.Now;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Flushes the Queue to the physical log file
    /// </summary>
    private void FlushLog()
    {
        while (logQueue.Count > 0)
        {
            Log entry = logQueue.Dequeue();


            // This could be optimised to prevent opening and closing the file for each write
            using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter log = new StreamWriter(fs))
                {
                    log.WriteLine(string.Format("{0}{1}", entry.LogTime, entry.Message));
                }
            }
        }
    }
}

/// <summary>
/// A Log class to store the message and the Date and Time the log entry was created
/// </summary>
public class Log
{
    public string Message { get; set; }
    public string LogTime { get; set; }
    public string LogDate { get; set; }

    public Log(string message)
    {
        Message = message;
        LogDate = DateTime.Now.ToString("yyyy-MM-dd");
        LogTime = DateTime.Now.ToString("hh:mm:ss.fff tt") + ":";
    }
}
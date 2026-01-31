namespace ConsoleFixer.Listeners;

using BepInEx.Logging;
using plog.Models;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary> Listens to any BepInEx logs and redirects them to PLog. </summary>
public class BepRedirector : ILogListener
{
    /// <summary> Cached loggers. </summary>
    public Dictionary<string, plog.Logger> Loggers = [];

    /// <summary> this method is invoked when ever a log is well uh logged so redirect it pls ty :3 </summary>
    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        // if it uses some schizo log type or if its unity (which is already handled by the game), dont redirect it
        if (eventArgs.Source.SourceName == "Unity Log" || !bepToPlog.TryGetValue(eventArgs.Level, out Level plogLvl))
            return;

        Record(eventArgs.Source.SourceName, eventArgs.Data.ToString(), plogLvl);
    }

    /// <summary> Records a log, using the cached loggers. </summary>
    public void Record(string name, string message, Level logLvl)
    {
        plog.Logger Logger;
        if (Loggers.TryGetValue(name, out var tryResult))
            Logger = tryResult;
        else
        {
            Logger = new(name);
            Loggers.Add(name, Logger);
        }

        Logger.Record(message, logLvl, stackTrace: StackTraceUtility.ExtractFormattedStackTrace(new StackTrace(5, true)));
    }

    /// <summary> IDisposable is forcing me to implement this so :P </summary>
    public void Dispose()
    {
    }

    /// <summary> Convert BepInEx.Logging.LogLevel to plog.Models.Level. </summary>
    public static Dictionary<LogLevel, Level> bepToPlog = new()
    {
        [LogLevel.None] = Level.Off,
        [LogLevel.Fatal] = Level.Exception,
        [LogLevel.Error] = Level.Error,
        [LogLevel.Warning] = Level.Warning,
        [LogLevel.Info] = Level.Info,
        [LogLevel.Debug] = Level.Debug,
    };
}

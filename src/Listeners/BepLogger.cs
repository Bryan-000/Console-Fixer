namespace ConsoleFixer.Listeners;

using BepInEx.Logging;
using plog.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

/// <summary> Listens to any BepInEx logs and also logs them to PLog. </summary>
public class BepLogger : ILogListener
{
    /// <summary> Cached loggers. </summary>
    public Dictionary<string, plog.Logger> Loggers = [];

    /// <summary> this method is invoked when ever a log is well uh logged so redirect it pls ty :3 </summary>
    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        // if its filtered mod or it uses some schizo log type, dont redirect it
        if (Plugin.FilterBepInEx.Value.Split(", ").Contains(eventArgs.Source.SourceName) || !bepToPlog.TryGetValue(eventArgs.Level, out Level plogLvl))
            return;

        Record(eventArgs.Source.SourceName, eventArgs.Data.ToString(), plogLvl);
    }

    /// <summary> Records a log, using the cached loggers. </summary>
    public void Record(string name, string message, Level logLvl)
    {
        if (!Loggers.TryGetValue(name, out plog.Logger Logger))
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

namespace ConsoleFixer.Listeners;

using BepInEx.Logging;
using plog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

/// <summary> Listens to any BepInEx logs and also logs them to PLog. </summary>
public class BepLogger : ILogListener
{
    /// <summary> Cached loggers. </summary>
    public Dictionary<string, PLogger> Loggers = [];

    /// <summary> This method is invoked whenever a log is well... logged. Redirect it please :3 </summary>
    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        // If its filtered mod or it uses some schizo log type, don't redirect it
        if (bepToPlog.TryGetValue(eventArgs.Level, out Level plogLvl) && !Settings.FilterBepInEx_Value.Contains(eventArgs.Source.SourceName))
            Record(eventArgs.Source.SourceName, eventArgs.Data.ToString(), plogLvl);
    }

    /// <summary> Records a log using the cached loggers. </summary>
    public void Record(string name, string message, Level logLvl)
    {
        if (!Loggers.TryGetValue(name, out PLogger Logger))
        {
            Logger = new(name);
            Loggers.Add(name, Logger);
        }

        Logger.Record(message, logLvl, stackTrace: StackTraceUtility.ExtractFormattedStackTrace(new StackTrace(5, true)));
    }

    /// <summary> IDisposable is forcing me to implement this so :P </summary>
    public void Dispose()
    {
        Loggers.Clear();
        Loggers = null;
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
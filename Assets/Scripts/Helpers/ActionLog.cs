﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public class ActionLog
    {
        protected Queue<LogEntry> m_Queue = new Queue<LogEntry>();

        protected const string FILENAME = "player.log";

        protected readonly bool IsEditor = Engine.EditorHint;
        protected StreamWriter Writer { get; set; }

        public event LogEntryHandler TextAdded;

        public ActionLog()
        {
            this.OpenLog();
        }

        public void Update()
        {
            if (this.Writer is null)
            {
                return;
            }
            this.ServiceQueue();
        }

        public bool OpenLog()
        {
            try
            {
                File.Delete(FILENAME);
                this.Writer = new StreamWriter(FILENAME);
                this.m_Queue = new Queue<LogEntry>();
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr("COULD NOT START LOG PROCESS");
                GD.PrintErr(ex.Message);
                GD.PrintErr(ex.StackTrace);
                return false;
            }
        }

        protected void ServiceQueue()
        {
            while (this.m_Queue.Count > 0)
            {
                LogEntry logEntry = this.m_Queue.Dequeue();
                this.TextAdded?.Invoke(logEntry.m_Data, logEntry.m_LogLevel);
                this.Writer?.WriteLine(logEntry);
            }
            this.Writer?.Flush();
        }

        public void Flush()
        {
            this.ServiceQueue();
        }

        public void Log(object objectToPrint, LogLevel logLevel = LogLevel.Information)
        {
            string toPrint = "";
            if (objectToPrint is ICollection collection)
            {
                toPrint += this.CollectionWalk(collection);
            }
            else if (objectToPrint is Node node)
            {
                toPrint += node.Name;
            }
            else
            {
                toPrint += objectToPrint;
            }

            LogEntry entry = new LogEntry
            {
                m_Data = toPrint,
                m_LogLevel = logLevel
            };

            if (logLevel == LogLevel.Debug
                && this.IsEditor)
            {
                GD.Print(entry);
            }
            else if(logLevel != LogLevel.Debug)
            {
                GD.Print(entry);
            }
            
            switch (logLevel)
            {
                case LogLevel.Warning:
                    GD.PushWarning(entry.ToString());
                    break;

                case LogLevel.Error:
                    GD.PushError(entry.ToString());
                    break;
            }
            this.m_Queue.Enqueue(entry);
        }

        public void StackTrace(Exception exception)
        {
            while (true)
            {
                this.Log(exception.Message, LogLevel.Error);
                this.Log(exception.StackTrace, LogLevel.Error);

                Exception innerException = exception.InnerException;
                if (innerException is null == false)
                {
                    exception = innerException;
                    continue;
                }

                break;
            }
        }

        public void PrintCollection(ICollection collection)
        {
            this.Log(this.CollectionWalk(collection));
        }

        public string CollectionWalk(ICollection collection)
        {
            StringBuilder builder = new StringBuilder();
            foreach (object o in collection)
            {
                switch (o)
                {
                    case DictionaryEntry entry:
                        builder.AppendLine("[" + entry.Key + ": " + entry.Value + "]");
                        break;
                    case ICollection child:
                        builder.AppendLine("Contents:");
                        builder.AppendLine(this.CollectionWalk(child));
                        break;
                    case Node node:
                        builder.AppendLine(node.Name);
                        break;
                    
                    default:
                        builder.AppendLine(o.ToString());
                        break;
                }
            }

            return builder.ToString();
        }
    }

    public enum LogLevel
    {
        Debug,
        Information,
        Gameplay,
        Warning,
        Error
    }

    public class LogEntry
    {
        public string m_Data;
        public LogLevel m_LogLevel;

        public override string ToString()
        {
            return "[" + this.m_LogLevel + "]: " + this.m_Data;
        }
    }

    public delegate void LogEntryHandler(string textAdded, LogLevel logLevel);
}
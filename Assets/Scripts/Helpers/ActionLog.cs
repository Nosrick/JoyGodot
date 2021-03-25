using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using File = System.IO.File;

namespace JoyLib.Code.Helpers
{
    public class ActionLog : IDisposable
    {
        public List<string> History { get; protected set; }

        private Queue<LogEntry> m_Queue = new Queue<LogEntry>();

        public const int LINES_TO_KEEP = 10;

        private const string FILENAME = "player.log";

        private readonly bool IsEditor = Engine.EditorHint;

        private StreamWriter Writer { get; set; }

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
                this.History = new List<string>();
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

        public void LogAction(Entity actor, string actionString)
        {
            this.Log(actor.JoyName + " is " + actionString, LogLevel.Gameplay);
        }

        protected void ServiceQueue()
        {
            bool written = false;
            while (this.m_Queue.Count > 0)
            {
                this.Writer.WriteLine(this.m_Queue.Dequeue());
                written = true;
            }

            if (written)
            {
                this.Writer.Flush();
            }

            while (this.History.Count > LINES_TO_KEEP)
            {
                this.History.RemoveAt(0);
            }
        }

        public void Log(object objectToPrint, LogLevel logLevel = LogLevel.Information)
        {
            string toPrint = "[" + logLevel + "] ";
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
             
            GD.Print(toPrint);
            switch (logLevel)
            {
                case LogLevel.Warning:
                    GD.PushWarning(toPrint);
                    break;

                case LogLevel.Error:
                    GD.PushError(toPrint);
                    break;
            }

            LogEntry entry = new LogEntry
            {
                m_Data = toPrint,
                m_LogLevel = logLevel
            };
            this.m_Queue.Enqueue(entry);
            if (logLevel == LogLevel.Gameplay)
            {
                this.History.Add(toPrint);
            }
        }

        public void StackTrace(Exception exception)
        {
            this.Log(exception.Message, LogLevel.Error);
            this.Log(exception.StackTrace, LogLevel.Error);

            Exception innerException = exception.InnerException;
            if (innerException is null == false)
            {
                this.StackTrace(innerException);
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

        ~ActionLog()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Writer?.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Directory = System.IO.Directory;
using Expression = NCalc.Expression;
using File = System.IO.File;
using Object = Godot.Object;

namespace JoyGodot.Assets.Scripts.Scripting
{
    public class ScriptingEngine
    {
        protected Type[] m_Types;

        public ScriptingEngine()
        {
            this.m_Types = this.GetType().Assembly.GetExportedTypes();
        }

        public object FetchAndInitialise(string type)
        {
            try
            {
                Type directType =
                    this.m_Types.FirstOrDefault(t => t.Name.Equals(type, StringComparison.OrdinalIgnoreCase));

                if (directType is null)
                {
                    throw new Exception();
                }

                return Activator.CreateInstance(directType);
            }
            catch (Exception ex)
            {
                GlobalConstants.ActionLog.StackTrace(ex);
                GlobalConstants.ActionLog.Log("Error when searching for Type in ScriptingEngine, type name " + type,
                    LogLevel.Error);
                return default;
            }
        }

        public T Initialise<T>()
        {
            try
            {
                return Activator.CreateInstance<T>();
            }
            catch (Exception e)
            {
                GlobalConstants.ActionLog.StackTrace(e);
                GlobalConstants.ActionLog.Log("Error when activating type " + nameof(T), LogLevel.Error);
                return default;
            }
        }

        public IEnumerable<T> FetchAndInitialiseChildren<T>()
        {
            try
            {
                Type[] types = this.m_Types.Where(t => typeof(T).IsAssignableFrom(t) && t.IsAbstract == false)
                    .Distinct()
                    .ToArray();
                List<T> children = new List<T>();
                foreach (Type tempType in types)
                {
                    children.Add((T) Activator.CreateInstance(tempType));
                }

                return children;
            }
            catch (Exception e)
            {
                GlobalConstants.ActionLog.StackTrace(e);
                GlobalConstants.ActionLog.Log(
                    "Error when searching for Type in ScriptingEngine, type name " + nameof(T), LogLevel.Error);
                return default;
            }
        }

        public IEnumerable<Type> FetchTypeAndChildren(string typeName)
        {
            try
            {
                Type directType = this.m_Types.FirstOrDefault(type =>
                    type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

                List<Type> children = new List<Type>();
                if (directType is null == false)
                {
                    children = this.m_Types.Where(type => directType.IsAssignableFrom(type)).ToList();
                }
                else
                {
                    directType = Type.GetType(typeName, true, true);
                    children = this.m_Types.Where(type => directType.IsAssignableFrom(type)).ToList();
                    children = children.Where(t => t.IsAbstract == false && t.IsInterface == false).ToList();
                }

                return children;
            }
            catch (Exception ex)
            {
                GlobalConstants.ActionLog.StackTrace(ex);
                GlobalConstants.ActionLog.Log("Error when searching for Type in ScriptingEngine, type name " + typeName,
                    LogLevel.Error);
                return default;
            }
        }

        public IEnumerable<Type> FetchTypeAndChildren(Type type)
        {
            try
            {
                Type[] types = this.m_Types.Where(t => type.IsAssignableFrom(t) && t.IsAbstract == false).ToArray();

                return types;
            }
            catch (Exception e)
            {
                GlobalConstants.ActionLog.StackTrace(e);
                GlobalConstants.ActionLog.Log(
                    "Error when searching for Type in ScriptingEngine, type name " + nameof(type), LogLevel.Error);
                return default;
            }
        }

        public IJoyAction FetchAction(string actionName)
        {
            try
            {
                Type type = this.m_Types.First(t => t.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));

                IJoyAction action = (IJoyAction) Activator.CreateInstance(type);
                return action;
            }
            catch (Exception e)
            {
                GlobalConstants.ActionLog.StackTrace(e);
                GlobalConstants.ActionLog.Log("Error when finding action, no such action " + actionName,
                    LogLevel.Error);
                return default;
            }
        }

        public IEnumerable<IJoyAction> FetchActions(string[] actionNames)
        {
            List<IJoyAction> actions = new List<IJoyAction>();
            foreach (string name in actionNames)
            {
                actions.Add(this.FetchAction(name));
            }

            return actions;
        }

        public T Evaluate<T>(string code)
        {
            Expression expression = new Expression(code);
            return (T) Convert.ChangeType(expression.Evaluate(), typeof(T));
        }
    }

    public class JoyLibTypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            if (x is null || y is null)
            {
                return false;
            }

            bool namespaceEquals = x.Namespace is null || y.Namespace is null ? false : x.Namespace.Equals(y.Namespace);

            return x.Name.Equals(y.Name) && namespaceEquals;
        }

        public int GetHashCode(Type obj)
        {
            return (obj.Namespace + ":" + obj.Name).GetHashCode();
        }
    }
}
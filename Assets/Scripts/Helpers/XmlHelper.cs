using System;
using System.Xml.Linq;

namespace JoyLib.Code.Helpers
{
    public static class XmlHelper
    {
        public static T GetAs<T>(this XElement element, T defaultValue = default(T))
        {
            T returnValue = defaultValue;

            if(element != null && !string.IsNullOrEmpty(element.Value))
            {
                //Try to cast to return data type
                returnValue = (T)Convert.ChangeType(element.Value, typeof(T));
            }

            return returnValue;
        }

        public static T GetAs<T>(this XAttribute attribute, T defaultValue = default(T))
        {
            T returnValue = defaultValue;

            if(attribute != null && !string.IsNullOrEmpty(attribute.Value))
            {
                returnValue = (T)Convert.ChangeType(attribute.Value, typeof(T));
            }

            return returnValue;
        }

        public static T DefaultIfEmpty<T>(this XElement element, T defaultValue)
        {
            T returnValue = defaultValue;
            if(element != null && !string.IsNullOrEmpty(element.Value))
            {
                returnValue = (T)Convert.ChangeType(element.Value, typeof(T));
            }
            return returnValue;
        }

        public static T DefaultIfEmpty<T>(this XAttribute attribute, T defaultValue)
        {
            T returnValue = defaultValue;
            if(attribute != null && !string.IsNullOrEmpty(attribute.Value))
            {
                returnValue = (T)Convert.ChangeType(attribute.Value, typeof(T));
            }
            return returnValue;
        }
    }
}

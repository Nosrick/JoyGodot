using Sirenix.OdinSerializer;
using UnityEngine;

namespace JoyLib.Code.Unity.GUI.Managed_Assets
{
    public class ManagedElement : SerializedMonoBehaviour
    {
        [SerializeField] protected string m_ElementName;
        public string ElementName => this.m_ElementName;
        public bool Initialised { get; protected set; }
    }
}
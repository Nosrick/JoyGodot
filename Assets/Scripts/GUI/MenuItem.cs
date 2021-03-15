using Code.Unity.GUI.Managed_Assets;
using TMPro;
using UnityEngine;

namespace JoyLib.Code.Unity.GUI
{
    public class MenuItem : ManagedButton
    {
        [SerializeField] protected TextMeshProUGUI m_Text;

        public TextMeshProUGUI Text
        {
            get => this.m_Text;
            protected set => this.m_Text = value;
        }
    }
}
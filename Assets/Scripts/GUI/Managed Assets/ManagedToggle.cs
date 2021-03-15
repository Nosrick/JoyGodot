using JoyLib.Code.Unity.GUI.Managed_Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Unity.GUI.Managed_Assets
{
    public class ManagedToggle : ManagedElement
    {
        [SerializeField] protected Toggle m_Toggle;
        [SerializeField] protected TextMeshProUGUI m_Checkmark;

        public bool Checked
        {
            get => this.m_Checked;
            set
            {
                this.m_Checked = value;
                this.SetCheckmark();
            }
        }
        protected bool m_Checked;

        public void Awake()
        {
            this.m_Toggle.onValueChanged.RemoveAllListeners();
            this.m_Toggle.onValueChanged.AddListener(this.ValueChanged);
        }

        protected void ValueChanged(bool value)
        {
            this.Checked = value;
        }

        protected void SetCheckmark()
        {
            if (this.m_Checked)
            {
                this.m_Checkmark.text = "x";
            }
            else
            {
                this.m_Checkmark.text = "";
            }
        }
    }
}
using TMPro;
using UnityEngine;

namespace JoyLib.Code.Unity.GUI
{
    public class JoyEquipmentSlot : JoyItemSlot
    {
        [SerializeField] protected TextMeshProUGUI m_SlotName;

        public TextMeshProUGUI SlotName => this.m_SlotName;
    }
}
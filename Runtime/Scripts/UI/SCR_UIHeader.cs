using UnityEngine;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    public class UIHeader : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private TextMeshProUGUI descriptionText = null;
        public void Initialize(string description) => this.descriptionText.text = description;
    }
}
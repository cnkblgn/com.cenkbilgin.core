using UnityEngine;

namespace Core.Localization
{
    public class SCR_Test : MonoBehaviour
    {
        public LocalizedString testString;
        public LocalizedString testString2;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Debug.Log("testString: " + testString.Get());
            Debug.Log("testString2: " + testString2.Get());
        }
        private void OnEnable()
        {
            ManagerCoreLocalization.OnLocalizationChanged += OnLocalizationChanged;
        }

        private void OnLocalizationChanged(int obj)
        {
            throw new System.NotImplementedException();
        }

        private void OnDisable()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

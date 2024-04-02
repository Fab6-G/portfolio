using _StarkAPP.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _StarkAPP.Scripts.VRDeviceConection
{
    public class ConnectVRDevice : MonoBehaviour
    {
        [SerializeField] private CampaignManager campaignManager = default;
        [SerializeField] private GameObject connectPanel = default;
        [SerializeField] private GameObject pairedPanel = default;
        [SerializeField] private TMP_Dropdown caseInput = default;
        [SerializeField] private TMP_Dropdown deviceInput = default;
        [SerializeField] private UnityEvent pageResetEvents = default;

        [Header("Anchors")]
        [SerializeField]
        private GameObject caseInputAnchor;
        [SerializeField]
        private GameObject deviceInputAnchor;
        
        [Header("Dropdown Templates")]
        [SerializeField]
        private RectTransform caseInputTemplate;
        [SerializeField]
        private RectTransform deviceInputTemplate;
        
        [Header("Prefabs To Spawn")] [SerializeField]
        private GameObject caseInputPrefab;
        [SerializeField]
        private GameObject deviceInputPrefab;

        ObjectStateExtensions.ObjectStateListener connectPanelStateListener;

        public void Start()
        {
            campaignManager.OnPairDeviceSuccess += DevicePairedSuccessfully;
            
            // Attachs Object State Listener to Connection Pannel
            connectPanelStateListener =  (ObjectStateExtensions.ObjectStateListener) connectPanel.GetStateListener();
            connectPanelStateListener.Disabled += ReSetDropdown;
        }

        /// <summary>
        /// Destroys and sets up the new drop downs
        /// </summary>
        private void ReSetDropdown()
        {
            DestroyImmediate(caseInput.gameObject);
            DestroyImmediate(deviceInput.gameObject);
            
            caseInput = Instantiate(caseInputPrefab,caseInputAnchor.transform).GetComponent<TMP_Dropdown>();
            caseInput.template = caseInputTemplate;
            caseInput.itemText = caseInput.template.GetComponentInChildren<TMP_Text>(); 
                
            deviceInput = Instantiate(deviceInputPrefab,deviceInputAnchor.transform).GetComponent<TMP_Dropdown>();
            deviceInput.template = deviceInputTemplate;
            deviceInput.itemText = deviceInputTemplate.GetComponentInChildren<TMP_Text>(); 
        }
        

        public void OnDestroy()
        {
            campaignManager.OnPairDeviceSuccess -= DevicePairedSuccessfully;
            connectPanelStateListener.Disabled -= ReSetDropdown;
        }

        public void ConnectUserToDevice()
        {
            string deviceID = (caseInput.options[caseInput.value].text + ".0" + deviceInput.options[deviceInput.value].text).ToString();
            campaignManager.SetUserID(deviceID);
        }

        public void DevicePairedSuccessfully(string deviceid)
        {
            ScreenManager.Instance.GoToScreen(ScreenManager.ScreenNames.VRPaired);
            campaignManager.GetVRSeeionCompletionStatus();
        }

        public void OnPageReset()
        {
            pageResetEvents.Invoke();
        }
    }
}

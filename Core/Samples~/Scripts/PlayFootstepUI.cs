using MidniteOilSoftware.Core;
using UnityEngine;
using UnityEngine.UI;

public class PlayFootstepUI : MonoBehaviour
{
    [SerializeField] Button _footstepButton;
    
    void Start()
    {
        _footstepButton.onClick.AddListener(FootstepButtonClicked);        
    }
    
    void OnDisable()
    {
        _footstepButton.onClick.RemoveListener(FootstepButtonClicked);
    }

    void FootstepButtonClicked()
    {
        EventBus.Instance.Raise(new FootstepsEvent());
    }

}

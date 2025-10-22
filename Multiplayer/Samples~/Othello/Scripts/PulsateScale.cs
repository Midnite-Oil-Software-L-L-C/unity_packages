using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class PulsateScale : MonoBehaviour
    {
        [SerializeField] float _pulsateSpeed = 1f;
        [SerializeField] float _pulsateScale = 1.1f;
        
        Vector3 _startScale;
        Vector3 _endScale;
        float _t;

        void Start()
        {
            _startScale = transform.localScale;
            _endScale = _startScale * _pulsateScale;
            _t = 0f;
        }

        void Update()
        {
            _t += Time.deltaTime * _pulsateSpeed;
            transform.localScale = Vector3.Lerp(_startScale, _endScale, Mathf.PingPong(_t, 1));
        }
    }
}
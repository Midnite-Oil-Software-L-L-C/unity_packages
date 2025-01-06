using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Core
{
    public class ReversiBoard : NetworkBehaviour
    {
        [SerializeField] Vector3 _originPoint = new Vector3(-3.5f, 0.25f, -3.5f);
        [SerializeField] float _cellSize = 1.0f;
        [SerializeField] GameObject _cellPrefab;
        [SerializeField] GameObject _chipPrefab;

        readonly GameObject[,] _cellHover = new GameObject[8, 8];
        readonly GameObject[,] _chips = new GameObject[8, 8];

        void Awake()
        {
            Debug.Log($"_originPoint={_originPoint}");
            for (var x = 0; x < 8; x++)
            {
                for (var z = 0; z < 8; z++)
                {
                    var cell = Instantiate(_cellPrefab, transform);
                    cell.name = $"Cell {x},{z}";
                    var position = new Vector3(
                        _originPoint.x + x * _cellSize,
                        _originPoint.y,
                        _originPoint.z + z * _cellSize);
                    cell.transform.localPosition = position;
                    Debug.Log($"x={x} z={z}. position={position}, cell position = {cell.transform.position}");
//                    _cellHover[x, z].SetActive(false);
                }
            }
        }
    }
}

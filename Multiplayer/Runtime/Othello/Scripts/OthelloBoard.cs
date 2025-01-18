using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using EventBus = MidniteOilSoftware.Core.EventBus;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class OthelloBoard : NetworkBehaviour
    {
        [SerializeField] Vector3 _originPoint = new Vector3(-3.5f, 0.25f, -3.5f);
        [SerializeField] float _cellSize = 1.0f;
        [SerializeField] GameObject _cellPrefab;
        [SerializeField] GameObject _chipPrefab;

        public bool BoardIsFull
        {
            get
            {
                return _cells.Cast<Cell>().All(cell => cell.Chip != null);
            }
        }
        public int WhiteChips => _cells.Cast<Cell>().Count(cell => cell.Chip && cell.Chip.Color == ChipColor.White);
        public int BlackChips => _cells.Cast<Cell>().Count(cell => cell.Chip && cell.Chip.Color == ChipColor.Black);

        public void SetupBoardForStartOfGame()
        {
            PlaceInitialChips();
        }

        OthelloGameManager _gameManager;
        readonly Cell[,] _cells = new Cell[8,8];
        Chip _chipCursor;
        InputAction _pointerPosition;
        InputAction _leftClick;
        Cell _hoveredCell;
        
        #region Directions
        readonly (int x, int y) Up = (0, 1);
        readonly (int x, int y) Down = (0, -1);
        readonly (int x, int y) Left = (-1, 0);
        readonly (int x, int y) Right = (1, 0);
        readonly (int x, int y) UpLeft = (-1, 1);
        readonly (int x, int y) UpRight = (1, 1);
        readonly (int x, int y) DownLeft = (-1, -1);
        readonly (int x, int y) DownRight = (1, -1);
        #endregion Directions

        void Awake()
        {
            _gameManager = FindFirstObjectByType<OthelloGameManager>(FindObjectsInactive.Include);
            BuildBoard();
            InstantiateChipCursor();
            SetupInputActions();
        }

        void OnEnable()
        {
            _pointerPosition.performed += OnPointerMoved;
            _leftClick.performed += OnLeftClick;
        }

        void OnDisable()
        {
            _pointerPosition.performed -= OnPointerMoved;
            _leftClick.performed -= OnLeftClick;
            Destroy(_chipCursor.gameObject);
        }

        void BuildBoard()
        {
            for (var x = 0; x < 8; x++)
            {
                for (var z = 0; z < 8; z++)
                {
                    var cell = Instantiate(_cellPrefab, transform).GetComponent<Cell>();
                    cell.Initialize(x, z);
                    cell.transform.localPosition = new Vector3(
                        _originPoint.x + x * _cellSize,
                        _originPoint.y,
                        _originPoint.z + z * _cellSize);
                    _cells[x, z] = cell;
                }
            }
        }

        void PlaceInitialChips()
        {
            foreach(var cell in _cells)
            {
                cell.ClearChip();
            }
            var chip = Instantiate(_chipPrefab, _cells[3, 3].transform).GetComponent<Chip>();
            chip.SetColor(ChipColor.White);
            _cells[3, 3].DropChip(chip);
            chip = Instantiate(_chipPrefab, _cells[4, 4].transform).GetComponent<Chip>();
            chip.SetColor(ChipColor.White);
            _cells[4, 4].DropChip(chip);
            chip = Instantiate(_chipPrefab, _cells[3, 4].transform).GetComponent<Chip>();
            chip.SetColor(ChipColor.Black);
            _cells[3, 4].DropChip(chip);
            chip = Instantiate(_chipPrefab, _cells[4, 3].transform).GetComponent<Chip>();
            chip.SetColor(ChipColor.Black);
            _cells[4, 3].DropChip(chip);
        }

        void InstantiateChipCursor()
        {
            _chipCursor = Instantiate(_chipPrefab, transform).GetComponent<Chip>();
            _chipCursor.name = "Chip Cursor";
            _chipCursor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            _chipCursor.gameObject.SetActive(false);
        }

        void SetupInputActions()
        {
            var playerInput = new InputActionMap("UI");
            _pointerPosition = playerInput.AddAction("PointerPosition", binding: "<Pointer>/position");
            _leftClick = playerInput.AddAction("LeftClick", binding: "<Pointer>/press");
            playerInput.Enable();
        }

        #region Input actions

        void OnPointerMoved(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsLocalPlayerTurn)
            {
                _chipCursor.gameObject.SetActive(false);
                return;
            }
            var cell = GetCellUnderMouse(context);
            if (cell)
            {
                if (_hoveredCell == cell || cell.Chip) return;
                _hoveredCell = cell;
                _chipCursor.transform.localPosition = _hoveredCell.transform.localPosition + Vector3.up * 0.25f;
                _chipCursor.SetColor(_gameManager.LocalPlayerChipColor);
                _chipCursor.gameObject.SetActive(true);
                ShowSurroundedChips(cell.X, cell.Y, _gameManager.LocalPlayerChipColor);
                if (!IsLegalMove(cell.X, cell.Y, _gameManager.LocalPlayerChipColor))
                {
                    cell.Highlight(true, true);
                }
                return;
            }
            _chipCursor.gameObject.SetActive(false);
            ClearChipHighlights();
            _hoveredCell = null;
        }

        private void OnLeftClick(InputAction.CallbackContext context)
        {
            if (!_gameManager.IsLocalPlayerTurn) return;
            if (_hoveredCell == null ||
                !IsLegalMove(_hoveredCell.X, _hoveredCell.Y, _gameManager.LocalPlayerChipColor))
            {
                return;
            }

            DropChip();
        }

        private static readonly RaycastHit[] RaycastHits = new RaycastHit[10];

        Cell GetCellUnderMouse(InputAction.CallbackContext context)
        {
            var screenPosition = context.ReadValue<Vector2>();
            var ray = Camera.main?.ScreenPointToRay(screenPosition);
            if (ray == null) return null;

            var hitCount = Physics.RaycastNonAlloc(ray.Value, RaycastHits);
            for (var i = 0; i < hitCount; i++)
            {
                var hit = RaycastHits[i];
                if (hit.collider.TryGetComponent<Cell>(out var cell))
                {
                    return cell;
                }
            }
            return null;
        }

        #endregion Input actions


        #region RPCs

        [Rpc(SendTo.Server)]
        void DropChipServerRPC(int x, int y, ChipColor color)
        {
            Debug.Log($"{color} chip dropped at {x},{y} on server.");
            DropChipClientRPC(x, y, color);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void DropChipClientRPC(int x, int y, ChipColor chipColor)
        {
            Debug.Log($"{chipColor} chip dropped at {x},{y} on client.");
            if (chipColor != _gameManager.CurrentPlayerChipColor)
            {
                Debug.LogWarning($"DropChipClientRPC: Chip color {chipColor} does not match current player color {_gameManager.CurrentPlayerChipColor}");
            }
            var cell = _cells[x, y];
            var chip = Instantiate(_chipPrefab, cell.transform).GetComponent<Chip>();
            chip.SetColor(_gameManager.CurrentPlayerChipColor);
            cell.DropChip(chip);
            EventBus.Instance.Raise(new ChipDroppedEvent((int)chipColor));
            var surroundedChips = FindSurroundedChips(x, y, chipColor);
            foreach (var surroundedChip in surroundedChips)
            {
                surroundedChip.FlipChip();
            }
        }

        #endregion

        public bool HasLegalMove(ChipColor chipColor)
        {
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    if (IsLegalMove(x, y, chipColor))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool IsLegalMove(int x, int y, ChipColor chipColor)
        {
            if (_cells[x, y].Chip != null)
            {
                return false;
            }
            var surroundedChips = FindSurroundedChips(x, y, chipColor);
            return surroundedChips.Any();
        }

        List<Chip> FindSurroundedChips(int x, int y, ChipColor chipColor)
        {
            var surroundedChips = new List<Chip>();
            
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, Up, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, Down, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, Right, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, Left, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, UpRight, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, UpLeft, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, DownLeft, chipColor));
            surroundedChips.AddRange(FindSurroundedChipsInDirection(x, y, DownRight, chipColor));

            return surroundedChips;
        }

        IEnumerable<Chip> FindSurroundedChipsInDirection(int x, int y, (int x, int y) direction, 
            ChipColor chipColor)
        {
            var chips = new List<Chip>();
            var currentX = x + direction.x;
            var currentY = y + direction.y;
            while (currentX is >= 0 and < 8 && currentY is >= 0 and < 8)
            {
                var currentCell = _cells[currentX, currentY];
                if (currentCell.Chip == null)
                {
                    chips.Clear();
                    return chips;
                }

                if (currentCell.Chip.Color == chipColor)
                {
                    return chips;
                }

                chips.Add(currentCell.Chip);
                currentX += direction.x;
                currentY += direction.y;
            }
            // if we reached the edge without hitting a chip of our color
            // then we didn't find any surrounded chips
            chips.Clear();
            return chips;            
        }

        void ShowSurroundedChips(int x, int y, ChipColor chipColor)
        {
            var surroundedChips = FindSurroundedChips(x, y, chipColor);
            foreach (var cell in _cells)
            {
                if (cell.Chip && surroundedChips.Contains(cell.Chip))
                {
                    cell.Highlight(true);
                    continue;
                }

                cell.Highlight(false);
            }
        }
        
        void ClearChipHighlights()
        {
            foreach (var cell in _cells)
            {
                cell.Highlight(false);
            }
        }

        void DropChip()
        {
            _chipCursor.gameObject.SetActive(false);
            ClearChipHighlights();
            DropChipServerRPC(_hoveredCell.X, _hoveredCell.Y, _gameManager.LocalPlayerChipColor);
        }
        
    }
}

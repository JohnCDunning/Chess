using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ChessManager : MonoBehaviour
{
    public bool isPaused = false;
    public Team _Turn = Team.White;
    public float _SetupBoardDelay;
    public float _PieceSetupDelay;

    [SerializeField] private GameObject _Pawn;
    [SerializeField] private GameObject _Castle;
    [SerializeField] private GameObject _Knight;
    [SerializeField] private GameObject _Bishop;
    [SerializeField] private GameObject _Queen;
    [SerializeField] private GameObject _King;
    [SerializeField] private GameObject _BoardObject;
    [SerializeField] private Material Black, White;
    private Vector2[] _Chessboard = new Vector2[64];
    private ChessPlace[] _ChessPlaces = new ChessPlace[64];
    private ChessPlace _HoveredChessPlace;
    private ChessPlace _SelectedChessPlace;

    List<ChessPlace> moves = new List<ChessPlace>();
    private void Start()
    {
        StartCoroutine(SetupBoard());
    }
    private void Update()
    {
        //Handles selecting chess area
        if (Input.GetMouseButtonDown(0))
        {
            if (_HoveredChessPlace != null)
            {
                //Reset old selected
                if (_SelectedChessPlace != null)
                {
                    //This space is a potential move. Meaning that the selected piece will move there if all conditions are met
                    if(_HoveredChessPlace._State == State.hover)
                    {
                        //Moving The Piece, possibly killing another piece
                        StartCoroutine(MovePiece(_SelectedChessPlace._PieceOnPlace.gameObject, new Vector3(_HoveredChessPlace._Spot.x, _SelectedChessPlace._PieceOnPlace.transform.position.y, _HoveredChessPlace._Spot.y)));
                        if (_HoveredChessPlace._PieceOnPlace != null)
                        {
                            Destroy(_HoveredChessPlace._PieceOnPlace.gameObject);
                        }

                        _HoveredChessPlace._PieceOnPlace = _SelectedChessPlace._PieceOnPlace;
                        _HoveredChessPlace._CurrentPiece = _HoveredChessPlace._PieceOnPlace._PieceType;
                        _SelectedChessPlace._CurrentPiece = PieceType.None;



                        if (_Turn == Team.Black)
                        {
                            _Turn = Team.White;
                        }
                        else
                        {
                            _Turn = Team.Black;
                        }
                        _SelectedChessPlace.ResetMaterial();
                        _SelectedChessPlace._PieceOnPlace = null;
                        HighlightPlaces(false);
                    }
                    
                }
                //Select the new area
                //If its not your turn dont select anything
                if(_HoveredChessPlace._PieceOnPlace != null && _HoveredChessPlace._PieceOnPlace._Team == _Turn && isPaused == false)
                {
                    if(_SelectedChessPlace != null)
                        _SelectedChessPlace.ResetMaterial();
                    _SelectedChessPlace = _HoveredChessPlace;
                    _SelectedChessPlace.Select();
                    ShowPotentialMoves(_SelectedChessPlace);                   
                }
            }            
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit, 100))
        {
            _HoveredChessPlace = hit.collider.GetComponent<ChessPlace>();
        }
    }
    IEnumerator SetupBoard()
    {
        //Used to generate the array of positional data;
        int total = 0;
        bool _LastBlack = true;

        for (int x = 1; x <= 8; x++)
        {
            _LastBlack = !_LastBlack;
            for (int y = 1; y <= 8; y++)
            {
                
                _Chessboard[total] = new Vector2(x,y);

                ChessPlace boardPlace = SpawnObject(_BoardObject,_LastBlack, new Vector2(x, y)).GetComponent<ChessPlace>();
                boardPlace._Spot = _Chessboard[total];
                _ChessPlaces[total] = boardPlace;

                yield return new WaitForSeconds(_SetupBoardDelay);

                if(x == 2 || x == 7)
                    boardPlace._CurrentPiece = PieceType.Pawn;

                #region Set Starting Positions
                if (x == 1 && y == 1 || x == 1 && y == 8 || x == 8 && y == 1 || x == 8 && y == 8)
                    boardPlace._CurrentPiece = PieceType.Castle;
                if (x == 1 && y == 2 || x == 1 && y == 7 || x == 8 && y == 2 || x == 8 && y == 7)
                    boardPlace._CurrentPiece = PieceType.Knight;
                if (x == 1 && y == 3 || x == 1 && y == 6 || x == 8 && y == 3 || x == 8 && y == 6)
                    boardPlace._CurrentPiece = PieceType.Bishop;
                if (x == 1 && y == 4 || x == 8 && y == 5)
                    boardPlace._CurrentPiece = PieceType.Queen;
                if (x == 1 && y == 5 || x == 8 && y == 4)
                    boardPlace._CurrentPiece = PieceType.King;
                #endregion

                _LastBlack = !_LastBlack;
                total++;
            }
        }
        StartCoroutine(SetupPieces());
        yield return null;
    }
    IEnumerator SetupPieces()
    {
        foreach(ChessPlace place in _ChessPlaces)
        {
            if (place._Spot.x <= 2 || place._Spot.x >= 7)
            {
                yield return new WaitForSeconds(_PieceSetupDelay);
                bool isBlack = false;
                isBlack = place._Spot.x <= 2;

                #region Spawn In Game Pieces
                switch (place._CurrentPiece)
                {
                    case PieceType.None:
                        break;
                    case PieceType.Pawn:
                        place._PieceOnPlace = SpawnObject(_Pawn, isBlack, place._Spot).GetComponent<ChessPiece>();
                        //Sets the direction for the pawn piece depending on colour
                        if(isBlack == true)
                        {
                            place._PieceOnPlace.GetComponent<ChessPiece>()._PawnMovementDirection = -1;
                        }
                        else
                        {
                            place._PieceOnPlace.GetComponent<ChessPiece>()._PawnMovementDirection = 1;
                        }                        
                        break;
                    case PieceType.Castle:
                        place._PieceOnPlace = SpawnObject(_Castle, isBlack, place._Spot).GetComponent<ChessPiece>();
                        break;
                    case PieceType.Knight:
                        place._PieceOnPlace = SpawnObject(_Knight, isBlack, place._Spot).GetComponent<ChessPiece>();
                        break;
                    case PieceType.Bishop:
                        place._PieceOnPlace = SpawnObject(_Bishop, isBlack, place._Spot).GetComponent<ChessPiece>();
                        break;
                    case PieceType.Queen:
                        place._PieceOnPlace = SpawnObject(_Queen, isBlack, place._Spot).GetComponent<ChessPiece>();
                        break;
                    case PieceType.King:
                        place._PieceOnPlace = SpawnObject(_King, isBlack, place._Spot).GetComponent<ChessPiece>();
                        break;
                }
                #endregion

                if (isBlack)
                {
                    place._PieceOnPlace._Team = Team.Black;
                }
                else
                {
                    place._PieceOnPlace._Team = Team.White;
                }                
            }
        }
        yield return null;
    }
    GameObject SpawnObject(GameObject obj,bool isBlack, Vector2 position)
    {
        float offset = 0.23f;
        if (obj == _BoardObject)
        {
            offset = 0;
        }

        GameObject newObject = Instantiate(obj, new Vector3(position.x, offset, position.y), Quaternion.identity,transform);
        if (isBlack == true)
        {
            newObject.GetComponent<MeshRenderer>().material = Black;
        }
        else
        {
            newObject.GetComponent<MeshRenderer>().material = White;
        }
        if(obj == _BoardObject)
        {
            newObject.name = position.x + "," + position.y;
        }
        else 
        {
            newObject.name = obj.name;
        }
        

        return newObject;
    }

    void ShowPotentialMoves(ChessPlace place)
    {
        //Reset any spaces that happen to be highlighted already
        HighlightPlaces(false);
        moves.Clear();

        if (place._CurrentPiece != PieceType.None)
        {
            //Depending on what piece it is, itll run the logic for movement. Uses MoveInDirection();
            switch (place._CurrentPiece)
            {
                case PieceType.Pawn:
                    #region Pawn Movement Logic
                    //For moving forward or 2 moves if at starting position
                    for (int i = 1; i <= 2; i++)
                    {
                        ChessPlace forwardPosition = FindSpot(new Vector2(place._Spot.x - place._PieceOnPlace._PawnMovementDirection * i, place._Spot.y));
                        if (forwardPosition != null && forwardPosition._CurrentPiece == PieceType.None)
                        {
                            moves.Add(forwardPosition);
                        }
                        else
                        {
                            break;
                        }
                        if(place._Spot.x != 2 && place._Spot.x != 7)
                        {
                            break;
                        }
                    }
                    //Attack Moves
                    ChessPlace leftAttack = FindSpot(new Vector2(place._Spot.x - place._PieceOnPlace._PawnMovementDirection, place._Spot.y - 1));
                    if (leftAttack != null && leftAttack._CurrentPiece != PieceType.None && leftAttack._PieceOnPlace._Team != _SelectedChessPlace._PieceOnPlace._Team)
                    {
                        moves.Add(leftAttack);
                    }
                    ChessPlace rightAttack = FindSpot(new Vector2(place._Spot.x - place._PieceOnPlace._PawnMovementDirection, place._Spot.y + 1));
                    if (rightAttack != null && rightAttack._CurrentPiece != PieceType.None && rightAttack._PieceOnPlace._Team != _SelectedChessPlace._PieceOnPlace._Team)
                    {
                        moves.Add(rightAttack);
                    }
                    #endregion
                    break;
                case PieceType.Castle:
                    #region Castle Movement Logic
                    MoveInDirection(new Vector2(-1, 0), place);
                    MoveInDirection(new Vector2(1, 0), place);
                    MoveInDirection(new Vector2(0, -1), place);
                    MoveInDirection(new Vector2(0, 1), place);
                    #endregion
                    break;
                case PieceType.Knight:
                    #region Knight Movement Logic
                    
                    MoveInDirection(new Vector2(0, 0), place,2,1,2);
                    MoveInDirection(new Vector2(0, 0), place, 2, -1, 2);
                    MoveInDirection(new Vector2(0, 0), place, 2, 1, -2);
                    MoveInDirection(new Vector2(0, 0), place, 2, -1, -2);
                    MoveInDirection(new Vector2(0, 0), place, 2, 2, -1);
                    MoveInDirection(new Vector2(0, 0), place, 2, 2, 1);
                    MoveInDirection(new Vector2(0, 0), place, 2, -2, -1);
                    MoveInDirection(new Vector2(0, 0), place, 2, -2, 1);
                    #endregion
                    break;
                case PieceType.Bishop:
                    #region Bishop Movement Logic
                    MoveInDirection(new Vector2(-1, -1), place);
                    MoveInDirection(new Vector2(-1, 1), place);
                    MoveInDirection(new Vector2(1, -1), place);
                    MoveInDirection(new Vector2(1, 1), place);
                    #endregion
                    break;
                case PieceType.Queen:
                    #region Queen Movement Logic
                    MoveInDirection(new Vector2(-1, 0), place);
                    MoveInDirection(new Vector2(1, 0), place);
                    MoveInDirection(new Vector2(0, -1), place);
                    MoveInDirection(new Vector2(0, 1), place);

                    MoveInDirection(new Vector2(-1, -1), place);
                    MoveInDirection(new Vector2(-1, 1), place);
                    MoveInDirection(new Vector2(1, -1), place);
                    MoveInDirection(new Vector2(1, 1), place);
                    #endregion
                    break;
                case PieceType.King:
                    #region King Movement Logic
                    MoveInDirection(new Vector2(-1, 0), place,2);
                    MoveInDirection(new Vector2(1, 0), place, 2);
                    MoveInDirection(new Vector2(0, -1), place, 2);
                    MoveInDirection(new Vector2(0, 1), place, 2);

                    MoveInDirection(new Vector2(-1, -1), place, 2);
                    MoveInDirection(new Vector2(-1, 1), place, 2);
                    MoveInDirection(new Vector2(1, -1), place, 2);
                    MoveInDirection(new Vector2(1, 1), place, 2);
                    #endregion
                    break;
            }
        }
        HighlightPlaces(true);
    }
    //Handles where chess pieces can move to. Supply it with information about what the piece can do, and itll give back all positions possible
    void MoveInDirection(Vector2 Direction, ChessPlace chessPlace, int placesToMove = 8,int xoffset = 0, int yoffset = 0)
    {
        for (int i = 1; i < placesToMove; i++)
        {
            ChessPlace chessPosition = FindSpot(new Vector2(chessPlace._Spot.x + (float)xoffset + i * Direction.x, chessPlace._Spot.y + (float)yoffset + i * Direction.y));
            if (chessPosition != null)
            {
                if (chessPosition._CurrentPiece != PieceType.None)
                {
                    if (chessPosition._PieceOnPlace._Team != _SelectedChessPlace._PieceOnPlace._Team)
                    {
                        moves.Add(chessPosition);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    moves.Add(chessPosition);
                }                
            }
            else
            {
                break;
            }
        }
    }
    //Returns the chess board square from the vector
    ChessPlace FindSpot(Vector2 spot)
    {
        ChessPlace matchingSpot = null;
        foreach (ChessPlace place in _ChessPlaces)
        {
            if (place._Spot == spot)
            {
                matchingSpot = place;
                break;
            }
        }
        return matchingSpot;
    }
    void HighlightPlaces(bool isHighlighted)
    {
        foreach (ChessPlace place in moves)
        {
            if (isHighlighted == true)
            {
                place.Hover();
            }
            else
            {
                place.ResetMaterial();
                
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_Chessboard.Length > 1)
        {
            foreach (Vector2 v in _Chessboard)
            {
                Handles.Label(new Vector3(v.x, 0, v.y),v.x.ToString("f0") + "," + v.y.ToString("f0"));
            }
        }
    }

    IEnumerator MovePiece(GameObject _Piece, Vector3 Position)
    {
        float timeStarted = Time.time;
        float TimeToTake = 0.5f;
        isPaused = true;
        Vector3 originalPos = _Piece.transform.position;
        
        while (_Piece.transform.position.y <= (originalPos.y + 1))
        {
            float timeSinceStarted = Time.time - timeStarted;
            float percentageComplete = timeSinceStarted / TimeToTake;
            _Piece.transform.position = new Vector3(_Piece.transform.position.x, Mathf.SmoothStep(_Piece.transform.position.y, originalPos.y + 1f, percentageComplete), _Piece.transform.position.z);
        
            yield return new WaitForFixedUpdate();
        }
        timeStarted = Time.time;
        TimeToTake = 1f;
        while (_Piece.transform.position != new Vector3(Position.x, _Piece.transform.position.y, Position.z))
        {
            float timeSinceStarted = Time.time - timeStarted;
            float percentageComplete = timeSinceStarted / TimeToTake;
            _Piece.transform.position = new Vector3(Mathf.SmoothStep(_Piece.transform.position.x, Position.x, percentageComplete), _Piece.transform.position.y, Mathf.SmoothStep(_Piece.transform.position.z, Position.z, percentageComplete));
            yield return new WaitForFixedUpdate();
        }
        timeStarted = Time.time;
        TimeToTake = 0.5f;
        while (_Piece.transform.position.y != (Position.y))
        {
            float timeSinceStarted = Time.time - timeStarted;
            float percentageComplete = timeSinceStarted / TimeToTake;
            _Piece.transform.position = new Vector3(_Piece.transform.position.x, Mathf.SmoothStep(_Piece.transform.position.y, Position.y, percentageComplete), _Piece.transform.position.z);
            yield return new WaitForFixedUpdate();
        }
        isPaused = false;
        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    idle,
    selected,
    hover
}
public class ChessPlace : MonoBehaviour
{
    public PieceType _CurrentPiece = PieceType.None;
    public ChessPiece _PieceOnPlace;
    public Vector2 _Spot;

    private Material _DefaultMat;
    MeshRenderer mr;

    public Material _SelectedMat, _HoverMat;

    public bool isPaused = false;
    public State _State = State.idle;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        _DefaultMat = mr.material;
    }

    public void Select()
    {
        _State = State.selected;
        mr.material = _SelectedMat;
    }
    public void Hover()
    {
        _State = State.hover;
        mr.material = _HoverMat;
    }
    public void ResetMaterial()
    {
        _State = State.idle;
        mr.material = _DefaultMat;
    }
}

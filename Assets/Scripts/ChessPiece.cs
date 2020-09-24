using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    None,
    Pawn,
    Castle,
    Knight,
    Bishop,
    Queen,
    King
}
public enum Team
{
    None,
    White,
    Black
}
public class ChessPiece : MonoBehaviour
{
    public PieceType _PieceType;

    [HideInInspector] public int _PawnMovementDirection = -1;
    public Team _Team;
}

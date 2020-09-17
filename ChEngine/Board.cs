using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace ChEngine
{
    public class Board : ICloneable
    {
        public Field[] Fields;
        public PlayerOption[] PlayerOptions;
        public bool IsWhiteToMove;
        public readonly Cache Cache = new Cache();

        public static readonly Field[] DEFAULT_FIELDS = CreateDefaultField();
        public static readonly PlayerOption[] DEFAULT_PLAYER_OPTIONS = new PlayerOption[] { PlayerOption.DefautOption, PlayerOption.DefautOption };
        // Defines all possible knight moves based on the location
        public static readonly int[][] LOOKUP_KNIGHTMOVES = CreateKnightLookup();
        private static int[][] CreateKnightLookup()
        {
            var result = new int[8 * 8][];
            result[0] = new int[] { 17, 10 };
            result[1] = new int[] { 16, 18, 11 };
            result[2] = new int[] { 8, 17, 19, 12 };
            result[3] = new int[] { 9, 18, 20, 13 };
            result[4] = new int[] { 10, 19, 21, 14 };
            result[5] = new int[] { 11, 20, 22, 15 };
            result[6] = new int[] { 12, 21, 23 };
            result[7] = new int[] { 13, 22 };
            result[8] = new int[] { 25, 2, 18 };
            result[9] = new int[] { 24, 26, 3, 19 };
            result[10] = new int[] { 0, 16, 25, 27, 4, 20 };
            result[11] = new int[] { 1, 17, 26, 28, 5, 21 };
            result[12] = new int[] { 2, 18, 27, 29, 6, 22 };
            result[13] = new int[] { 3, 19, 28, 30, 7, 23 };
            result[14] = new int[] { 4, 20, 29, 31 };
            result[15] = new int[] { 5, 21, 30 };
            result[16] = new int[] { 1, 33, 10, 26 };
            result[17] = new int[] { 0, 32, 2, 34, 11, 27 };
            result[18] = new int[] { 8, 24, 1, 33, 3, 35, 12, 28 };
            result[19] = new int[] { 9, 25, 2, 34, 4, 36, 13, 29 };
            result[20] = new int[] { 10, 26, 3, 35, 5, 37, 14, 30 };
            result[21] = new int[] { 11, 27, 4, 36, 6, 38, 15, 31 };
            result[22] = new int[] { 12, 28, 5, 37, 7, 39 };
            result[23] = new int[] { 13, 29, 6, 38 };
            result[24] = new int[] { 9, 41, 18, 34 };
            result[25] = new int[] { 8, 40, 10, 42, 19, 35 };
            result[26] = new int[] { 16, 32, 9, 41, 11, 43, 20, 36 };
            result[27] = new int[] { 17, 33, 10, 42, 12, 44, 21, 37 };
            result[28] = new int[] { 18, 34, 11, 43, 13, 45, 22, 38 };
            result[29] = new int[] { 19, 35, 12, 44, 14, 46, 23, 39 };
            result[30] = new int[] { 20, 36, 13, 45, 15, 47 };
            result[31] = new int[] { 21, 37, 14, 46 };
            result[32] = new int[] { 17, 49, 26, 42 };
            result[33] = new int[] { 16, 48, 18, 50, 27, 43 };
            result[34] = new int[] { 24, 40, 17, 49, 19, 51, 28, 44 };
            result[35] = new int[] { 25, 41, 18, 50, 20, 52, 29, 45 };
            result[36] = new int[] { 26, 42, 19, 51, 21, 53, 30, 46 };
            result[37] = new int[] { 27, 43, 20, 52, 22, 54, 31, 47 };
            result[38] = new int[] { 28, 44, 21, 53, 23, 55 };
            result[39] = new int[] { 29, 45, 22, 54 };
            result[40] = new int[] { 25, 57, 34, 50 };
            result[41] = new int[] { 24, 56, 26, 58, 35, 51 };
            result[42] = new int[] { 32, 48, 25, 57, 27, 59, 36, 52 };
            result[43] = new int[] { 33, 49, 26, 58, 28, 60, 37, 53 };
            result[44] = new int[] { 34, 50, 27, 59, 29, 61, 38, 54 };
            result[45] = new int[] { 35, 51, 28, 60, 30, 62, 39, 55 };
            result[46] = new int[] { 36, 52, 29, 61, 31, 63 };
            result[47] = new int[] { 37, 53, 30, 62 };
            result[48] = new int[] { 33, 42, 58 };
            result[49] = new int[] { 32, 34, 43, 59 };
            result[50] = new int[] { 40, 56, 33, 35, 44, 60 };
            result[51] = new int[] { 41, 57, 34, 36, 45, 61 };
            result[52] = new int[] { 42, 58, 35, 37, 46, 62 };
            result[53] = new int[] { 43, 59, 36, 38, 47, 63 };
            result[54] = new int[] { 44, 60, 37, 39 };
            result[55] = new int[] { 45, 61, 38 };
            result[56] = new int[] { 41, 50 };
            result[57] = new int[] { 40, 42, 51 };
            result[58] = new int[] { 48, 41, 43, 52 };
            result[59] = new int[] { 49, 42, 44, 53 };
            result[60] = new int[] { 50, 43, 45, 54 };
            result[61] = new int[] { 51, 44, 46, 55 };
            result[62] = new int[] { 52, 45, 47 };
            result[63] = new int[] { 53, 46 };
            return result;
        }

        private static Field[] CreateDefaultField()
        {
            var result = new Field[8 * 8];

            // Standard config: pawns
            for (int i = 8; i < 8 + 8; i++)
            {
                result[i] = new Field(true, TypeOfFigure.Pawn);
                result[i + (5 * 8)] = new Field(false, TypeOfFigure.Pawn);
            }

            const int sevenRows = 7 * 8;

            // standard config: pieces
            result[0] = new Field(true, TypeOfFigure.Rook);
            result[0 + sevenRows] = new Field(false, TypeOfFigure.Rook);

            result[1] = new Field(true, TypeOfFigure.Knight);
            result[1 + sevenRows] = new Field(false, TypeOfFigure.Knight);

            result[2] = new Field(true, TypeOfFigure.Bishop);
            result[2 + sevenRows] = new Field(false, TypeOfFigure.Bishop);

            result[3] = new Field(true, TypeOfFigure.Queen);
            result[3 + sevenRows] = new Field(false, TypeOfFigure.Queen);

            result[4] = new Field(true, TypeOfFigure.King);
            result[4 + sevenRows] = new Field(false, TypeOfFigure.King);

            result[5] = new Field(true, TypeOfFigure.Bishop);
            result[5 + sevenRows] = new Field(false, TypeOfFigure.Bishop);

            result[6] = new Field(true, TypeOfFigure.Knight);
            result[6 + sevenRows] = new Field(false, TypeOfFigure.Knight);

            result[7] = new Field(true, TypeOfFigure.Rook);
            result[7 + sevenRows] = new Field(false, TypeOfFigure.Rook);

            return result;
        }


        private Board()
        { }

        public Board(IEnumerable<Move> moves)
        {
            Fields = (Field[])DEFAULT_FIELDS.Clone();
            IsWhiteToMove = true;
            PlayerOptions = (PlayerOption[])DEFAULT_PLAYER_OPTIONS.Clone();

            // Apply all mutations
            foreach (var move in moves)
                Mutate(move);
        }

        public object Clone()
        {
            return new Board()
            {
                Fields = (Field[])Fields.Clone(),
                PlayerOptions = (PlayerOption[])PlayerOptions.Clone(),
                IsWhiteToMove = IsWhiteToMove
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Board b)
                return Enumerable.SequenceEqual(b.Fields, Fields)
                    && (b.IsWhiteToMove && IsWhiteToMove)
                    && Enumerable.SequenceEqual(b.PlayerOptions, PlayerOptions);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                IsWhiteToMove,
                Enumerable.Aggregate(Fields, 0, (last, next) => HashCode.Combine(
                    last,
                    next.GetHashCode()
                )),
                Enumerable.Aggregate(PlayerOptions, 0, (last, next) => HashCode.Combine(
                    last,
                    next.GetHashCode()
                )));
        }


        public void Mutate(Move move)
        {
            switch (move.Type)
            {
                case TypeOfMove.Move:
                case TypeOfMove.Take:

                    // Pick piece
                    Field figureFrom = Fields[move.From];

                    // This could be special move (king castles) or lose the right to castle
                    if (figureFrom.Figure == TypeOfFigure.King)
                    {
                        // this will lose the right to castle always
                        PlayerOptions[CurrentPlayerId()].KingsideCastle = PlayerOptions[CurrentPlayerId()].QueensideCastle = false;

                        // King can normally never jump two!    
                        int colJump = move.To % 8 - move.From % 8;
                        int rowNumber = move.To / 8;

                        // kingside
                        if (colJump == 2)
                        {
                            // Note: there must be several conditions met such that this mutation is possible!
                            // teleport rook here
                            Fields[7 + rowNumber * 8].Figure = TypeOfFigure.EMPTY;
                            Fields[5 + rowNumber * 8].Figure = TypeOfFigure.Rook;
                            Fields[5 + rowNumber * 8].IsWhite = IsWhiteToMove;
                        }
                        // queenside
                        else if (colJump == -2)
                        {
                            // teleport rook here
                            Fields[0 + rowNumber * 8].Figure = TypeOfFigure.EMPTY;
                            Fields[3 + rowNumber * 8].Figure = TypeOfFigure.Rook;
                            Fields[3 + rowNumber * 8].IsWhite = IsWhiteToMove;
                        }
                    }

                    // this could be special move pawn long jump
                    if ((figureFrom.Figure == TypeOfFigure.Pawn) && (Math.Abs(move.From - move.To) == 2 * 8))
                    {
                        // add a en passant taking option for the enemy
                        PlayerOptions[OtherPlayerId()].GiveEnpassantOnCol(move.From % 8);
                    }

                    // this rook move could loose the right to castle
                    if (figureFrom.Figure == TypeOfFigure.Rook)
                    {
                        // this can overwrite multiple times, which is not bad just maybe not optimal
                        int colNum = move.From % 8;
                        if (colNum == 0)
                            PlayerOptions[CurrentPlayerId()].QueensideCastle = false;
                        else if (colNum == 7)
                            PlayerOptions[CurrentPlayerId()].KingsideCastle = false;
                    }

                    // remove from
                    Fields[move.From].Figure = TypeOfFigure.EMPTY;

                    // Check destination
                    Fields[move.To] = figureFrom;
                    break;

                case TypeOfMove.NullMove:
                    // Do nothing actually
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (move.Promotion)
            {
                case TypeOfPromotion.NoPromotion:
                    break;

                case TypeOfPromotion.PromoteKnight:
                case TypeOfPromotion.PromoteBishop:
                case TypeOfPromotion.PromoteRook:
                case TypeOfPromotion.PromoteQueen:
                    // Get type
                    var pieceType = (TypeOfFigure)move.Promotion;

                    // place piece there
                    Fields[move.To].Figure = pieceType;

                    break;

                default:
                    throw new NotImplementedException();
            }

            // Revoke all options for en passant
            PlayerOptions[CurrentPlayerId()].ClearEnpassant();

            // Flip who is to move
            IsWhiteToMove = !IsWhiteToMove;

            // Disvalidate Cache
            Cache.Clear();
        }


        public int CurrentPlayerId() => PlayerId(IsWhiteToMove);
        public int OtherPlayerId() => PlayerId(!IsWhiteToMove);
        public static int PlayerId(bool isWhite) => isWhite ? 0 : 1;

        public List<Move> GetLegalMoves()
        {
            // Check Cache
            if (Cache.LegalMoves != null)
                return Cache.LegalMoves;

            List<Move> moves = GetMoves_IgnoreCheckRules().ToList();

            // castling?
            // check that you are not in check!
            if (GetInCheck() == false)
            {
                int kingPos = 4 + (IsWhiteToMove ? 0 : 7) * 8;
                if (PlayerOptions[CurrentPlayerId()].KingsideCastle)
                {
                    if (CheckTemporaryCastlingCondition(kingPos, towardsKingside: true))
                        // if you got here, nice!
                        moves.Add(new Move(kingPos, kingPos + 2, TypeOfMove.Move));
                }

                if (PlayerOptions[CurrentPlayerId()].QueensideCastle)
                {
                    if (CheckTemporaryCastlingCondition(kingPos, towardsKingside: false))
                        // if you got here, nice!
                        moves.Add(new Move(kingPos, kingPos - 2, TypeOfMove.Move));
                }
            }

            // you are never allowed to make a move which would make it possible for your enemy to take your king on his next move!
            // This automatically includes 'check' and 'pinned pieces' rule
            List<Move> protectKingRule = new List<Move>();

            // find your king here
            int kingLocation = FindInArray(Fields, x => x.Figure == TypeOfFigure.King && x.IsWhite == IsWhiteToMove);

            // todo: maybe easiest is to make a copy for each legal move and see if you can take the king
            foreach (var move in moves)
            {
                var clone = (Board)Clone();
                clone.Mutate(move);

                // check if you need to recalculate king location
                int possiblyNewKingLocation = move.From != kingLocation ? kingLocation : move.To;

                // Check enemy moves
                var couldTakeKing = clone.GetMoves_IgnoreCheckRules().Any(x => x.Type == TypeOfMove.Take && x.To == possiblyNewKingLocation);
                if (couldTakeKing)
                    continue;

                // This move is ok
                protectKingRule.Add(move);
            }

            moves = protectKingRule;

            // Store cache
            Cache.LegalMoves = moves;

            return moves;
        }

        /// <summary>
        /// Returns an iterator for looking for simple moves
        /// These are moves that you can take by applying the normal move rules for pieces
        /// Like: Empty space, takes, etc
        /// Does not take check or pinning rules into consideration
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Move> GetMoves_IgnoreCheckRules()
        {
            // Find each figure
            for (int i = 0; i < 8 * 8; i++)
            {
                Field f = Fields[i];

                // XOR is same as saying that the color is not who is to move
                if (f.IsWhite ^ IsWhiteToMove)
                    continue;

                switch (f.Figure)
                {
                    case TypeOfFigure.EMPTY:
                        break;

                    case TypeOfFigure.Rook:
                        foreach (var item in RooklikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Knight:
                        foreach (var item in KnightlikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Bishop:
                        foreach (var item in BishoplikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Queen:
                        foreach (var item in RooklikeMoves(i))
                            yield return item;
                        foreach (var item in BishoplikeMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.King:
                        foreach (var item in KingMoves(i))
                            yield return item;
                        break;

                    case TypeOfFigure.Pawn:
                        foreach (var item in PawnMoves(i))
                            yield return item;
                        break;

                    default:
                        break;
                }
            }
        }

        public bool GetInCheck()
        {
            if (Cache.InCheck.HasValue)
                return Cache.InCheck.Value;


            // find your king here
            int kingLocation = FindInArray(Fields, x => x.Figure == TypeOfFigure.King && x.IsWhite == IsWhiteToMove);

            // make a copy
            var clone = (Board)Clone();

            // make a null move (to give the other player the move)
            clone.Mutate(Move.NullMove);

            // Check enemy moves
            var result = clone.GetMoves_IgnoreCheckRules().Any(x => x.Type == TypeOfMove.Take && x.To == kingLocation);

            // store cache
            Cache.InCheck = result;

            return result;
        }

        private int FindInArray<T>(T[] arr, Func<T, bool> predicate)
        {
            for (int i = 0; i < arr.Length; i++)
                if (predicate(arr[i]))
                    return i;

            return -1;
        }

        private IEnumerable<Move> PawnMoves(int i)
        {
            int rowNum = i / 8;
            bool isWhite = Fields[i].IsWhite;
            int oneFromLastRow = isWhite ? 6 : 1;
            int sign = isWhite ? 1 : -1;


            // can go one up if free
            int nextField = i + sign * 8;
            if (Fields[nextField].Figure == TypeOfFigure.EMPTY)
            {
                if (rowNum == oneFromLastRow)
                {
                    // promote if last line
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteBishop);
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteKnight);
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteQueen);
                    yield return new Move(i, nextField, TypeOfMove.Move, TypeOfPromotion.PromoteRook);
                }
                else
                {
                    yield return new Move(i, nextField, TypeOfMove.Move);

                    // can jump 2 times if both are free and is on first pawn line
                    if (rowNum == (isWhite ? 1 : 7))
                    {
                        nextField += sign * 8;
                        if (Fields[nextField].Figure == TypeOfFigure.EMPTY)
                            yield return new Move(i, nextField, TypeOfMove.Move);
                    }
                }
            }

            int enPassantRow = isWhite ? 4 : 3;

            // Can take diagonally left
            int colNum = i % 8;
            if (colNum > 0)
            {
                // note, this is left regardless if black or white!
                int dest = i - 1 + sign * 8;
                if (
                    (Fields[dest].Figure != TypeOfFigure.EMPTY)
                    &&
                    (Fields[dest].IsWhite != IsWhiteToMove)
                )
                {
                    if (rowNum != oneFromLastRow)
                    {
                        // normal move forward
                        yield return new Move(i, dest, TypeOfMove.Take);
                    }
                    // promote if last line
                    else
                    {
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteBishop);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteKnight);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteQueen);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteRook);
                    }
                }

                // Check en passant
                if ((rowNum == enPassantRow) && PlayerOptions[CurrentPlayerId()].CheckEnpassantOnCol(colNum - 1))
                    yield return new Move(i, dest, TypeOfMove.Take);
            }

            // and right
            if (colNum < 7)
            {
                // Note: this is right regardless if black or white
                int dest = i + 1 + sign * 8;
                if (
                    (Fields[dest].Figure != TypeOfFigure.EMPTY)
                    &&
                    (Fields[dest].IsWhite != IsWhiteToMove)
                )
                {
                    if (rowNum != oneFromLastRow)
                    {
                        yield return new Move(i, dest, TypeOfMove.Take);
                    }
                    // And promote if last line
                    else
                    {
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteBishop);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteKnight);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteQueen);
                        yield return new Move(i, dest, TypeOfMove.Move, TypeOfPromotion.PromoteRook);
                    }
                }

                // Check en passant
                if ((rowNum == enPassantRow) && PlayerOptions[CurrentPlayerId()].CheckEnpassantOnCol(colNum + 1))
                    yield return new Move(i, dest, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> KingMoves(int from)
        {
            // There are 8 configurations
            List<Point> vectors = new List<Point>() {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(1, -1),
                new Point(0, -1),
                new Point(-1, -1),
                new Point(-1, 0),
                new Point(-1, 1),
            };

            // Check bounds
            int colNumber = from % 8;
            int rowNumber = from / 8;
            // check to right / left
            switch (colNumber)
            {
                case 0:
                    // remove all possibilites that go left
                    vectors.RemoveAll(x => x.X < 0);
                    break;
                case 7:
                    // remove all possibilites that go right 
                    vectors.RemoveAll(x => x.X > 0);
                    break;
                default:
                    break;
            }

            // check to top / bottom
            switch (rowNumber)
            {
                case 0:
                    // remove all possibilites that go bottom
                    vectors.RemoveAll(x => x.Y < 0);
                    break;
                case 7:
                    // remove all possibilites that go top 
                    vectors.RemoveAll(x => x.Y > 0);
                    break;
                default:
                    break;
            }

            // iterate over each
            foreach (var vec in vectors)
            {
                int index = from + vec.X + (vec.Y * 8);

                if (Fields[index].Figure == TypeOfFigure.EMPTY)
                    yield return new Move(from, index, TypeOfMove.Move);
                else if (Fields[index].IsWhite ^ IsWhiteToMove)
                    yield return new Move(from, index, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> KnightlikeMoves(int from)
        {
            // iterate over possibilities
            foreach (var index in LOOKUP_KNIGHTMOVES[from])
            {
                if (Fields[index].Figure == TypeOfFigure.EMPTY)
                    yield return new Move(from, index, TypeOfMove.Move);
                else if (Fields[index].IsWhite ^ IsWhiteToMove)
                    yield return new Move(from, index, TypeOfMove.Take);
            }
        }

        private IEnumerable<Move> BishoplikeMoves(int i)
        {
            // Store some vars
            int colNum = i % 8;
            int rowNum = i / 8;
            int columnsToRight = 8 - (colNum + 1);
            int rowsToTop = 8 - (rowNum + 1);

            int index;

            // Go right and up until you hit something
            index = i;
            for (int times = 0; times < Math.Min(columnsToRight, rowsToTop); times++)
            {
                index += 8 + 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }

            // Go left and up until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowsToTop); times++)
            {
                index += 8 - 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }

            // Go left and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(colNum, rowNum); times++)
            {
                index += -8 - 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }

            // Go right and down until you hit something
            index = i;
            for (int times = 0; times < Math.Min(columnsToRight, rowNum); times++)
            {
                index += -8 + 1;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return (new Move(i, index, TypeOfMove.Move));
            }
        }

        private IEnumerable<Move> RooklikeMoves(int i)
        {
            // Store some vars
            int colNum = i % 8;
            int rowNum = i / 8;
            int columnOffset = colNum;
            int rowOffset = (i / 8) * 8;

            // Go right until you hit something
            for (int colI = columnOffset + 1; colI < 8; colI++)
            {
                int index = colI + rowOffset;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return new Move(i, index, TypeOfMove.Take);

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }

            // Go left until you hit something
            for (int colI = columnOffset - 1; colI >= 0; colI--)
            {
                int index = colI + rowOffset;
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return new Move(i, index, TypeOfMove.Take);

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }

            // Go up until you hit something
            for (int rowI = rowNum + 1; rowI < 8; rowI++)
            {
                int index = columnOffset + (rowI * 8);
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return new Move(i, index, TypeOfMove.Take);

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }

            // go down until you hit something
            for (int rowI = rowNum - 1; rowI >= 0; rowI--)
            {
                int index = columnOffset + (rowI * 8);
                if (Fields[index].Figure != TypeOfFigure.EMPTY)
                {
                    // if is opposite color, add as well
                    if (Fields[index].IsWhite ^ IsWhiteToMove)
                        yield return (new Move(i, index, TypeOfMove.Take));

                    break;
                }

                yield return new Move(i, index, TypeOfMove.Move);
            }
        }

        public double GetEvaluation()
        {
            // Check cache
            if (Cache.Evaluation != null)
                return Cache.Evaluation.Value;

            // simply count pieces
            double score = 0;
            for (int i = 0; i < 8 * 8; i++)
                score += Weighting(Fields[i].Figure) * (Fields[i].IsWhite ? 1 : -1);


            // store cache
            Cache.Evaluation = score;

            return score;
        }

        public bool CheckTemporaryCastlingCondition(int kingPos, bool towardsKingside)
        {
            int sign = towardsKingside ? 1 : -1;
            int rowOffset = (kingPos / 8) * 8; // note the integer division!

            // first of all, all cols to the corner must be free
            for (int i = 4 + sign; (i > 0) && (i < 7); i += sign)
            {
                if (Fields[i + rowOffset].Figure != TypeOfFigure.EMPTY)
                    return false;
            }

            // each subsequent move must not result in check
            Board copy = (Board)Clone();

            for (int i = 0; i < 2; i++)
            {
                // move the king one square
                copy.Mutate(new Move(kingPos + sign * i, kingPos + sign * (i + 1), TypeOfMove.Move));

                // Make a null move such that it is the same player again
                copy.Mutate(Move.NullMove);

                // you cannot be in check!
                if (copy.GetInCheck())
                    return false;
            }

            return true;
        }

        public static double Weighting(TypeOfFigure type)
        {
            return type switch
            {
                TypeOfFigure.EMPTY => 0,
                TypeOfFigure.Rook => 5,
                TypeOfFigure.Knight => 3,
                TypeOfFigure.Bishop => 3,
                TypeOfFigure.Queen => 9,
                TypeOfFigure.King => 50,
                TypeOfFigure.Pawn => 1,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public class Cache
    {
        public List<Move> LegalMoves { get; set; }
        public double? Evaluation { get; set; }
        public bool? InCheck;

        public void Clear()
        {
            LegalMoves = null;
            Evaluation = null;
            InCheck = null;
        }
    }

    public struct PlayerOption
    {
        public byte EnPassantOptions;
        public bool KingsideCastle;
        public bool QueensideCastle;

        public static readonly PlayerOption DefautOption = new PlayerOption() { EnPassantOptions = 0, KingsideCastle = true, QueensideCastle = true };


        // set bit number colNumber
        public void GiveEnpassantOnCol(int colNumber) => EnPassantOptions = (byte)(EnPassantOptions | (1 << colNumber));

        // Clear all bits
        public void ClearEnpassant() => EnPassantOptions = 0;

        // test bit
        public bool CheckEnpassantOnCol(int colNumber) => (EnPassantOptions & (1 << colNumber)) != 0;
    }
}
using System.Collections.Generic;
using System.Linq;
using Models.Board;
using Models.Player;
using UnityEngine;

namespace Models
{
    public class Game : MonoBehaviour
    {
        public int BoardLength { get; private set; }

        public GameObject whitePref;
        public GameObject blackPref;
        public GameObject board;
        public GameObject tilePref;

        private Board.Board _logicalBoard; // this is the actual state
        private IPlayer _player1;
        private IPlayer _player2;

        public IPlayer CurrentPlayer { get; private set; }
        public static Game Instance;

        private bool _isWhitesTurn;
        
        public void Awake()
        {
            BoardLength = PlayerPrefs.GetInt("BoardLength");
            if (BoardLength < 6 || BoardLength > 10 || BoardLength % 2 != 0) BoardLength = 8;
            
            _logicalBoard = new Board.Board(BoardLength);

            _player1 = ComputerPlayer
                .Create(PlayerColor.Black);
            _player2 = HumanPlayer
                .Create(PlayerColor.White);

            CurrentPlayer = _player1;
            Instance = this;
        }
        
        public void Start()
        {
            GeneratePhysicalBoard();
            NextPlayer(false); // human will start
        }

        public void Update()
        {
            if (CurrentPlayer.HasPassed() && _player2.HasPassed())
            {
                int ScoreFor(PlayerColor color) => _logicalBoard.LogicalState.Count(piece => piece.Color == color);
                int scoreBlack = ScoreFor(PlayerColor.Black);
                int scoreWhite = ScoreFor(PlayerColor.White);
                string winner;
                
                if (scoreWhite == scoreBlack) winner = "Draw!";
                else if (scoreBlack > scoreWhite) winner = "Black wins!";
                else winner = "White wins!";
                
                Debug.Log(winner + scoreBlack + " : " + scoreWhite);
                // TODO Wie gibt man ein Ergebnis aus?????
                return; // zurueck zum menu
            }
            if (!CurrentPlayer.HasNextMove())
            {
                NextPlayer(true);
                return;
            }

            List<Move> nextMove = CurrentPlayer.GetNextMove();
            if (nextMove.Count == 0) return;
            
            _logicalBoard = _logicalBoard.With(nextMove);

            NextPlayer(false);
        }
        
        public GameObject GetPrefForColor(PlayerColor color) => color == PlayerColor.Black ? blackPref : whitePref;
        
        private void GeneratePhysicalBoard()
        {
            int middle = BoardLength / 2;
            int offMiddle = middle - 1;
            
            void CreateTileAt(int x, int z) => 
                Instantiate(tilePref, new Vector3(x, 0, z), Quaternion.identity)
                    .transform.SetParent(board.transform);
            
            for (int x = 0; x < BoardLength; x++)
            {
                for (int z = 0; z < BoardLength; z++)
                {
                    CreateTileAt(x, z);
                }
            }

            _logicalBoard = _logicalBoard
                .With(new LogicalPiece(offMiddle, offMiddle, PlayerColor.Black))
                .With(new LogicalPiece(offMiddle, middle, PlayerColor.White))
                .With(new LogicalPiece(middle, middle, PlayerColor.Black))
                .With(new LogicalPiece(middle, offMiddle, PlayerColor.White));
        }
        
        private void NextPlayer(bool currentHasPassed)
        {
            if (currentHasPassed)
            {
                _player1 = CurrentPlayer.WithPass();
                CurrentPlayer = _player2;
                _player2 = _player1;
            }
            else
            {
                _player1 = CurrentPlayer.WithCalculatedPotentialMovesFrom(_logicalBoard.LogicalState);
                CurrentPlayer = _player2.WithCalculatedPotentialMovesFrom(_logicalBoard.LogicalState);
                _player2 = _player1;
            }
        }
    }
}
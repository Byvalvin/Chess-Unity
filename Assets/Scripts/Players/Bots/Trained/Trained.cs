using UnityEngine;
using Unity.Barracuda;
using System.IO;
using System.Collections.Generic;

public class Trained : Bot
{
}

public abstract class TrainedState : BotState
{
    protected NNModel nnmodel;
    protected Model model;  // Use Model, not NNModel
    protected IWorker worker;

    protected string trainedModelPath;

    protected WorkerFactory.Type GPUorCPU = WorkerFactory.Type.CSharpBurst;
    
    protected enum PieceType
    {
        PAWN = 1,
        KNIGHT = 2,
        BISHOP = 3,
        ROOK = 4,
        QUEEN = 5,
        KING = 6
    }

    public TrainedState(string trainedModelPath, string playerName, bool isWhite) : base(playerName, isWhite)
    {   
        nnmodel = Resources.Load<NNModel>($"Models/{trainedModelPath}");
        model = ModelLoader.Load(nnmodel);
        worker = WorkerFactory.CreateWorker(GPUorCPU, model);
    }

    public TrainedState(TrainedState original) : base(original)
    {   
        nnmodel = original.nnmodel;
        model = ModelLoader.Load(nnmodel);
        worker = WorkerFactory.CreateWorker(GPUorCPU, model);
    }

    public abstract override PlayerState Clone();

    // Override the GetMove method to use the trained model for decision-making
    public override Vector2Int GetMove()
    {
        // Create a board representation in the form that the model expects
        Tensor boardState = ConvertFENToTensor();

        // Feed the tensor into the model
        worker.Execute(boardState);

        // Get the predicted move from the model
        var output = worker.PeekOutput();

        // Decode the model's output to get the best move. You might have to adjust this depending on the model's output format
        Vector2Int bestMove = DecodeMove(output);

        // Clean up the tensor
        boardState.Dispose();
        Debug.Log("best ML move");
        Debug.Log(bestMove);
        
        return bestMove;
    }

    // This function converts a boardstate FEN string into a tensor-like array
    // This function converts a FEN string into a 13x8x8 tensor-like array
    protected virtual Tensor ConvertFENToTensor() 
    {
        // Initialize a 13x8x8 array to hold the board data
        float[,,] inputTensor = new float[13, 8, 8];
        string fen = CurrentGame.FEN();
        
        // Split the FEN string by spaces to extract the board state and who's turn it is
        string[] fenParts = fen.Split(' ');
        
        // Parse the board layout part (FEN board position)
        string board = fenParts[0];
        
        // Loop through each rank (8 rows of the board)
        string[] ranks = board.Split('/');
        
        for (int row = 0; row < 8; row++) 
        {
            string rank = ranks[row];
            int col = 0;

            // Loop through each character in the rank string
            for (int i = 0; i < rank.Length; i++) 
            {
                char c = rank[i];
                if (char.IsDigit(c)) // If it's a number (empty squares)
                {
                    int emptySquares = int.Parse(c.ToString());
                    col += emptySquares; // Skip these squares
                } 
                else 
                {
                    int pieceValue = PieceToValue(c);
                    inputTensor[pieceValue, row, col] = 1; // Mark the piece in the corresponding position
                    col++;
                }
            }
        }
        
        // Parse the player to move (White or Black)
        string playerToMove = fenParts[1].ToLower();
        inputTensor[12, 0, 0] = (playerToMove == "w") ? 1f : -1f;

        // Convert the 4D tensor to a 1D array (needed by Unity ML-Agents)
        float[] tensorData = new float[1 * 8 * 8 * 13];
        int idx = 0;
        for (int i = 0; i < 1; i++) // 1 batch
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int channel = 0; channel < 13; channel++)
                    {
                        tensorData[idx++] = inputTensor[channel, y, x];
                    }
                }
            }
        }

        // Create and return the Unity Tensor
        Tensor tensorObj = new Tensor(1,8,8,13,tensorData,"board_state");
        return tensorObj;
    }

    // This function gets the move to play from a boardstate as a tensor
    protected virtual Vector2Int DecodeMove(Tensor output) => BestValid(output);
    protected virtual Vector2Int BestValid(Tensor output)
    {
        Debug.Log(output.shape);
        float maxProb = float.MinValue;
        int bmove = -1;
        int from = bmove, to = bmove;
        PieceBoard chosenPiece;
        for(int i=0; i<4096; i++){
            from = i/64;
            to = i%64;
            chosenPiece = null;
            foreach(var pieceBoard in PieceBoards.Values)
                if(pieceBoard.ValidMovesMap.ContainsKey(from)){
                    chosenPiece = pieceBoard;
                    break;     
                }
            if(chosenPiece==null)
                continue; // no piece at this location

            bool isLegal = BitOps.GetBitBoard(to, CurrentGame.GetMovesAllowed(chosenPiece, from))!=0;
            if(maxProb<output[0,i] && isLegal){
                maxProb = output[0,i];
                bmove = i;
            }
        }
        from = bmove/64;
        to = bmove%64;
        
        Debug.Log($"{bmove} -> {from} to {to}");
      
        // Debug logs to show the details of the decoded move
        int fromIndex = from, toIndex=to, promotionIndex=-1;
        Debug.Log($"From: {fromIndex} To: {toIndex} Promotion: {promotionIndex}");
        // Depending on the promotion, return the move (you might want to handle the promotion here as well)
        Vector2Int move = new Vector2Int(fromIndex, toIndex);
        return move;  // You can also return a tuple with 'from' and 'to' if you need to keep track of the promotion.
    }
    
    // This function maps a piece character to a value for each channel
    protected int PieceToValue(char piece) 
    {
        switch (piece) 
        {
            // White pieces
            case 'P': return 0; // White Pawn
            case 'N': return 1; // White Knight
            case 'B': return 2; // White Bishop
            case 'R': return 3; // White Rook
            case 'Q': return 4; // White Queen
            case 'K': return 5; // White King

            // Black pieces
            case 'p': return 6; // Black Pawn
            case 'n': return 7; // Black Knight
            case 'b': return 8; // Black Bishop
            case 'r': return 9; // Black Rook
            case 'q': return 10; // Black Queen
            case 'k': return 11; // Black King

            default: return -1; // Invalid piece
        }
    }
    

    // Clean up any resources when no longer needed (e.g., when the game ends)
    protected void Dispose()
    {
        if (worker != null)
        {
            worker.Dispose();
            worker = null;
        }
    }
    public override void Close()=>Dispose();

    // extra
    public Tensor ConvertBoardToTensor()
    {
        // Initialize a 4D tensor with the shape [1, 8, 8, 12]
        float[,,,] tensor = new float[1, 8, 8, 12];
        char[,] board = CurrentGame.StringBoard();

        // Define a dictionary to map piece types to tensor channel indices
        Dictionary<char, int> pieceToChannel = new Dictionary<char, int>()
        {
            {'K', 0}, {'Q', 1}, {'R', 2}, {'B', 3}, {'N', 4}, {'P', 5},  // White pieces
            {'k', 6}, {'q', 7}, {'r', 8}, {'b', 9}, {'n', 10}, {'p', 11}   // Black pieces
        };

        // Iterate over each square of the board (8x8)
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                char piece = board[y, x];

                // If the square is empty, skip to the next one
                if (piece == '.')
                    continue;

                // Find the channel index based on the piece type
                if (pieceToChannel.ContainsKey(piece))
                {
                    // Set the corresponding channel for this piece to 1 (indicating the piece is present)
                    int channelIndex = pieceToChannel[piece];
                    tensor[0, y, x, channelIndex] = 1f; // Set the tensor value for this piece
                }
            }
        }

        // Convert the 4D array into a 1D array for the Tensor constructor
        float[] tensorData = new float[1 * 8 * 8 * 12];
        int idx = 0;
        for (int i = 0; i < 1; i++) // 1 batch
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int channel = 0; channel < 12; channel++)
                    {
                        tensorData[idx++] = tensor[i, y, x, channel];
                    }
                }
            }
        }

        // Create a new Tensor from the flattened data with shape [1, 8, 8, 12]
        Tensor tensorObj = new Tensor(1, 8, 8, 12, tensorData);
        return tensorObj;
    }

}

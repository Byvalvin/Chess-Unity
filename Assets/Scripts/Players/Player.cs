public class Player
{
    public PlayerState PlayerState { get; private set; }

    public Player(PlayerState playerState)
    {
        PlayerState = playerState;
    }

    // Add methods for player actions, like making a move, if needed
}

namespace GroundChickenKing.Betting
{
    public enum BetCommandError
    {
        None,
        BettingClosed,
        PlayerNotEligible,
        InvalidChicken,
        InvalidStake,
        InsufficientBalance,
        MissingChicken,
        MissingStake,
        AlreadyLocked,
        LockedBetsCannotBeCleared,
    }
}

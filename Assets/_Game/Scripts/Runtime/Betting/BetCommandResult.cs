namespace GroundChickenKing.Betting
{
    public readonly struct BetCommandResult
    {
        private BetCommandResult(bool isSuccess, BetCommandError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public BetCommandError Error { get; }

        public static BetCommandResult Success()
        {
            return new BetCommandResult(true, BetCommandError.None);
        }

        public static BetCommandResult Failure(BetCommandError error)
        {
            return new BetCommandResult(false, error);
        }
    }
}

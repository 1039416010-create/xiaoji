namespace GroundChickenKing.Players
{
    public readonly struct PlayerSeatDefinition
    {
        public PlayerSeatDefinition(PlayerSeat seat, string stableId, string displayName, PlayerFacing facing, PlayerTheme theme, string icon)
        {
            Seat = seat;
            StableId = stableId;
            DisplayName = displayName;
            Facing = facing;
            Theme = theme;
            Icon = icon;
        }

        public PlayerSeat Seat { get; }
        public string StableId { get; }
        public string DisplayName { get; }
        public PlayerFacing Facing { get; }
        public PlayerTheme Theme { get; }
        public string Icon { get; }
    }
}

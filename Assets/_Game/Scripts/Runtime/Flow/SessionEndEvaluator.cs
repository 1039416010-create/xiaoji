using System;
using System.Collections.Generic;
using GroundChickenKing.Players;

namespace GroundChickenKing.Flow
{
    public static class SessionEndEvaluator
    {
        public static GameOverReason Evaluate(IReadOnlyList<PlayerSnapshot> players, int completedRound, int maximumRounds)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));
            if (completedRound <= 0 || maximumRounds <= 0) throw new ArgumentOutOfRangeException();
            var joined = 0; var solvent = 0;
            for (var i = 0; i < players.Count; i++) if (players[i].IsJoined) { joined++; if (players[i].Coins > 0) solvent++; }
            if (joined > 0 && solvent == 0) return GameOverReason.Bankrupt;
            return completedRound >= maximumRounds ? GameOverReason.MaxRounds : GameOverReason.None;
        }
    }
}

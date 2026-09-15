using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class RacePlanValidationResult
    {
        public RacePlanValidationResult(IReadOnlyList<string> errors)
        {
            var copy = new string[errors.Count];
            for (var i = 0; i < copy.Length; i++) copy[i] = errors[i];
            Errors = Array.AsReadOnly(copy);
        }
        public IReadOnlyList<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }
}

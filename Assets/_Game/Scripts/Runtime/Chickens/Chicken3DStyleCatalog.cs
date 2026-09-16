using System;
using System.Collections.Generic;
using UnityEngine;

namespace GroundChickenKing.Chickens
{
    public enum ChickenAccessory
    {
        Headband,
        BowTie,
        RunningShoes,
        Scarf,
        Sunglasses,
        LightningCrest,
        DumplingHat,
        CaptainHat,
        SleepCap,
        RocketPack,
        NinjaMask,
        ScholarCap,
        LuckyCrown,
    }

    public readonly struct Chicken3DStyle
    {
        public Chicken3DStyle(string id, Color body, Color accent, Vector3 proportions, ChickenAccessory accessory, float gait, float swagger)
        {
            Id = id;
            Body = body;
            Accent = accent;
            Proportions = proportions;
            Accessory = accessory;
            Gait = gait;
            Swagger = swagger;
        }

        public string Id { get; }
        public Color Body { get; }
        public Color Accent { get; }
        public Vector3 Proportions { get; }
        public ChickenAccessory Accessory { get; }
        public float Gait { get; }
        public float Swagger { get; }
    }

    public static class Chicken3DStyleCatalog
    {
        private static readonly Chicken3DStyle[] Styles =
        {
            new("chicken-flash",   Hex("F8D84A"), Hex("D63B2F"), new Vector3(.82f, 1.08f, .82f), ChickenAccessory.Headband,      1.35f, .20f),
            new("chicken-chubby",  Hex("E9993E"), Hex("7A2E22"), new Vector3(1.18f, .90f, 1.08f), ChickenAccessory.BowTie,        .82f, .10f),
            new("chicken-tiny",    Hex("77DCE1"), Hex("ED4C57"), new Vector3(.68f, .76f, .70f), ChickenAccessory.RunningShoes,  1.55f, .28f),
            new("chicken-bro",     Hex("A94E31"), Hex("287BA0"), new Vector3(1.12f, 1.12f, 1.02f), ChickenAccessory.Scarf,        1.02f, .55f),
            new("chicken-slacker", Hex("B9D98C"), Hex("222D32"), new Vector3(1.02f, .92f, 1.00f), ChickenAccessory.Sunglasses,   .70f, .42f),
            new("chicken-thunder", Hex("EACB45"), Hex("7040A5"), new Vector3(.92f, 1.12f, .88f), ChickenAccessory.LightningCrest,1.22f, .34f),
            new("chicken-dumpling",Hex("F3EEE0"), Hex("D87463"), new Vector3(1.20f, .82f, 1.16f), ChickenAccessory.DumplingHat,   .76f, .16f),
            new("chicken-captain", Hex("E7B466"), Hex("234E77"), new Vector3(1.02f, 1.10f, .98f), ChickenAccessory.CaptainHat,    .96f, .46f),
            new("chicken-sleepy",  Hex("B7A5D8"), Hex("476B89"), new Vector3(1.02f, .94f, 1.02f), ChickenAccessory.SleepCap,      .66f, .14f),
            new("chicken-rocket",  Hex("F06A43"), Hex("46A6A1"), new Vector3(.86f, 1.16f, .82f), ChickenAccessory.RocketPack,     1.48f, .50f),
            new("chicken-ninja",   Hex("3D4652"), Hex("D44844"), new Vector3(.88f, 1.02f, .86f), ChickenAccessory.NinjaMask,      1.38f, .22f),
            new("chicken-scholar", Hex("EED9A1"), Hex("315E8C"), new Vector3(.96f, 1.04f, .94f), ChickenAccessory.ScholarCap,     .88f, .12f),
            new("chicken-lucky",   Hex("F3C945"), Hex("D44932"), new Vector3(1.06f, 1.08f, 1.00f), ChickenAccessory.LuckyCrown,    1.08f, .62f),
        };

        private static readonly Dictionary<string, Chicken3DStyle> ById = BuildLookup();

        public static IReadOnlyList<Chicken3DStyle> All => Styles;

        public static Chicken3DStyle Get(string chickenId)
        {
            if (string.IsNullOrWhiteSpace(chickenId))
                throw new ArgumentException("Chicken ID is required.", nameof(chickenId));
            if (!ById.TryGetValue(chickenId, out var style))
                throw new KeyNotFoundException($"No 3D style is registered for {chickenId}.");
            return style;
        }

        private static Dictionary<string, Chicken3DStyle> BuildLookup()
        {
            var result = new Dictionary<string, Chicken3DStyle>(StringComparer.Ordinal);
            foreach (var style in Styles)
                result.Add(style.Id, style);
            return result;
        }

        private static Color Hex(string value)
        {
            if (!ColorUtility.TryParseHtmlString($"#{value}", out var color))
                throw new InvalidOperationException($"Invalid color {value}.");
            return color;
        }
    }
}

namespace GWK.Kart {
    public class BoostTier {
        public readonly int tier;
        private BoostTier(int tier) => this.tier = tier;

        public static readonly BoostTier None = new BoostTier(0);
        public static readonly BoostTier Normal = new BoostTier(1);
        public static readonly BoostTier Super = new BoostTier(2);
        public static readonly BoostTier Ultra = new BoostTier(3);
        public static readonly BoostTier Ultimate = new BoostTier(4);

        public BoostTier OneUp() {
            int tier = this.tier + 1;
            return tier switch {
                1 => Normal,
                2 => Super,
                _ => Ultra,
            };
        }

        public static explicit operator float(BoostTier tier) => tier.tier switch {
            0 => 0f,
            1 => 1f,
            2 => 1.1f,
            3 => 1.2f,
            4 => 1.25f,
            _ => 1f
        };

        public override string ToString() => tier switch {
            0 => "None",
            1 => "Normal",
            2 => "Super",
            3 => "Ultra",
            4 => "Ultimate",
            _ => string.Empty,
        };

        public static bool operator > (BoostTier lhs, BoostTier rhs) => lhs.tier > rhs.tier;
        public static bool operator < (BoostTier lhs, BoostTier rhs) => lhs.tier < rhs.tier;
        public static bool operator >= (BoostTier lhs, BoostTier rhs) => lhs.tier >= rhs.tier;
        public static bool operator <= (BoostTier lhs, BoostTier rhs) => lhs.tier <= rhs.tier;
        public static bool operator == (BoostTier lhs, BoostTier rhs) => lhs.tier == rhs.tier;
        public static bool operator != (BoostTier lhs, BoostTier rhs) => lhs.tier != rhs.tier;
    }
}
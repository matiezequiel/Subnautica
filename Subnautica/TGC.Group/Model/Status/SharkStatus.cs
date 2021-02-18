using TGC.Core.Mathematica;

namespace TGC.Group.Model.Status
{
    internal class SharkStatus
    {
        public struct Constants
        {
            public static int LIFE_MAX = 250;
            public static int LIFE_MIN = 0;
            public static float LIFE_REDUCE_STEP = -0.5f;
            public static float DAMAGE_RECEIVED = 50f;
        }

        private float DamageAcumulated = 0;

        public float Life { get; set; } = Constants.LIFE_MAX;
        public bool IsDead => Life == 0;
        public bool DamageReceived { get; set; }

        public SharkStatus() { }

        public float GetLifeMax() => Constants.LIFE_MAX;

        public void Reset()
        {
            Life = Constants.LIFE_MAX;
            DamageAcumulated = 0;
        }

        public void Update()
        {
            if (DamageReceived)
            {
                TakeDamage();
                DamageReceived = false;
            }

            if (DamageAcumulated > 0)
            {
                UpdateLife(Constants.LIFE_REDUCE_STEP);
                DamageAcumulated += Constants.LIFE_REDUCE_STEP;
            }
        }

        private void UpdateLife(float value) => Life = FastMath.Clamp(Life + value, Constants.LIFE_MIN, Constants.LIFE_MAX);

        private void TakeDamage() => DamageAcumulated = Constants.DAMAGE_RECEIVED;
    }
}

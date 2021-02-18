using System;
using TGC.Core.Mathematica;
using TGC.Group.Model.Objects;

namespace TGC.Group.Model.Status
{
    class CharacterStatus
    {
        public struct Constants
        {
            public static int LIFE_MAX = 100;
            public static int LIFE_MIN = 0;
            public static int OXYGEN_MIN = 0;
            public static int OXYGEN_INCREASE_BY_CRAFT = 20;
            public static float LIFE_REDUCE_STEP = -0.3f;
            public static float LIFE_INCREMENT_STEP = 0.05f;
            public static float OXYGEN_INCREMENT_STEP = 1f;
            public static float DAMAGE_RECEIVED = 30f;
        }

        private Character Character { get; set; }
        private bool CanBreathe => (Character.IsInsideShip || Character.CanBreathe) && !IsDead;
        private float DamageAcumulated = 0;
        public bool ActiveAlarmForDamageReceived { get; set; }

        public int OxygenMax = 40;
        public float Life { get; set; } = Constants.LIFE_MAX;
        public float Oxygen { get; set; }
        public bool IsDead => Oxygen == 0 || Life == 0;
        public bool HasDivingHelmet { get; set; }
        public float DamageReceived { get; set; }
        public bool ActiveRenderAlarm => Life < 20 || Oxygen < 15 || ActiveAlarmForDamageReceived;
        public int ShowLife { get => (int)Math.Round(Life, 0); }
        public int ShowOxygen { get => (int)Math.Round(Oxygen, 0); }

        public CharacterStatus(Character character)
        {
            Character = character;
            Oxygen = OxygenMax;
        }

        public float GetLifeMax() => Constants.LIFE_MAX;

        public float GetOxygenMax() => OxygenMax;

        private void RecoverLife() => UpdateLife(Constants.LIFE_INCREMENT_STEP);

        public void UpdateOxygenMax() => OxygenMax += Constants.OXYGEN_INCREASE_BY_CRAFT;

        public void Reset()
        {
            Life = Constants.LIFE_MAX;
            Oxygen = OxygenMax;
            DamageAcumulated = 0;
            ActiveAlarmForDamageReceived = false;
        }

        public void Update(float elapsedTime, bool godMode)
        {
            if (IsDead || godMode)
            {
                DamageAcumulated = 0;
                DamageReceived = 0;
                Oxygen = OxygenMax;
                Life = GetLifeMax();
                return;
            }

            if (DamageReceived > 0)
            {
                TakeDamage();
                DamageReceived = 0;
                ActiveAlarmForDamageReceived = true;
            }

            if (DamageAcumulated > 0)
            {
                UpdateLife(Constants.LIFE_REDUCE_STEP);
                DamageAcumulated += Constants.LIFE_REDUCE_STEP;
            }

            if (Character.IsInsideShip)
            {
                RecoverLife();
            }

            if (CanBreathe)
            {
                UpdateOxygen(Constants.OXYGEN_INCREMENT_STEP);
            }
            else
            {
                UpdateOxygen(-elapsedTime);
            }

            if (Character.SwimActivated)
            {
                UpdateOxygen(-elapsedTime * 3);
                Character.SwimActivated = false;
            }
        }

        private void UpdateLife(float value) => Life = FastMath.Clamp(Life + value, Constants.LIFE_MIN, Constants.LIFE_MAX);

        private void UpdateOxygen(float value) => Oxygen = FastMath.Clamp(Oxygen + value, Constants.OXYGEN_MIN, OxygenMax);

        private void TakeDamage() => DamageAcumulated += FastMath.Min(DamageReceived, Life);

        public void Respawn()
        {
            Reset();
            Character.Respawn();
        }
    }
}

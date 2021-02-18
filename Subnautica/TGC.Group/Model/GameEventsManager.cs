using System.Collections.Generic;
using TGC.Group.Model.Objects;
using TGC.Group.Model.Status;

namespace TGC.Group.Model
{
    internal class GameEventsManager
    {
        private struct Constants
        {
            public static float TIME_BETWEEN_ATTACKS = 25;
        }

        private readonly Shark Shark;
        private readonly Character Character;
        private readonly GameSoundManager SoundManager;
        private float timeBetweenAttacks = Constants.TIME_BETWEEN_ATTACKS;

        public bool SharkIsAttacking { get; private set; } = false;

        public GameEventsManager(Shark shark, Character character, GameSoundManager soundManager)
        {
            Shark = shark;
            Character = character;
            SoundManager = soundManager;
        }

        public void Update(float elapsedTime, List<Fish> fishes, SharkStatus status)
        {
            if (Character.IsOutsideShip)
            {
                CheckIfSharkCanAttack(elapsedTime, status);
            }
            else
            {
                SoundManager.SharkStalking.stop();
                Shark.EndSharkAttack();
                timeBetweenAttacks = Constants.TIME_BETWEEN_ATTACKS;
                InformFinishFromAttack();
            }
            fishes.ForEach(fish => fish.ActivateMove = Character.IsOutsideShip);
        }

        public void InformFinishFromAttack() => SharkIsAttacking = false;

        private void CheckIfSharkCanAttack(float elapsedTime, SharkStatus status)
        {
            if (!SharkIsAttacking)
            {
                timeBetweenAttacks -= elapsedTime;
                if (timeBetweenAttacks <= 0)
                {
                    if (status.IsDead)
                    {
                        status.Reset();
                    }

                    SoundManager.SharkAppear.play();
                    Shark.ActivateShark(this);
                    SharkIsAttacking = true;
                    timeBetweenAttacks = Constants.TIME_BETWEEN_ATTACKS;
                }
            }
        }
    }
}

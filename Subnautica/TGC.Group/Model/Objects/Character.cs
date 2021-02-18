using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using TGC.Core.BulletPhysics;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Utils;

namespace TGC.Group.Model.Objects
{
    class Character
    {
        struct Constants
        {
            public static TGCVector3 OUTDOOR_POSITION = new TGCVector3(1300, 3505, 20);
            public static TGCVector3 INDOOR_POSITION = new TGCVector3(515, -2340, -40);
            public static float MOVEMENT_SPEED = 850f;
            public static TGCVector3 CAMERA_HEIGHT = new TGCVector3(0, 85, 0);
            public static TGCVector3 PLANE_DIRECTOR => TGCVector3.TransformCoordinate(new TGCVector3(-1, 0, 0), TGCMatrix.RotationY(FastMath.PI_HALF));
            public static float CAPSULE_SIZE = 160f;
            public static float CAPSULE_RADIUS = 40f;
        }

        private readonly BulletRigidBodyFactory RigidBodyFactory = BulletRigidBodyFactory.Instance;
        private readonly TgcD3dInput Input;
        private readonly CameraFPS Camera;
        private readonly GameSoundManager SoundManager;
        private Vector3 MovementDirection;
        private float prevLatitude;
        private float Gravity => Body.CenterOfMassPosition.Y < 0 ? -200 : 0;

        public RigidBody Body { get; set; }
        public Weapon Weapon { get; set; }
        public bool IsInsideShip => Camera.Position.Y < 0;
        public bool IsOutsideShip => !IsInsideShip;
        public bool IsOutOfWater => Camera.Position.Y > 3605;
        public bool Submerge => !IsInsideShip && !CanBreathe;
        public bool IsNearSkybox { get; set; }
        public bool CanBreathe => Camera.Position.Y > 3505;

        public bool LooksAtTheHatch { get; set; }
        public bool CanAttack { get; set; }
        public bool NearShip { get; set; }
        public bool SwimActivated { get; set; }

        public bool HasWeapon { get; set; }
        public bool HasDivingHelmet { get; set; }
        public bool CanFish { get; set; }
        public bool InHand { get; set; }

        public bool AttackedShark { get; set; }

        public Character(CameraFPS camera, TgcD3dInput input, GameSoundManager soundManager)
        {
            Camera = camera;
            Input = input;
            SoundManager = soundManager;
            Init();
        }

        private void ChangePosition(TGCVector3 newPosition)
        {
            Body.CenterOfMassTransform = TGCMatrix.Translation(newPosition).ToBulletMatrix();
            Camera.Position = new TGCVector3(Body.CenterOfMassPosition);
            RestartBodySpeed();
        }

        public void Dispose() => Body.Dispose();

        private void Init()
        {
            prevLatitude = Camera.Latitude;
            Constants.PLANE_DIRECTOR.TransformCoordinate(TGCMatrix.RotationY(FastMath.PI_HALF));
            Body = RigidBodyFactory.CreateCapsule(Constants.CAPSULE_RADIUS, Constants.CAPSULE_SIZE, Constants.INDOOR_POSITION, 1f, false);
            Body.CenterOfMassTransform = TGCMatrix.Translation(Constants.INDOOR_POSITION).ToBulletMatrix();
        }

        private void Movement(Vector3 director, Vector3 sideDirector, float speed)
        {
            if (Input.keyDown(Key.W))
            {
                Body.LinearVelocity = MovementDirection = director * speed;
            }

            if (Input.keyDown(Key.S))
            {
                Body.LinearVelocity = MovementDirection = director * -speed;
            }

            if (Input.keyDown(Key.A))
            {
                Body.LinearVelocity = MovementDirection = sideDirector * -speed;
            }

            if (Input.keyDown(Key.D))
            {
                Body.LinearVelocity = MovementDirection = sideDirector * speed;
            }
        }

        private void OutsideMovement(Vector3 director, Vector3 sideDirector, float speed)
        {
            if (IsNearSkybox)
            {
                Body.ApplyCentralImpulse((TGCVector3.Normalize(new TGCVector3(MovementDirection) * -100)).ToBulletVector3());
                return;
            }

            Movement(director, sideDirector, speed);
            if (Input.keyDown(Key.LeftControl))
            {
                Body.LinearVelocity = Vector3.UnitY * -speed;
            }

            if (Input.keyDown(Key.Space))
            {
                Body.LinearVelocity = Vector3.UnitY * speed;
            }

            if (Input.keyDown(Key.LeftShift))
            {
                Body.LinearVelocity = MovementDirection * 2;
                SwimActivated = true;
            }
        }

        public void Respawn() => ChangePosition(Constants.INDOOR_POSITION);

        public void RestartBodySpeed()
        {
            Body.LinearVelocity = Vector3.Zero;
            Body.AngularVelocity = Vector3.Zero;
        }

        private void RestartSpeedForKeyUp()
        {
            if (Input.keyUp(Key.W) || Input.keyUp(Key.S) || Input.keyUp(Key.A) || Input.keyUp(Key.D) ||
                    Input.keyUp(Key.Space) || Input.keyUp(Key.LeftControl))
            {
                RestartBodySpeed();
            }
        }

        public void Teleport()
        {
            if (LooksAtTheHatch)
            {
                ChangePosition(Constants.OUTDOOR_POSITION);
            }

            if (NearShip)
            {
                ChangePosition(Constants.INDOOR_POSITION);
            }
        }

        public void Update(Ray ray, TgcMesh shark, float elapsedTime)
        {
            var speed = Constants.MOVEMENT_SPEED;
            var director = Camera.Direction.ToBulletVector3();
            var sideRotation = Camera.Latitude - prevLatitude;
            var sideDirector = TGCVector3.TransformCoordinate(Constants.PLANE_DIRECTOR, TGCMatrix.RotationY(sideRotation)).ToBulletVector3();

            Body.ActivationState = ActivationState.ActiveTag;
            Body.AngularVelocity = Vector3.Zero;

            if (IsOutOfWater)
            {
                Body.LinearVelocity = Vector3.Zero;
                Body.ApplyCentralImpulse(Vector3.UnitY * -100);
            }
            else if (IsInsideShip)
            {
                Movement(director, sideDirector, speed);
            }
            else
            {
                OutsideMovement(director, sideDirector, speed);
            }

            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT) && HasWeapon && InHand)
            {
                Weapon.ActivateAtackMove();
                SoundManager.WeaponHit.play();
                AttackedShark = ray.IntersectsWithObject(shark.BoundingBox, 150);
                if (AttackedShark)
                {
                    SoundManager.HitToShark.play();
                }
            }

            RestartSpeedForKeyUp();

            Body.LinearVelocity += TGCVector3.Up.ToBulletVector3() * Gravity;
            Camera.Position = new TGCVector3(Body.CenterOfMassPosition) + Constants.CAMERA_HEIGHT;

            if (InHand)
            {
                Weapon.Update(new TGCVector3(director), elapsedTime);
            }
        }

        public void Render()
        {
            if (InHand)
            {
                Weapon.Render();
            }
        }
    }
}
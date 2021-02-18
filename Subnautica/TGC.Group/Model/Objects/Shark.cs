using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.Direct3D;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Utils;

namespace TGC.Group.Model.Objects
{
    internal class Shark
    {
        private struct Constants
        {
            public static string FILE_NAME = "shark-TgcScene.xml";
            public static TGCVector2 SHARK_HEIGHT = new TGCVector2(700, 1800);
            public static TGCVector3 Scale = new TGCVector3(5, 5, 5);
            public static TGCVector3 StartPosition = TGCVector3.Empty;
            public static TGCVector3 SharkBodySize = new TGCVector3(176, 154, 560);
            public static TGCVector3 DirectorZ = new TGCVector3(0, 0, 1);
            public static TGCVector3 DirectorY = new TGCVector3(0, -1, 0);
            public static float MaxYRotation = FastMath.QUARTER_PI;
            public static float MaxAxisRotation = FastMath.PI_HALF / 1.3f;
            public static float MaxZRotation = FastMath.PI;
            public static float MASS = 1000;
            public static float EVENT_TIME = 50;
            public static float DEATH_TIME = 40;
            public static float CHANGE_DIRECTION_TIME = 3;
            public static float WATER_HEIGHT = 3500;
            public static float MIN_DISTANCE_TO_SURFACE = 300;
        }

        private TGCVector3 director;
        private readonly Skybox Skybox;
        private readonly Terrain Terrain;
        private readonly CameraFPS Camera;
        private readonly GameSoundManager SoundManager;
        private bool NormalMove;
        private bool StalkerModeMove;
        public bool DeathMove { get; set; }
        private float AcumulatedYRotation;
        private float AcumulatedXRotation;
        private float AcumulatedZRotation;
        private float DeathTimeCounter;
        private float EventTimeCounter;
        private float ChangeDirectionTimeCounter;
        public bool AttackedCharacter { get; set; }
        private GameEventsManager Events { get; set; }
        public bool CharacterOnSight { get; set; }
        private TGCMatrix TotalRotation;
        private readonly BulletRigidBodyFactory RigidBodyFactory = BulletRigidBodyFactory.Instance;

        public RigidBody Body { get; set; }
        public TgcMesh Mesh;

        private readonly string MediaDir;

        public Shark(string mediaDir, Skybox skybox, Terrain terrain, CameraFPS camera, GameSoundManager soundManager)
        {
            MediaDir = mediaDir;
            Skybox = skybox;
            Terrain = terrain;
            Camera = camera;
            SoundManager = soundManager;
            Init();
        }

        public void ActivateShark(GameEventsManager events)
        {
            Events = events;
            StalkerModeMove = true;
            NormalMove = true;
            DeathMove = false;
            var position = CalculateInitialPosition();
            Mesh.Transform = TGCMatrix.Scaling(Constants.Scale) * TGCMatrix.Translation(position);
            Body.WorldTransform = Mesh.Transform.ToBulletMatrix();
            director = Constants.DirectorZ;
            TotalRotation = TGCMatrix.Identity;
            EventTimeCounter = Constants.EVENT_TIME;
            DeathTimeCounter = Constants.DEATH_TIME;
            ChangeDirectionTimeCounter = Constants.CHANGE_DIRECTION_TIME;
            AcumulatedXRotation = 0;
            AcumulatedYRotation = 0;
            AcumulatedZRotation = 0;
        }

        private void Init()
        {
            Mesh = new TgcSceneLoader().loadSceneFromFile(MediaDir + Constants.FILE_NAME).Meshes[0];
            TotalRotation = TGCMatrix.Identity;
            EventTimeCounter = Constants.EVENT_TIME;
            DeathTimeCounter = Constants.DEATH_TIME;
            ChangeDirectionTimeCounter = Constants.CHANGE_DIRECTION_TIME;
            NormalMove = false;
            StalkerModeMove = false;
            DeathMove = false;
            AcumulatedYRotation = 0;
            AcumulatedXRotation = 0;
            AcumulatedZRotation = 0;
            director = Constants.DirectorZ;
            Mesh.Transform = TGCMatrix.Scaling(Constants.Scale) * TGCMatrix.Translation(Constants.StartPosition);
            Body = RigidBodyFactory.CreateBox(Constants.SharkBodySize, Constants.MASS, Constants.StartPosition, 0, 0, 0, 0, false);
        }

        public void Update(float elapsedTime)
        {
            var speed = 1000f;
            var headPosition = GetHeadPosition();

            Body.ActivationState = ActivationState.ActiveTag;
            Body.AngularVelocity = Vector3.Zero;

            if (!StalkerModeMove && !NormalMove && !DeathMove)
            {
                return;
            }

            if (DeathMove)
            {
                PerformDeathMove(elapsedTime);
                SoundManager.SharkStalking.stop();
            }
            else if (StalkerModeMove && CanSeekPlayer(out float rotationAngle, out TGCVector3 rotationAxis))
            {
                PerformStalkerMove(elapsedTime, speed, rotationAngle, rotationAxis);
                SoundManager.SharkStalking.play();
            }
            else if (NormalMove)
            {
                PerformNormalMove(elapsedTime, speed, headPosition);
                SoundManager.SharkStalking.stop();
            }

            if (EventTimeCounter <= 0)
            {
                ManageEndOfAttack();
            }

            if (DeathTimeCounter <= 0)
            {
                ManageEndOfDeath();
            }
        }

        public void SetShader(Effect fogShader, string technique)
        {
            Mesh.Effect = fogShader;
            Mesh.Technique = technique;
        }

        public void Render()
        {
            Mesh.Transform = TGCMatrix.Scaling(Constants.Scale) * new TGCMatrix(Body.InterpolationWorldTransform);
            Mesh.BoundingBox.transform(Mesh.Transform);
            Mesh.Render();
        }

        public void Dispose()
        {
            Body.Dispose();
            Mesh.Dispose();
        }
        public void ChangeSharkWay()
        {
            var rotation = TGCMatrix.RotationY(FastMath.PI_HALF * -RotationYSign());
            director = Constants.DirectorZ;
            director.TransformCoordinate(rotation);
            Mesh.Transform = rotation * TGCMatrix.Translation(new TGCVector3(Body.CenterOfMassPosition));
            Body.WorldTransform = Mesh.Transform.ToBulletMatrix();
            AcumulatedXRotation = 0;
            AcumulatedYRotation = 0;
            TotalRotation = rotation;
        }

        public void EndSharkAttack()
        {
            NormalMove = false;
            StalkerModeMove = false;
            Mesh.Transform = TGCMatrix.Scaling(Constants.Scale) * TGCMatrix.Translation(Constants.StartPosition);
            Body.WorldTransform = Mesh.Transform.ToBulletMatrix();
        }

        private void PerformNormalMove(float elapsedTime, float speed, TGCVector3 headPosition)
        {
            CharacterOnSight = false;
            var XRotation = 0f;
            var YRotation = 0f;
            ChangeDirectionTimeCounter -= elapsedTime;
            Terrain.world.InterpoledHeight(headPosition.X, headPosition.Z, out float floorHeight);
            var distanceToFloor = Body.CenterOfMassPosition.Y - floorHeight;
            var XRotationStep = FastMath.PI * 0.1f * elapsedTime;
            var YRotationStep = FastMath.PI * 0.4f * elapsedTime;

            var distanceToWater = Constants.WATER_HEIGHT - floorHeight - 200;
            TGCVector2 sharkRangePosition =
                new TGCVector2(Constants.SHARK_HEIGHT.X, FastMath.Min(distanceToWater, Constants.SHARK_HEIGHT.Y));

            if (distanceToFloor < sharkRangePosition.X - 150 && AcumulatedXRotation < Constants.MaxAxisRotation)
            {
                XRotation = XRotationStep;
            }
            else if (FastUtils.IsNumberBetweenInterval(distanceToFloor, sharkRangePosition) && AcumulatedXRotation > 0.0012)
            {
                XRotation = -XRotationStep;
            }
            else if (distanceToFloor > sharkRangePosition.Y + 150 && AcumulatedXRotation > -Constants.MaxAxisRotation)
            {
                XRotation = -XRotationStep;
            }
            else if (FastUtils.IsNumberBetweenInterval(distanceToFloor, sharkRangePosition) && AcumulatedXRotation < -0.0012)
            {
                XRotation = XRotationStep;
            }

            if (ChangeDirectionTimeCounter <= 0)
            {
                if (FastMath.Abs(AcumulatedYRotation) < Constants.MaxYRotation)
                {
                    YRotation = YRotationStep * RotationYSign();
                }
                else
                {
                    ChangeDirectionTimeCounter = Constants.CHANGE_DIRECTION_TIME;
                }
            }
            else
            {
                AcumulatedYRotation = 0;
            }

            AcumulatedXRotation += XRotation;
            AcumulatedYRotation += YRotation;

            Body.ActivationState = ActivationState.ActiveTag;
            TGCMatrix rotation = TGCMatrix.Identity;
            if (XRotation != 0 || FastMath.Abs(AcumulatedXRotation) > 0.0012)
            {
                var rotationAxis = TGCVector3.Cross(TGCVector3.Up, director);
                director.TransformCoordinate(TGCMatrix.RotationAxis(rotationAxis, XRotation));
                rotation = TGCMatrix.RotationAxis(rotationAxis, XRotation);
                speed /= 1.5f;
            }
            else if (YRotation != 0)
            {
                director.TransformCoordinate(TGCMatrix.RotationY(YRotation));
                rotation = TGCMatrix.RotationY(YRotation);
            }
            TotalRotation *= rotation;
            Mesh.Transform = TotalRotation * TGCMatrix.Translation(new TGCVector3(Body.CenterOfMassPosition));
            Body.WorldTransform = Mesh.Transform.ToBulletMatrix();
            Body.LinearVelocity = director.ToBulletVector3() * -speed;
        }

        private void PerformStalkerMove(float elapsedTime, float speed, float rotationAngle, TGCVector3 rotationAxis)
        {
            CharacterOnSight = true;
            var actualDirector = -1 * director;
            EventTimeCounter -= elapsedTime;
            var RotationStep = FastMath.PI * 0.3f * elapsedTime;

            if (rotationAngle <= RotationStep)
            {
                return;
            }

            actualDirector.TransformCoordinate(TGCMatrix.RotationAxis(rotationAxis, RotationStep));
            var newRotation = TGCMatrix.RotationAxis(rotationAxis, RotationStep);
            TotalRotation *= newRotation;

            Mesh.Transform = TotalRotation * TGCMatrix.Translation(new TGCVector3(Body.CenterOfMassPosition));
            Body.WorldTransform = Mesh.Transform.ToBulletMatrix();

            director = -1 * actualDirector;
            Body.LinearVelocity = director.ToBulletVector3() * -speed;
        }

        private void PerformDeathMove(float elapsedTime)
        {
            CharacterOnSight = false;
            DeathTimeCounter -= elapsedTime;
            var RotationStep = FastMath.PI * 0.4f * elapsedTime;
            if (FastUtils.GreaterThan(AcumulatedZRotation, Constants.MaxZRotation))
            {
                return;
            }

            AcumulatedZRotation += RotationStep;
            TotalRotation *= TGCMatrix.RotationAxis(director, RotationStep);
            Mesh.Transform = TotalRotation * TGCMatrix.Translation(new TGCVector3(Body.CenterOfMassPosition));
            Body.WorldTransform = Mesh.Transform.ToBulletMatrix();
            Body.LinearVelocity = Constants.DirectorY.ToBulletVector3() * 200;
        }


        private void ManageEndOfAttack()
        {
            if (StalkerModeMove)
            {
                ChangeSharkWay();
            }

            StalkerModeMove = false;
            if (!Skybox.Contains(Body))
            {
                EndSharkAttack();
                Events.InformFinishFromAttack();
            }
        }

        private void ManageEndOfDeath()
        {
            DeathMove = false;
            Events.InformFinishFromAttack();
            EndSharkAttack();
        }

        private TGCVector3 CalculateInitialPosition()
        {
            var outOfSkyboxPosition = Camera.Position + TGCVector3.Mul(director, new TGCVector3(0, 0, Skybox.Radius + 300));
            Terrain.world.InterpoledHeight(outOfSkyboxPosition.X, outOfSkyboxPosition.Z, out float Y);
            return new TGCVector3(outOfSkyboxPosition.X, Y + 600, outOfSkyboxPosition.Z);
        }


        private bool CanSeekPlayer(out float rotationAngle, out TGCVector3 rotationAxis)
        {
            var actualDirector = -1 * director;
            var directorToPlayer = TGCVector3.Normalize(Camera.Position - new TGCVector3(Body.CenterOfMassPosition));
            var NormalVectorFromDirAndPlayer = FastUtils.ObtainNormalVector(actualDirector, directorToPlayer);
            if (NormalVectorFromDirAndPlayer.Length() > 0.98f)
            {
                var RotationToPlayer = FastUtils.AngleBetweenVectors(actualDirector, directorToPlayer);
                if (RotationToPlayer <= FastMath.QUARTER_PI && RotationToPlayer != 0)
                {
                    rotationAngle = RotationToPlayer;
                    rotationAxis = NormalVectorFromDirAndPlayer;
                    return true;
                }
            }
            rotationAngle = 0;
            rotationAxis = TGCVector3.Empty;
            return false;
        }

        private float RotationYSign()
        {
            var bodyToSkyboxCenterVector = TGCVector3.Normalize(Skybox.GetSkyboxCenter() - new TGCVector3(Body.CenterOfMassPosition));
            var actualDirector = -1 * director;
            var normalVector = TGCVector3.Cross(actualDirector, bodyToSkyboxCenterVector);
            return normalVector.Y > 0 ? 1 : -1;
        }

        private TGCVector3 GetHeadPosition() => new TGCVector3(Body.CenterOfMassPosition) + director * -560;

        private bool IsNearFromSurface(float sharkHeight) =>
                                        Constants.WATER_HEIGHT - sharkHeight <= Constants.MIN_DISTANCE_TO_SURFACE;

    }
}

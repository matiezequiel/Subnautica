using BulletSharp;
using BulletSharp.Math;

namespace TGC.Group.Model.Objects
{
    internal class PhysicalWorld
    {
        private Vector3 gravityZero = Vector3.Zero;
        private CollisionDispatcher dispatcher;
        private DefaultCollisionConfiguration collisionConfiguration;
        private SequentialImpulseConstraintSolver constraintSolver;
        private BroadphaseInterface overlappingPairCache;
        public DiscreteDynamicsWorld dynamicsWorld;

        public PhysicalWorld() => Init();

        public void AddBodyToTheWorld(RigidBody Body) => dynamicsWorld.AddRigidBody(Body);

        public void AddContactPairTest(RigidBody firstBody, RigidBody secondBody, ContactResultCallback callback) =>
            dynamicsWorld.ContactPairTest(firstBody, secondBody, callback);

        public void Dispose()
        {
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }

        private void Init()
        {
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase();
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration) { Gravity = gravityZero };
        }

        public void RemoveBodyToTheWorld(RigidBody Body) => dynamicsWorld.RemoveRigidBody(Body);
    }
}

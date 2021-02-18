using BulletSharp;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Linq;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Objects
{
    internal class Common
    {
        public struct TypeCommon
        {
            public string Name;
            public TgcMesh Mesh;
            public int Quantity;
            public RigidBody Body { get; set; }
        }

        private struct Constants
        {
            public static string NAME_CORAL_NORMAL = "NORMALCORAL";
            public static string NAME_CORAL_TREE = "TREECORAL";
            public static string NAME_CORAL_SPIRAL = "SPIRALCORAL";
            public static string NAME_ORE_GOLD = "GOLD";
            public static string NAME_ORE_IRON = "IRON";
            public static string NAME_ORE_SILVER = "SILVER";
            public static string NAME_ROCK = "ROCK";
            public static string NAME_NORMAL_FISH = "NORMALFISH";
            public static string NAME_YELLOW_FISH = "YELLOWFISH";
            public static int QUANTITY_CORAL_NORMAL = 30;
            public static int QUANTITY_CORAL_TREE = 30;
            public static int QUANTITY_CORAL_SPIRAL = 30;
            public static int QUANTITY_ORE_IRON = 30;
            public static int QUANTITY_ORE_SILVER = 30;
            public static int QUANTITY_ORE_GOLD = 30;
            public static int QUANTITY_ROCK = 30;
            public static int QUANTITY_NORMAL_FISH = 30;
            public static int QUANTITY_YELLOW_FISH = 30;
            public static TGCVector3 Scale = new TGCVector3(10, 10, 10);
        }

        private TgcMesh coralNormal;
        private TgcMesh coralTree;
        private TgcMesh coralSpiral;
        private TgcMesh oreIron;
        private TgcMesh oreSilver;
        private TgcMesh oreGold;
        private TgcMesh rock;
        private TgcMesh normalFish;
        private TgcMesh yellowFish;
        private readonly string MediaDir;

        private readonly BulletRigidBodyFactory RigidBodyFactory = BulletRigidBodyFactory.Instance;

        public List<TypeCommon> ListCorals = new List<TypeCommon>();
        public List<TypeCommon> ListOres = new List<TypeCommon>();
        public List<TypeCommon> ListRock = new List<TypeCommon>();
        public List<TypeCommon> ListFishes = new List<TypeCommon>();

        public Common(string mediaDir)
        {
            MediaDir = mediaDir;
            Init();
        }

        public void Dispose()
        {
            ListCorals.ForEach(coral => coral.Mesh.Dispose());
            ListOres.ForEach(ore => ore.Mesh.Dispose());
            ListRock.ForEach(rock => rock.Mesh.Dispose());
        }

        private void Init()
        {
            InitializerFishes();
            GenerateDuplicates(coralNormal, ref ListCorals, quantity: Constants.QUANTITY_CORAL_NORMAL);
            GenerateDuplicates(coralTree, ref ListCorals, quantity: Constants.QUANTITY_CORAL_TREE);
            GenerateDuplicates(coralSpiral, ref ListCorals, quantity: Constants.QUANTITY_CORAL_SPIRAL);
            GenerateDuplicates(oreIron, ref ListOres, quantity: Constants.QUANTITY_ORE_IRON);
            GenerateDuplicates(oreSilver, ref ListOres, quantity: Constants.QUANTITY_ORE_SILVER);
            GenerateDuplicates(oreGold, ref ListOres, quantity: Constants.QUANTITY_ORE_GOLD);
            GenerateDuplicates(normalFish, ref ListFishes, quantity: Constants.QUANTITY_NORMAL_FISH, createRB: false);
            GenerateDuplicates(yellowFish, ref ListFishes, quantity: Constants.QUANTITY_YELLOW_FISH, createRB: false);
        }

        private void InitializerFishes()
        {
            LoadInitial(ref coralNormal, Constants.NAME_CORAL_NORMAL);
            LoadInitial(ref coralSpiral, Constants.NAME_CORAL_SPIRAL);
            LoadInitial(ref coralTree, Constants.NAME_CORAL_TREE);
            LoadInitial(ref oreGold, Constants.NAME_ORE_GOLD);
            LoadInitial(ref oreIron, Constants.NAME_ORE_IRON);
            LoadInitial(ref oreSilver, Constants.NAME_ORE_SILVER);
            LoadInitial(ref rock, Constants.NAME_ROCK);
            LoadInitial(ref normalFish, Constants.NAME_NORMAL_FISH);
            LoadInitial(ref yellowFish, Constants.NAME_YELLOW_FISH);
        }

        private void LoadInitial(ref TgcMesh mesh, string meshName)
        {
            mesh = new TgcSceneLoader().loadSceneFromFile(MediaDir + meshName + "-TgcScene.xml").Meshes[0];
            mesh.Name = meshName;
        }

        public void LocateObjects()
        {
            ListCorals.ForEach(coral =>
            {
                coral.Body.Translate(coral.Mesh.Position.ToBulletVector3());
                coral.Mesh.BoundingBox.scaleTranslate(coral.Mesh.Position, Constants.Scale);
            });

            ListOres.ForEach(ore =>
            {
                ore.Body.Translate(ore.Mesh.Position.ToBulletVector3());
                ore.Mesh.BoundingBox.scaleTranslate(ore.Mesh.Position, Constants.Scale);
            });

            ListRock.ForEach(rock =>
            {
                rock.Body.Translate(rock.Mesh.Position.ToBulletVector3());
                rock.Mesh.BoundingBox.scaleTranslate(rock.Mesh.Position, Constants.Scale);
            });

            ListFishes.ForEach(fish => fish.Mesh.BoundingBox.scaleTranslate(fish.Mesh.Position, Constants.Scale));
        }

        private void CreateRigidBody(ref TypeCommon common)
        {
            common.Body = RigidBodyFactory.CreateRigidBodyFromTgcMesh(common.Mesh);
            common.Body.CenterOfMassTransform = TGCMatrix.Translation(common.Mesh.Position).ToBulletMatrix();
            common.Body.CollisionShape.LocalScaling = Constants.Scale.ToBulletVector3();
        }

        public void GenerateDuplicates(TgcMesh common, ref List<TypeCommon> commons, int quantity, bool createRB = true)
        {
            foreach (int index in Enumerable.Range(0, quantity))
            {
                TypeCommon newCommon = new TypeCommon
                {
                    Quantity = index,
                    Name = common.Name + "_" + index
                };
                newCommon.Mesh = common.createMeshInstance(newCommon.Name);
                newCommon.Mesh.Transform = TGCMatrix.Scaling(Constants.Scale);
                if (createRB)
                {
                    CreateRigidBody(ref newCommon);
                }

                commons.Add(newCommon);
            }
        }

        public void Render()
        {
            ListCorals.ForEach(coral => coral.Mesh.Render());
            ListOres.ForEach(ore => ore.Mesh.Render());
            ListRock.ForEach(rock => rock.Mesh.Render());
        }

        public void SetShader(Effect fogShader, string technique)
        {
            ListCorals.ForEach(coral => { coral.Mesh.Effect = fogShader; coral.Mesh.Technique = technique; });
            ListOres.ForEach(ore =>
            {
                ore.Mesh.Effect = fogShader;
                if (ore.Name.ToLower().Contains("gold"))
                {
                    ore.Mesh.Technique = "Gold";
                }
                else if (ore.Name.ToLower().Contains("silver"))
                {
                    ore.Mesh.Technique = "Silver";
                }
                else if (ore.Name.ToLower().Contains("iron"))
                {
                    ore.Mesh.Technique = "Iron";
                }
                else
                {
                    ore.Mesh.Technique = technique;
                }
            });
            ListRock.ForEach(rock => { rock.Mesh.Effect = fogShader; rock.Mesh.Technique = technique; });
            ListFishes.ForEach(fish => { fish.Mesh.Effect = fogShader; fish.Mesh.Technique = technique; });
        }

        public List<TgcMesh> AllMeshes()
        {
            var meshes = new List<TypeCommon>();
            meshes.AddRange(ListCorals);
            meshes.AddRange(ListOres);
            meshes.AddRange(ListRock);
            return meshes.Select(mesh => mesh.Mesh).ToList();
        }
    }
}

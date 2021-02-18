using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Linq;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Objects
{
    internal class Vegetation
    {
        public struct TypeVegetation
        {
            public int Quantity;
            public string Name;
            public TgcMesh Mesh;
        }

        private struct Constants
        {
            public static string NAME_ALGA_1 = "alga1";
            public static string NAME_ALGA_2 = "alga2";
            public static string NAME_ALGA_3 = "alga3";
            public static string NAME_ALGA_4 = "alga4";
            public static int QUANTITY_ALGA_1 = 75;
            public static int QUANTITY_ALGA_2 = 75;
            public static int QUANTITY_ALGA_3 = 75;
            public static int QUANTITY_ALGA_4 = 75;
            public static TGCVector3 Scale = new TGCVector3(7, 7, 7);
        }

        private readonly string MediaDir;
        private TypeVegetation alga1;
        private TypeVegetation alga2;
        private TypeVegetation alga3;
        private TypeVegetation alga4;

        public List<TypeVegetation> ListAlgas = new List<TypeVegetation>();

        public Vegetation(string mediaDir)
        {
            MediaDir = mediaDir;
            Init();
        }

        public void Dispose() => ListAlgas.ForEach(vegetation => vegetation.Mesh.Dispose());

        private void Init()
        {
            InitializerVegetation();
            GenerateDuplicates(alga1, ref ListAlgas);
            GenerateDuplicates(alga2, ref ListAlgas);
            GenerateDuplicates(alga3, ref ListAlgas);
            GenerateDuplicates(alga4, ref ListAlgas);
        }

        private void InitializerVegetation()
        {
            alga1.Name = Constants.NAME_ALGA_1;
            alga1.Quantity = Constants.QUANTITY_ALGA_1;
            LoadInitial(ref alga1);

            alga2.Name = Constants.NAME_ALGA_2;
            alga2.Quantity = Constants.QUANTITY_ALGA_2;
            LoadInitial(ref alga2);

            alga3.Name = Constants.NAME_ALGA_3;
            alga3.Quantity = Constants.QUANTITY_ALGA_3;
            LoadInitial(ref alga3);

            alga4.Name = Constants.NAME_ALGA_4;
            alga4.Quantity = Constants.QUANTITY_ALGA_4;
            LoadInitial(ref alga4);
        }

        private void LoadInitial(ref TypeVegetation vegetation)
        {
            vegetation.Mesh = new TgcSceneLoader().loadSceneFromFile(MediaDir + vegetation.Name + "-TgcScene.xml").Meshes[0];
            vegetation.Mesh.Name = vegetation.Name;
        }

        public void GenerateDuplicates(TypeVegetation vegetation, ref List<TypeVegetation> vegetations)
        {
            foreach (int index in Enumerable.Range(0, vegetation.Quantity))
            {
                TypeVegetation newVegetation = new TypeVegetation
                {
                    Quantity = index,
                    Name = vegetation.Name + "_" + index
                };

                newVegetation.Mesh = vegetation.Mesh.createMeshInstance(newVegetation.Name);
                newVegetation.Mesh.AlphaBlendEnable = true;
                newVegetation.Mesh.Transform = TGCMatrix.Scaling(Constants.Scale);
                newVegetation.Mesh.BoundingBox.scaleTranslate(newVegetation.Mesh.Position, Constants.Scale);
                vegetations.Add(newVegetation);
            }
        }

        public void Render() => ListAlgas.ForEach(vegetation => vegetation.Mesh.Render());

        public void SetShader(Effect fogShader, string technique) =>
            ListAlgas.ForEach(alga => { alga.Mesh.Effect = fogShader; alga.Mesh.Technique = technique; });
    }
}

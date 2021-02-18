using BulletSharp.Math;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Exam.Camara;

namespace TGC.Exam.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TGCExample
    {
        private List<TgcMesh> meshes = new List<TgcMesh>();
        private TgcMesh sphereOne, sphereTwo, sphereThree, plane, robot, lightBoxOne, lightBoxTwo;

        private TGCVector3 spheresCenter = TGCVector3.Up * 25f;
        private TGCVector3 spheresScale = TGCVector3.One * 4f;

        private Surface depthStencil;
        private Texture renderTarget;
        private VertexBuffer fullScreenQuad;

        private Effect effect;

        private bool enableLighting = true;

        private TGCVector3 lightOnePosition, lightTwoPosition;

        private float timer = 0f;

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "TgcExamShader.fx");

            InitializeMeshes();

            plane.Transform = TGCMatrix.Translation(-50f, 0f, -50f);

            sphereOne.Transform = TGCMatrix.Scaling(spheresScale) *  TGCMatrix.Translation(spheresCenter);
            sphereTwo.Transform = TGCMatrix.Scaling(spheresScale) * TGCMatrix.Translation(spheresCenter + new TGCVector3(10f, 0f, 0f));
            sphereThree.Transform = TGCMatrix.Scaling(spheresScale) * TGCMatrix.Translation(spheresCenter + new TGCVector3(-10f, 0f, 0f));

            //meshes.ForEach(mesh => { mesh.Effect = effect; mesh.Technique = "Default"; });
            FixedTickEnable = true;


            //Aplico efecto solamente en la esfera dos
            sphereTwo.Effect = effect;
            sphereTwo.Technique = "Default";
            //robot.Effect = effect;
            //robot.Technique = "Default";

            Camera = new TGCRotationalCamera(spheresCenter, 50f, Input);

            CreateFullScreenQuad();
            CreateRenderTarget();

            if (enableLighting)
                InitializeLights();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>

        float radio = 20;
        public override void Update()
        {
            timer += ElapsedTime;

            //----------------------Repaso Parcial
            //giros_esferas();

            //----------------------Parcial tema a y c
            //rebote();

            //----------------------Parcial tema b y d
            //giro_en_plano();

            //----------------------Recuperatorio
            //deformacion_esfera();

            UpdateShaderUniforms();
        }

        public void giros_esferas()
        {
            sphereOne.Transform = TGCMatrix.Scaling(spheresScale) * TGCMatrix.RotationY(timer) * TGCMatrix.Translation(spheresCenter);
            sphereTwo.Transform = TGCMatrix.Scaling(spheresScale * 0.1f) * TGCMatrix.RotationY(timer) * TGCMatrix.Translation(new TGCVector3(10f, 0f, 0f)) * sphereOne.Transform;
            sphereThree.Transform = TGCMatrix.Scaling(spheresScale * 0.5f) * TGCMatrix.RotationY(timer) * TGCMatrix.Translation(new TGCVector3(-10f, 0f, 0f)) * sphereTwo.Transform;
        }

        public void rebote()
        {
            timer += ElapsedTime;

            var pos = 27f * FastMath.Abs(FastMath.Sin(timer * 2));

            var posicion = new TGCVector3(0f, pos, 0f);
            var aplastarse = new TGCVector3(4f, 4f, 4f);

            if (pos < 10)
                aplastarse = new TGCVector3(4f, 4f * FastMath.Abs(FastMath.Sin(timer * 4)), 4f);

            sphereOne.Transform = TGCMatrix.Scaling(aplastarse) * TGCMatrix.Translation(posicion);
        }

        public void giro_en_plano()
        {
            timer += ElapsedTime;

            var posicion = new TGCVector3(radio * FastMath.Sin(timer), 0f, radio * FastMath.Cos(timer));

            var posicionNormalizada = posicion;
            posicionNormalizada.Normalize();
            var quaternion = TGCQuaternion.RotationAxis(posicionNormalizada, -timer * 5f);

            sphereOne.Transform = TGCMatrix.Scaling(spheresScale) * TGCMatrix.RotationTGCQuaternion(quaternion) * TGCMatrix.Translation(posicion + new TGCVector3(0f, 4f, 0f));
        }

        public void deformacion_esfera()
        {
            timer += ElapsedTime;

            var variacionY = FastMath.Max(2f, 6f * FastMath.Abs(FastMath.Cos(timer * 1.5f)));
            var variacionZ = FastMath.Max(2f, 6f * FastMath.Abs(FastMath.Sin(timer * 1.5f)));

            var escalado = new TGCVector3(6f, variacionY, variacionZ);

            sphereOne.Transform = TGCMatrix.RotationY(timer) * TGCMatrix.Scaling(escalado) * TGCMatrix.Translation(spheresCenter);
        }


        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            var device = D3DDevice.Instance.Device;

            // Capturamos las texturas de pantalla
            Surface screenRenderTarget = device.GetRenderTarget(0);
            Surface screenDepthSurface = device.DepthStencilSurface;

            // Especificamos que vamos a dibujar en una textura
            Surface surface = renderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, surface);
            device.DepthStencilSurface = depthStencil;

            // Captura de escena en render target
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            device.BeginScene();
            meshes.ForEach(mesh => mesh.Render());
            device.EndScene();
            // Fin de escena


            // Especificamos que vamos a dibujar en pantalla
            device.SetRenderTarget(0, screenRenderTarget);
            device.DepthStencilSurface = screenDepthSurface;

            // Dibujado de textura en full screen quad
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            device.BeginScene();

            effect.Technique = "PostProcess";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, fullScreenQuad, 0);
            effect.SetValue("renderTarget", renderTarget);

            // Dibujamos el full screen quad
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            RenderFPS();
            RenderAxis();
            device.EndScene();

            device.Present();

            surface.Dispose();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            meshes.ForEach(mesh => mesh.Dispose());
        }


        private void InitializeMeshes()
        {
            TgcTexture tiles, lava, stones, water;

            tiles = TgcTexture.createTexture(MediaDir + "Textures/tiles.jpg");
            lava = TgcTexture.createTexture(MediaDir + "Textures/lava.jpg");
            stones = TgcTexture.createTexture(MediaDir + "Textures/stones.bmp");
            water = TgcTexture.createTexture(MediaDir + "Textures/water.bmp");

            TGCSphere baseSphere = new TGCSphere(1f, Color.White, TGCVector3.Empty);
            baseSphere.setTexture(lava);
            baseSphere.updateValues();
            sphereOne = baseSphere.toMesh("sphereOne");
            meshes.Add(sphereOne);
            baseSphere.setTexture(stones);
            sphereTwo = baseSphere.toMesh("sphereTwo");
            meshes.Add(sphereTwo);
            baseSphere.setTexture(water);
            sphereThree = baseSphere.toMesh("sphereThree");
            meshes.Add(sphereThree);

            TgcSceneLoader loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "Robot2-TgcScene.xml");
            robot = scene.Meshes[0];
            robot.Transform = TGCMatrix.Scaling(0.1f, 0.1f, 0.1f) * TGCMatrix.RotationY(FastMath.PI) * TGCMatrix.Translation(TGCVector3.Up * 40f);
            meshes.Add(robot);

            TgcPlane tgcPlane = new TgcPlane(TGCVector3.Empty, TGCVector3.One * 100f, TgcPlane.Orientations.XZplane, tiles, 10f, 10f);
            plane = tgcPlane.toMesh("plane");
            meshes.Add(plane);
        }

        private void UpdateShaderUniforms()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array(Camera.Position));

            effect.SetValue("screenWidth", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screenHeight", d3dDevice.PresentationParameters.BackBufferHeight);

            effect.SetValue("timer", timer);

            if (enableLighting)
            {
                lightOnePosition = spheresCenter + new TGCVector3(FastMath.Cos(timer) * 20f, 0f, FastMath.Sin(timer) * 20f);
                lightTwoPosition = spheresCenter + new TGCVector3(-FastMath.Sin(timer) * 20f, 0f, FastMath.Cos(timer) * 20f);

                lightBoxOne.Transform = TGCMatrix.Translation(lightOnePosition);
                lightBoxTwo.Transform = TGCMatrix.Translation(lightTwoPosition);

                effect.SetValue("lights[0].position", TGCVector3.TGCVector3ToFloat3Array(lightOnePosition));
                effect.SetValue("lights[1].position", TGCVector3.TGCVector3ToFloat3Array(lightTwoPosition));
            }
        }

        private void InitializeLights()
        {
            
            
            var tgcLightBoxOne = new TGCBox();
            tgcLightBoxOne.Color = Color.DarkOrange;
            tgcLightBoxOne.Size = TGCVector3.One;
            tgcLightBoxOne.Transform = TGCMatrix.Translation(lightOnePosition);
            tgcLightBoxOne.updateValues();
            lightBoxOne = tgcLightBoxOne.ToMesh("lightBoxOne");
            meshes.Add(lightBoxOne);
         
     

            var tgcLightBoxTwo = new TGCBox();
            tgcLightBoxTwo.Color = Color.White;
            tgcLightBoxTwo.Size = TGCVector3.One;
            tgcLightBoxTwo.Transform = TGCMatrix.Translation(lightTwoPosition);
            tgcLightBoxTwo.updateValues();
            lightBoxTwo = tgcLightBoxTwo.ToMesh("lightBoxTwo");
            meshes.Add(lightBoxTwo);
            

            effect.SetValue("lights[0].color", new float[3] { 0.5f, 0.2f, 0.1f });
            effect.SetValue("lights[1].color", new float[3] { 1f, 1f, 1f });
        }

        private void CreateFullScreenQuad()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            // Creamos un FullScreen Quad
            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };

            // Vertex buffer de los triangulos
            fullScreenQuad = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            fullScreenQuad.SetData(vertices, 0, LockFlags.None);
        }

        private void CreateRenderTarget()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            depthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            renderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
        }

    }
}
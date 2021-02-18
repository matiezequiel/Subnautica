using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Interpolation;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Group.Utils
{
    internal class FullQuad
    {
        private Effect Effect { get; set; }
        private Surface DepthStencil { get; set; }
        private Surface OldDepthStencil { get; set; }
        private Surface OldRenderTarget { get; set; }
        private Surface Surf { get; set; }
        private VertexBuffer FullScreenQuad { get; set; }
        private Texture RenderTarget2D { get; set; }
        private TgcTexture AlarmTexture { get; set; }
        private TgcTexture DivingHelmetTexture { get; set; }
        private TgcTexture PDA { get; set; }
        private InterpoladorVaiven IntVaivenAlarm;

        private readonly Device Device = D3DDevice.Instance.Device;
        private float Time = 0;

        public string ShadersDir { get; set; }
        public string MediaDir { get; set; }
        public bool RenderTeleportEffect { get; set; }
        public bool RenderAlarmEffect { get; set; }
        public bool RenderPDA { get; set; }
        private readonly float ElapsedTime;

        public FullQuad(string mediaDir, string shadersDir, float elapsedTime)
        {
            MediaDir = mediaDir;
            ShadersDir = shadersDir;
            ElapsedTime = elapsedTime;
            Initializer();
        }

        private void Initializer()
        {
            CustomVertex.PositionTextured[] vertex =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };

            FullScreenQuad = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, Device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            FullScreenQuad.SetData(vertex, 0, LockFlags.None);

            RenderTarget2D = new Texture(Device, Device.PresentationParameters.BackBufferWidth, Device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
            DepthStencil = Device.CreateDepthStencilSurface(Device.PresentationParameters.BackBufferWidth, Device.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            Effect = TGCShaders.Instance.LoadEffect(ShadersDir + "PostProcess.fx");
            Effect.Technique = "DefaultTechnique";
            AlarmTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Textures\alarm.png");
            DivingHelmetTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Images\divingHelmet.png");
            PDA = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + @"Images\PDA.png");
            IntVaivenAlarm = new InterpoladorVaiven
            {
                Min = 0,
                Max = 1,
                Speed = 0.04f
            };
            IntVaivenAlarm.reset();
            Effect.SetValue("texture_alarm", AlarmTexture.D3dTexture);
            Effect.SetValue("texture_diving_helmet", DivingHelmetTexture.D3dTexture);
            Effect.SetValue("texture_PDA", PDA.D3dTexture);
            Effect.SetValue("Color", Color.DarkSlateBlue.ToArgb());
        }

        public void SetTime(float value) => Time = FastMath.Clamp(Time + value, 0, 10);

        public void PreRenderMeshes()
        {
            TexturesManager.Instance.clearAll();

            OldRenderTarget = Device.GetRenderTarget(0);
            OldDepthStencil = Device.DepthStencilSurface;
            Surf = RenderTarget2D.GetSurfaceLevel(0);

            Device.SetRenderTarget(0, Surf);
            Device.DepthStencilSurface = DepthStencil;
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1f, 0);

            Device.BeginScene();
        }

        public void Render()
        {
            Device.EndScene();
            Surf.Dispose();

            Device.SetRenderTarget(0, OldRenderTarget);
            Device.DepthStencilSurface = OldDepthStencil;

            Device.BeginScene();
            Device.VertexFormat = CustomVertex.PositionTextured.Format;

            Device.SetStreamSource(0, FullScreenQuad, 0);
            Effect.SetValue("render_target2D", RenderTarget2D);

            if (RenderTeleportEffect)
            {
                Effect.Technique = "Darken";
                Effect.SetValue("time", Time);
            }
            else
            {
                Time = 0;
                if (RenderAlarmEffect)
                {
                    Effect.Technique = "AlarmTechnique";
                    Effect.SetValue("alarmScaleFactor", IntVaivenAlarm.update(ElapsedTime));
                }
                else
                {
                    Effect.Technique = "DivingHelmet";
                }
            }

            if (RenderPDA)
            {
                Effect.Technique = "PDA";
            }

            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1f, 0);

            Effect.Begin(FX.None);
            Effect.BeginPass(0);
            Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            Effect.EndPass();
            Effect.End();
        }

        public void Dispose()
        {
            if (FullScreenQuad != null && !FullScreenQuad.Disposed)
            {
                FullScreenQuad.Dispose();
            }

            if (Effect != null && !Effect.Disposed)
            {
                Effect.Dispose();
            }

            if (AlarmTexture != null && !AlarmTexture.D3dTexture.Disposed)
            {
                AlarmTexture.dispose();
            }

            if (RenderTarget2D != null && !RenderTarget2D.Disposed)
            {
                RenderTarget2D.Dispose();
            }

            if (DepthStencil != null && !DepthStencil.Disposed)
            {
                DepthStencil.Dispose();
            }

            if (OldDepthStencil != null && !OldDepthStencil.Disposed)
            {
                OldDepthStencil.Dispose();
            }

            if (OldRenderTarget != null && !OldRenderTarget.Disposed)
            {
                OldRenderTarget.Dispose();
            }
        }
    }
}

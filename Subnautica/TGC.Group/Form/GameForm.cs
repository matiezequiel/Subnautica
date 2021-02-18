using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.Shaders;
using TGC.Core.Sound;
using TGC.Core.Textures;
using TGC.Group.Model;

namespace TGC.Group.Form
{
    /// <summary>
    ///     GameForm es el formulario de entrada, el mismo invocara a nuestro modelo  que extiende TGCExample, e inicia el
    ///     render loop.
    /// </summary>
    public partial class GameForm : System.Windows.Forms.Form
    {
        /// <summary>
        ///     Constructor de la ventana.
        /// </summary>
        public GameForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            Image = new Bitmap(Game.Default.MediaDirectory + @"Images\PRE_CARGA_INICIAL.jpg");
            panel3D.BackgroundImage = Image;
        }

        private Bitmap Image { get; set; }

        /// <summary>
        ///     Ejemplo del juego a correr
        /// </summary>
        private TGCExample Modelo { get; set; }

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        private bool ApplicationRunning { get; set; }

        /// <summary>
        ///     Permite manejar el sonido.
        /// </summary>
        private TgcDirectSound DirectSound { get; set; }

        /// <summary>
        ///     Permite manejar los inputs de la computadora.
        /// </summary>
        private TgcD3dInput Input { get; set; }

        private void GameForm_Load(object sender, EventArgs e)
        {
            InitGraphics();
            Text = Modelo.Name + @" - " + Modelo.Description;
            panel3D.Focus();
            InitRenderLoop();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ApplicationRunning)
            {
                ShutDown();
            }
        }

        /// <summary>
        ///     Inicio todos los objetos necesarios para cargar el ejemplo y directx.
        /// </summary>
        public void InitGraphics()
        {
            ApplicationRunning = true;
            D3DDevice.Instance.InitializeD3DDevice(panel3D);

            //Inicio inputs
            Input = new TgcD3dInput();
            Input.Initialize(this, panel3D);

            //Inicio sonido
            DirectSound = new TgcDirectSound();
            try
            {
                DirectSound.InitializeD3DDevice(panel3D);
            }
            catch (ApplicationException ex)
            {
                throw new Exception("No se pudo inicializar el sonido", ex);
            }

            //Directorio actual de ejecución
            var currentDirectory = Environment.CurrentDirectory + "\\";

            //Cargar shaders del framework
            TGCShaders.Instance.LoadCommonShaders(currentDirectory + Game.Default.ShadersDirectory, D3DDevice.Instance);

            Modelo = new GameModel(currentDirectory + Game.Default.MediaDirectory,
                currentDirectory + Game.Default.ShadersDirectory);

            ExecuteModel();
        }

        /// <summary>
        ///     Comienzo el loop del juego.
        /// </summary>
        public void InitRenderLoop()
        {
            while (ApplicationRunning)
            {
                //Renderizo si es que hay un ejemplo activo.
                if (Modelo != null)
                {
                    //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios.
                    if (ApplicationActive())
                    {
                        Modelo.Tick();
                        if (Input.keyDown(Key.Escape))
                        {
                            Close();
                        }

                        Cursor.Hide();
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                // Process application messages.
                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Indica si la aplicacion esta activa.
        ///     Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
        /// </summary>
        public bool ApplicationActive()
        {
            if (ContainsFocus)
            {
                return true;
            }

            foreach (var form in OwnedForms)
            {
                if (form.ContainsFocus)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Arranca a ejecutar un ejemplo.
        ///     Para el ejemplo anterior, si hay alguno.
        /// </summary>
        public void ExecuteModel()
        {
            //Ejecutar Init
            try
            {
                Modelo.ResetDefaultConfig();
                Modelo.DirectSound = DirectSound;
                Modelo.Input = Input;
                Modelo.Init();
                panel3D.Focus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"Error en Init() del juego", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Deja de ejecutar el ejemplo actual
        /// </summary>
        public void StopCurrentExample()
        {
            if (Modelo != null)
            {
                Modelo.Dispose();
                Modelo = null;
            }
        }

        /// <summary>
        ///     Finalizar aplicacion
        /// </summary>
        public void ShutDown()
        {
            ApplicationRunning = false;

            StopCurrentExample();

            //Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Dispose();
            TexturesPool.Instance.clearAll();
        }
    }
}

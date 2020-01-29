using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using NmeaParser;
using NmeaParser.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SampleApp.WinDesktop
{
    /// <summary>
    /// Interaction logic for View3D.xaml
    /// </summary>
    public partial class View3D : UserControl
    {
        public View3D()
        {
            InitializeComponent();
            InitializeScene();
            this.IsVisibleChanged += View3D_IsVisibleChanged;
        }

        private void View3D_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible)
            {
                graphic3D.Geometry = null;
                if (NmeaDevice != null)
                    NmeaDevice.MessageReceived += NmeaDevice_MessageReceived;
            }
            else
            {
                if (NmeaDevice != null)
                    NmeaDevice.MessageReceived -= NmeaDevice_MessageReceived;
            }
        }

        private void NmeaDevice_MessageReceived(object sender, NmeaMessageReceivedEventArgs e)
        {
            if (e.Message is Rmc rmc)
                UpdateLocation(rmc.Latitude, rmc.Longitude, rmc.Course);
        }

        private Graphic graphic3D;

        private async void InitializeScene()
        {
            sceneView.Scene = new Scene(BasemapType.Imagery);
            sceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));
            sceneView.GraphicsOverlays.Add(new GraphicsOverlay() { Id = "Position", Renderer = new SimpleRenderer() });
            sceneView.GraphicsOverlays["Position"].Renderer.SceneProperties.HeadingExpression = "[Heading]";

            graphic3D = new Esri.ArcGISRuntime.UI.Graphic() { Symbol = SimpleMarkerSceneSymbol.CreateSphere(System.Drawing.Color.FromArgb(127, 255, 0, 0), 10) };
            var symb = await ModelSceneSymbol.CreateAsync(new Uri("car.glb", UriKind.Relative));
            symb.Width = 3; symb.Depth = 5; symb.Height = 2; symb.AnchorPosition = SceneSymbolAnchorPosition.Bottom;
            graphic3D.Symbol = symb;
            sceneView.GraphicsOverlays["Position"].Graphics.Add(graphic3D);

            sceneView.CameraController = new OrbitGeoElementCameraController(sceneView.GraphicsOverlays["Position"].Graphics[0], 200);
        }
        
        private int updateId = 0;
        private async void UpdateLocation(double latitude, double longitude, double newHeading)
        {
            // Handle 3D updates on 3D Scene
            var cid = ++updateId;
            var start = graphic3D.Geometry as MapPoint;
            if (start == null)
            {
                graphic3D.Attributes["Heading"] = newHeading;
                graphic3D.Geometry = new MapPoint(longitude, latitude, SpatialReferences.Wgs84);
            }
            else
            {
                var heading = (double)graphic3D.Attributes["Heading"];
                if (double.IsNaN(heading))
                    graphic3D.Attributes["Heading"] = heading = newHeading;
                var dx = longitude - start.X;
                var dy = latitude - start.Y;
                var dh = newHeading - heading;
                if (dh < -180) dh += 360;
                else if (dh > 180) dh -= 360;
                // Interpolate position update over 16 frames
                for (int i = 0; i < 60; i++)
                {
                    if (updateId != cid) break;
                    graphic3D.Geometry = new MapPoint(start.X + dx * i / 60d, start.Y + dy * i / 60d, start.SpatialReference);
                    var h = heading + dh * i / 60d;
                    if (h > 360) h -= 360;
                    else if (h < 0) h += 360;
                    graphic3D.Attributes["Heading"] = h;
                    await Task.Delay(16).ConfigureAwait(false);
                }
            }
        }

        public NmeaDevice NmeaDevice
        {
            get { return (NmeaDevice)GetValue(NmeaDeviceProperty); }
            set { SetValue(NmeaDeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NmeaDeviceProperty =
            DependencyProperty.Register(nameof(NmeaDevice), typeof(NmeaDevice), typeof(View3D), new PropertyMetadata(null));

    }
}

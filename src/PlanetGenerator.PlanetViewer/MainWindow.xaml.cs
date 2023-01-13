using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PlanetGenerator.Wpf;

namespace PlanetGenerator.PlanetViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModelVisual3D _planetVisual;
        private Matrix3D _cameraTransform;
        private double _x = 0, _y = 1, _z = 0, _h, _angle = 0;
        private PlanetSettings _settings;

        public MainWindow()
        {
            InitializeComponent();
            _h = 11000;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _settings = new PlanetSettings();
            _settings.TileResolution = 512;
            _settings.Seed = 3;
            _planetVisual = new ModelVisual3D();
            viewLight.Transform = new RotateTransform3D
            {
                Rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), _settings.AxialTilt * 180)
            };
            PlanetBuilder generator = new PlanetBuilder(_settings, new SimplexNoise(3));
            {
                var tile = generator.GenerateTile(0, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(1, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(2, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(3, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(4, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(5, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(6, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(7, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(8, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            {
                var tile = generator.GenerateTile(9, 0);
                var model = tile.GenerateSphereModel();
                _planetVisual.Children.Add(new ModelVisual3D { Content = model });
            }
            _cameraTransform = Matrix3D.Identity;
            viewRoot.Children.Add(_planetVisual);
            view.CameraController.MoveSensitivity = 100;
            view.CameraController.CameraPosition = new Point3D(11000, 0, 0);
            view.CameraController.CameraLookDirection = new Vector3D(-1, 0, 0);
            view.CameraController.CameraUpDirection = new Vector3D(0, 1, 0);
            //view.CameraController.rotat
            UpdateInformation();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetCamera_Click(object sender, RoutedEventArgs e)
        {
            view.CameraController.CameraPosition = new Point3D(11000, 0, 0);
            view.CameraController.CameraLookDirection = new Vector3D(-1, 0, 0);
            view.CameraController.CameraUpDirection = new Vector3D(0, 1, 0);
        }

        private void UpdateInformation()
        {
            if (_x > 180)
                longitudeText.Text = "东经";
            else
                longitudeText.Text = "西经";
            if (_y >= 0)
                latitudeText.Text = "北纬";
            else
                latitudeText.Text = "南纬";
            longitudeValue.Text = _x.ToString("F6");
            latitudeValue.Text = _y.ToString("F6");
            highValue.Text = _h.ToString("F6");
            if (_angle > 1)
                angleValue.Text = _angle.ToString("F4") + "千米";
            else
                angleValue.Text = (_angle * 1000).ToString("F0") + "米";
        }

        private void view_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _h -= e.Delta;
            UpdateInformation();
            //var storyboard = (Storyboard)view.Resources["cameraMoveStoryboard"];
            //storyboard.Stop();
            var to = (Vector3D)view.CameraController.CameraPosition;
            to.Normalize();
            to = to * _h;
            //((Point3DAnimation)storyboard.Children[0]).To = (Point3D)to;
            //storyboard.Begin();
            view.CameraController.CameraPosition = (Point3D)to;
            //view.LookAt(new Point3D(), _h, 1500);
        }

        private bool _dragPlanet = false;
        private Matrix3D _dragMatrix;
        //private Point _dragPoint;
        private Vector3D _dragPoint;
        private void view_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = GetPointer();
            if (p.HasValue)
            {
                view.CaptureMouse();
                _dragPlanet = true;
                _dragMatrix = _cameraTransform;
                _dragPoint = p.Value;
                Debug.WriteLine("start:" + _dragPoint);
                //_dragPoint = e.GetPosition(view);
            }
        }

        private void view_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragPlanet)
            {
                _dragPlanet = false;
                view.ReleaseMouseCapture();
                Debug.WriteLine("end:" + _dragPoint);
            }
        }

        private void view_MouseMove(object sender, MouseEventArgs e)
        {
            var p = GetPointer();
            if (_dragPlanet && p.HasValue)
            {
                var v1 = _dragPoint;
                var v2 = p.Value;
                var axis = Vector3D.CrossProduct(v1, v2);
                axis.Normalize();
                var angle = Vector3D.AngleBetween(v1, v2);
                Debug.WriteLine("P1:" + v1);
                Debug.WriteLine("P2:" + v2);
                Debug.WriteLine("Axis:" + axis + ",Angle:" + angle);
                var m = new Matrix3D();
                m.Prepend(_cameraTransform);
                m.Rotate(new Quaternion(axis, -angle));
                var position = m.Transform(new Point3D(_h, 0, 0));
                view.CameraController.CameraPosition = position;
                view.CameraController.CameraLookDirection = new Vector3D(-position.X, -position.Y, -position.Z);
                view.CameraController.CameraUpDirection = m.Transform(new Vector3D(0, 1, 0));
                _cameraTransform = m;
                _dragPoint = p.Value;
            }
            else
            {
                Debug.WriteLine("PointOnPlanet:" + p);
            }
        }

        private Vector3D? GetPointer()
        {
            var d = view.CursorRay.Direction;
            var o = view.CursorRay.Origin;
            double a = (d.X * d.X) + (d.Y * d.Y) + (d.Z * d.Z);
            double b = (2 * d.X * (o.X) + 2 * d.Y * (o.Y) + 2 * d.Z * (o.Z));
            double c = ((o.X) * (o.X) + (o.Y) * (o.Y) + (o.Z) * (o.Z)) - _settings.PlanetRadius * _settings.PlanetRadius;
            var result = SolvingQuadratics(a, b, c);
            if (result.Count == 0)
                return null;
            var p = o + d * result.Max();
            return new Vector3D(p.X, p.Y, p.Z);
        }

        List<double> SolvingQuadratics(double a, double b, double c)
        {
            List<double> t = new List<double>();
            double delta = b * b - 4 * a * c;
            if (delta < 0)
            {
                return t;
            }
            if (Math.Abs(delta) < 0.0000000001)
            {
                t.Add(-b / (2 * a));
            }
            else
            {
                t.Add((-b + Math.Sqrt(delta)) / (2 * a));
                t.Add((-b - Math.Sqrt(delta)) / (2 * a));
            }
            return t;
        }

        private static Vector3D ProjectToball(Point point, double w, double h)
        {
            var l = Math.Max(w, h) / 2;
            //double r = Math.Sqrt(l * l + l * l) / 2;
            double x = (point.X - w / 2) / l;
            double y = (h / 2 - point.Y) / l;
            double z2 = 1 - x * x - y * y;
            double z = z2 > 0 ? Math.Sqrt(z2) : 0;

            return new Vector3D(x, y, z);
        }
    }
}

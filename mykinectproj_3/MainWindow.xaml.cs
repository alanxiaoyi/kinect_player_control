using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect;
using System.Diagnostics;
using Kinect.Toolbox;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;


namespace mykinectproj_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public delegate bool CallBack(int hwnd, int lParam);
    public partial class MainWindow : Window
    {
        private readonly KinectSensorChooser _sensorChooser = new KinectSensorChooser();
        private KinectSensor _sensor;
        SwipeGestureDetector swipeGestureRecognizer;
        VerticalGestureDetector verticalGestureRecognizer;
 //       TemplatedGestureDetector circleGestureRecognizer;
        private Skeleton[] skeletons;
        readonly ContextTracker contextTracker = new ContextTracker();
        public MainWindow()
        {
            InitializeComponent();  
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sensorChooserUI.KinectSensorChooser = _sensorChooser;
            _sensorChooser.Start();
            _sensor = KinectSensor.KinectSensors[0];

            if (_sensor.Status == KinectStatus.Connected)
            {
                _sensor.ColorStream.Enable();
                _sensor.DepthStream.Enable();
                _sensor.SkeletonStream.Enable();
                _sensor.AllFramesReady += _sensor_AllFramesReady;
                swipeGestureRecognizer = new SwipeGestureDetector();
                verticalGestureRecognizer = new VerticalGestureDetector();
                swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;
                verticalGestureRecognizer.OnGestureDetected += OnGestureDetected;
                _sensor.Start();

  
            }
        }
        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        CallBack winCtrCallback = new CallBack(ctrWindow);
        void OnGestureDetected(string gesture)
        {
            int flag = 0;
            if (gesture.IndexOf("Right") != -1)
            {
                flag = 2;

            }
            else if (gesture.IndexOf("Left") != -1)
            {
                flag = 1;

            }
            else if (gesture.IndexOf("Vertical") != -1)
            {
                flag = 0;

            }
            int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));
            detectedGestures.SelectedIndex = pos;
            EnumWindows(winCtrCallback, flag);

        }

        public static bool ctrWindow(int hwnd, int lParam)
        {

            StringBuilder lpCaptionName = new StringBuilder("",255);
            GetWindowText((IntPtr)hwnd, lpCaptionName, lpCaptionName.Capacity);
            if ((lpCaptionName.ToString().IndexOf("快播") != -1))             //change to which player you want to control: like VLC, wmp or others. You can you spy++ to find the name of the winodw
            {
                Debug.WriteLine("find kuaibo");
                SetForegroundWindow((IntPtr)hwnd);
                switch (lParam)
                {
                    case 2:
                        SendKeys.SendWait("{RIGHT}");
                        break;
                    case 1:
                        SendKeys.SendWait("{LEFT}");
                        break;
                    case 0:
                        SendKeys.SendWait(" ");
                        break;
                    default:
                        break;
                }

            }

            return true;
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {        
    
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;
                frame.GetSkeletons(ref skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;
                Dictionary<int, string> stabilities = new Dictionary<int, string>();
                foreach (var skeleton in skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;                               //check if the skeleton is tracked

                    contextTracker.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                    stabilities.Add(skeleton.TrackingId, contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId) ? "Stable" : "Non stable");
                    if (!contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId))
                        continue;                           //check if the skeleton is stable (not in motion)

                    foreach (Joint joint in skeleton.Joints)
                    {
                        if (joint.TrackingState != JointTrackingState.Tracked)
                            continue;

                        if (joint.JointType == JointType.HandLeft )
                        {
                            if(joint.Position.Y > skeleton.Joints[JointType.Head].Position.Y-0.2 && joint.Position.X<skeleton.Joints[JointType.Head].Position.X-0.1)
                                 swipeGestureRecognizer.Add(joint.Position, _sensor);
                            if (joint.Position.X > skeleton.Joints[JointType.Head].Position.X - 0.1 && joint.Position.X < skeleton.Joints[JointType.Head].Position.X + 0.1)
                             verticalGestureRecognizer.Add(joint.Position, _sensor);
                        }

                        if (joint.JointType == JointType.HandRight)
                        {
                            if (controlMouse.IsChecked == true)
                                MouseController.Current.SetHandPosition(_sensor, joint, skeleton);
                            else {
                                if (joint.Position.Y > skeleton.Joints[JointType.Head].Position.Y - 0.2 && joint.Position.X > skeleton.Joints[JointType.Head].Position.X+0.1)
                                    swipeGestureRecognizer.Add(joint.Position, _sensor);
                                if (joint.Position.X > skeleton.Joints[JointType.Head].Position.X - 0.1 && joint.Position.X < skeleton.Joints[JointType.Head].Position.X + 0.1)
                                    verticalGestureRecognizer.Add(joint.Position, _sensor);
                             }
                        }
                    }

                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
        }
    }
}

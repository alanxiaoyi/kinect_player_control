using System;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public class VerticalGestureDetector : GestureDetector
    {
        public float VerticalMinimalLength { get; set; }
        public float VerticalMaximalWidth { get; set; }
        public int VerticalMininalDuration { get; set; }
        public int VerticalMaximalDuration { get; set; }

        public VerticalGestureDetector(int windowSize = 20)
            : base(windowSize)
        {
            VerticalMinimalLength = 0.4f;
            VerticalMaximalWidth= 0.2f;
            VerticalMininalDuration = 250;
            VerticalMaximalDuration = 1500;
        }

        protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction, 
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void LookForGesture()
        {
      
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < VerticalMaximalWidth, // Height
                (p1, p2) => p2.Y - p1.Y > -0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.Y - p1.Y) > VerticalMinimalLength, // Length
                VerticalMininalDuration, VerticalMaximalDuration)) // Duration
            {
                RaiseGestureDetected("VerticalUp");
                return;
            }

            // Swipe to left
  /*          if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < VerticalMaximalHeight,  // Height
                (p1, p2) => p2.X - p1.X < 0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > VerticalMinimalLength, // Length
                VerticalMininalDuration, VerticalMaximalDuration))// Duration
            {
                RaiseGestureDetected("VerticalToLeft");
                return;
            }*/
        }
    }
}
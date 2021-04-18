using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class Wagon : MonoBehaviour
    {
        public  bool          isEngine;
        public  Wagon         back;
        public  float         offset;
        private Wagon         front;
        private SplineSegment segment, tempSegment;

        private SplineTracer tracer;

        private void Awake()
        {
            tracer = GetComponent<SplineTracer>();
            //Wagon compoenent that is attached to the train engine and is marked as "isEngine" will
            //run a recursive setup for the rest of the wagons
            if (isEngine) SetupRecursively(null, new SplineSegment(tracer.spline, -1, tracer.direction));
        }

        private void SetupRecursively(Wagon frontWagon, SplineSegment inputSegment)
        {
            front   = frontWagon;
            segment = inputSegment;
            if (back != null) back.SetupRecursively(this, segment);
        }

        public void UpdateOffset()
        {
            ApplyOffset();
            if (back != null) back.UpdateOffset();
        }

        private Wagon GetRootWagon()
        {
            var current                           = this;
            while (current.front != null) current = current.front;
            return current;
        }

        private void ApplyOffset()
        {
            if (isEngine)
            {
                ResetSegments();
                return;
            }

            float totalMoved = 0f, moved = 0f;
            var   start      = front.tracer.UnclipPercent(front.tracer.result.percent);
            //Travel backwards along the front wagon's spline
            var inverseDirection = front.segment.direction;
            InvertDirection(ref inverseDirection);
            var spline = front.segment.spline;
            var percent =
                front.segment.Travel(start, offset, inverseDirection, out moved, front.segment.spline.isClosed);
            totalMoved += moved;
            //Finalize if moved fully without reaching a spline end or a junction
            if (Mathf.Approximately(totalMoved, offset))
            {
                if (segment != front.segment)
                    if (back != null)
                        back.segment = segment;
                if (segment != front.segment) segment = front.segment;
                ApplyTracer(spline, percent, front.tracer.direction);
                return;
            }

            //Otherwise, move along the current recorded spline segment
            if (segment != front.segment)
            {
                inverseDirection = segment.direction;
                InvertDirection(ref inverseDirection);
                spline     =  segment.spline;
                percent    =  segment.Travel(offset - totalMoved, inverseDirection, out moved, segment.spline.isClosed);
                totalMoved += moved;
            }

            ApplyTracer(spline, percent, segment.direction);
        }

        private void ResetSegments()
        {
            var current = back;
            var same    = true;
            while (current != null)
            {
                if (current.segment != segment)
                {
                    same = false;
                    break;
                }

                current = current.back;
            }

            //if all wagons are on the same segment, remove the segment entrance so that they can loop
            if (same) segment.start = -1;
        }

        private void ApplyTracer(SplineComputer spline, double percent, Spline.Direction direction)
        {
            var rebuild = tracer.spline != spline;
            tracer.spline = spline;
            if (rebuild) tracer.RebuildImmediate();
            tracer.direction = direction;
            tracer.SetPercent(tracer.ClipPercent(percent));
        }

        public void EnterSplineSegment(int              previousSplineExitPoint, SplineComputer spline, int entryPoint,
                                       Spline.Direction direction)
        {
            if (!isEngine) return;
            if (back != null)
            {
                segment.end  = previousSplineExitPoint;
                back.segment = segment;
            }

            segment = new SplineSegment(spline, entryPoint, direction);
        }

        private static void InvertDirection(ref Spline.Direction direction)
        {
            if (direction == Spline.Direction.Forward) direction = Spline.Direction.Backward;
            else direction                                       = Spline.Direction.Forward;
        }

        //A helper class which contains an information for a spline and the points 
        //between which the spline is traversed (start and end)
        //If one of the points is equal to -1 it means that there is no constraint
        public class SplineSegment
        {
            public Spline.Direction direction;
            public SplineComputer   spline;
            public int              start = -1, end = -1;

            public SplineSegment(SplineComputer spline, int entryPoint, Spline.Direction direction)
            {
                this.spline    = spline;
                start          = entryPoint;
                this.direction = direction;
            }

            public SplineSegment(SplineSegment input)
            {
                spline    = input.spline;
                start     = input.start;
                end       = input.end;
                direction = input.direction;
            }

            public double Travel(double percent, float distance, Spline.Direction direction, out float moved, bool loop)
            {
                var max             = direction == Spline.Direction.Forward ? 1.0 : 0.0;
                if (start >= 0) max = spline.GetPointPercent(start);
                return TravelClamped(percent, distance, direction, max, out moved, loop);
            }

            //Travel the spline segment by automatically starting at the segment's exit (end)
            public double Travel(float distance, Spline.Direction direction, out float moved, bool loop)
            {
                var startPercent    = spline.GetPointPercent(end);
                var max             = direction == Spline.Direction.Forward ? 1.0 : 0.0;
                if (start >= 0) max = spline.GetPointPercent(start);
                return TravelClamped(startPercent, distance, direction, max, out moved, loop);
            }

            //Travel the spline segment while not exceeding the "max" percent
            //It also supports looping splines unlike the standard Travel methods found in SplineComputer and SplineUser
            private double TravelClamped(double    percent, float distance, Spline.Direction direction, double max,
                                         out float moved,   bool  loop)
            {
                moved = 0f;
                var traveled = 0f;
                var result   = spline.Travel(percent, distance, out traveled, direction);
                moved += traveled;
                if (loop && moved < distance)
                {
                    if (direction == Spline.Direction.Forward && Mathf.Approximately((float) result, 1f))
                        result = spline.Travel(0.0, distance - moved, out traveled, direction);
                    else if (direction == Spline.Direction.Backward && Mathf.Approximately((float) result, 0f))
                        result = spline.Travel(1.0, distance - moved, out traveled, direction);
                    moved += traveled;
                }

                if (direction == Spline.Direction.Forward && percent <= max)
                {
                    if (result > max)
                    {
                        moved  -= spline.CalculateLength(result, max);
                        result =  max;
                    }
                }
                else if (direction == Spline.Direction.Backward && percent >= max)
                {
                    if (result < max)
                    {
                        moved  -= spline.CalculateLength(max, result);
                        result =  max;
                    }
                }

                return result;
            }
        }
    }
}
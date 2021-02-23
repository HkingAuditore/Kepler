namespace Dreamteck.Splines.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Dreamteck.Splines;
    using System;

    public class TrainEngine : MonoBehaviour
    {
        private SplineTracer _tracer = null;
        private double _lastPercent = 0.0;
        private Wagon _wagon;

        private void Awake()
        {
            _wagon = GetComponent<Wagon>();
        }

        void Start()
        {
            _tracer = GetComponent<SplineTracer>();
            //Subscribe to the onNode event to receive junction information automatically when a Node is passed
            _tracer.onNode += OnJunction;
            //Subscribe to the onMotionApplied event so that we can immediately update the wagons' positions once the engine's position is set
            _tracer.onMotionApplied += OnMotionApplied;

            //If the tracer is a SplineFollower (which should be the the case), subscribe to onEndReached and onOnBeginningReached
            if (_tracer is SplineFollower)
            {
                SplineFollower follower = (SplineFollower)_tracer;
                Debug.Log("Subscribing to follower");
                follower.onBeginningReached += FollowerOnBeginningReached;
                follower.onEndReached += FollowerOnEndReached;
            }
        }

        private void OnMotionApplied()
        {
            //Apply the wagon's offset (this will recursively apply the offsets to the rest of the wagons in the chain)
            _lastPercent = _tracer.result.percent;
            _wagon.UpdateOffset();
        }

        /// <summary>
        /// Gets the last follower percent before reaching the beginning and looping / ping-ponging
        /// </summary>
        private void FollowerOnBeginningReached(double lastPercent)
        {
            _lastPercent = lastPercent;
        }

        /// <summary>
        /// Gets the last follower percent before reaching the end and looping / ping-ponging
        /// </summary>
        private void FollowerOnEndReached(double lastPercent)
        {
            _lastPercent = lastPercent;
        }

        //Called when the tracer has passed a junction (a Node)
        private void OnJunction(List<SplineTracer.NodeConnection> passed)
        {
            Node node = passed[0].node; //Get the node of the junction
            JunctionSwitch junctionSwitch = node.GetComponent<JunctionSwitch>(); //Look for a JunctionSwitch component
            if (junctionSwitch == null) return; //No JunctionSwitch - ignore it - this isn't a real junction
            if (junctionSwitch.bridges.Length == 0) return; //The JunctionSwitch does not have bridge elements
            foreach (JunctionSwitch.Bridge bridge in junctionSwitch.bridges)
            {
                //Look for a suitable bridge element based on the spline we are currently traversing
                if (!bridge.active) continue;
                if (bridge.a == bridge.b) continue; //Skip bridge if it points to the same spline  
                int currentConnection = 0;
                Node.Connection[] connections = node.GetConnections();
                //get the connected splines and find the index of the tracer's current spline
                for (int i = 0; i < connections.Length; i++)
                {
                    if (connections[i].spline == _tracer.spline)
                    {
                        currentConnection = i;
                        break;
                    }
                }
                //Skip the bridge if we are not on one of the splines that the switch connects
                if (currentConnection != bridge.a && currentConnection != bridge.b) continue;
                if (currentConnection == bridge.a)
                {
                    if ((int)_tracer.direction != (int)bridge.bDirection) continue;
                    //This bridge is suitable and should use it
                    SwitchSpline(connections[bridge.a], connections[bridge.b]);
                    return;
                }
                else
                {
                    if ((int)_tracer.direction != (int)bridge.aDirection) continue;
                    //This bridge is suitable and should use it
                    SwitchSpline(connections[bridge.b], connections[bridge.a]);
                    return;
                }
            }
        }

        void SwitchSpline(Node.Connection from, Node.Connection to)
        {
            //See how much units we have travelled past that Node in the last frame
           
            float excessDistance = from.spline.CalculateLength(from.spline.GetPointPercent(from.pointIndex), _tracer.UnclipPercent(_lastPercent));
            //Set the spline to the tracer
            _tracer.spline = to.spline;
            _tracer.RebuildImmediate();
            //Get the location of the junction point in percent along the new spline
            double startpercent = _tracer.ClipPercent(to.spline.GetPointPercent(to.pointIndex));
            if (Vector3.Dot(from.spline.Evaluate(from.pointIndex).forward, to.spline.Evaluate(to.pointIndex).forward) < 0f)
            {
                if (_tracer.direction == Spline.Direction.Forward) _tracer.direction = Spline.Direction.Backward;
                else _tracer.direction = Spline.Direction.Forward;
            }
            //Position the tracer at the new location and travel excessDistance along the new spline
            _tracer.SetPercent(_tracer.Travel(startpercent, excessDistance, _tracer.direction));
            //Notify the wagon that we have entered a new spline segment
            _wagon.EnterSplineSegment(from.pointIndex, _tracer.spline, to.pointIndex, _tracer.direction);
            _wagon.UpdateOffset();
        }
    }
}

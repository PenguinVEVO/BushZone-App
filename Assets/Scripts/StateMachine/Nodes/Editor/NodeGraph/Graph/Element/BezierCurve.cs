using System.Collections.Generic;
using UnityEngine;

/*
    Script: BezierCurve
    Author: Gareth Lockett
    Version: 1.0
    Description: Class (non-monobehaviour) for creating and managing a bezier curve.
    Ref: https://www.habrador.com/tutorials/interpolation/2-bezier-curve/
    Usage:
        - Create an instance via new BezierCurve() to create the curve
        - Can modify the curve after it has been created using SetStartPosition()...SetCurve() Curve is automatically updated.
        - Can draw the curve using DrawCurve()
        - Can check for raycast hits using RaycastCurve() Note: Can automatically update the curve if gets a hit.
        - Can get a point along the curve using GetPositionAlongCurve() or GetPointAlongCurveByDistance()
*/

public class BezierCurve
{
    // Properties
    public Vector3 StartPosition { get; private set; }          // Start point position in world space.
    public Vector3 StartHandlePosition { get; private set; }    // Start handle in world space.
    public Vector3 EndHandlePosition { get; private set; }      // End handle position in world space.
    public Vector3 EndPosition { get; private set; }            // End point position in world space.
    public float CurveStepSize { get; private set; }            // Segment sizes along curve (Last segment may be shorter)
    public float CurveLength { get; private set; }              // The approximated length of the curve (Eg accumilated distance between CurvePoints)
    public List<Vector3> CurvePoints { get; private set; }      // The calculated points along the curve in world space.
    public List<Vector3> CurveNormals { get; private set; }     // The calculated normals along the curve in world space. Should have the same number as CurvePoints.

    // Constructor
    public BezierCurve( Vector3 startPosition, Vector3 startHandlePosition, Vector3 endHandlePosition, Vector3 endPosition, float curveStepSize )
    {
        // Sanity checks.
        if( curveStepSize <= 0f ) { curveStepSize = 0.01f; }

        // Init properties.
        this.CurvePoints = new List<Vector3>(); this.CurveNormals = new List<Vector3>();
        this.SetCurve( startPosition, startHandlePosition, endHandlePosition, endPosition, curveStepSize );

        // Init curve points and length.
        this.UpdateCurve();
    }

    // Use these to update the curve after it has been created.
    public void SetStartPosition( Vector3 startPosition ) { if( this.StartPosition == startPosition ) { return; } this.StartPosition = startPosition; this.UpdateCurve(); }
    public void SetStartHandlePosition( Vector3 startHandlePosition ) { if( this.StartHandlePosition == startHandlePosition ) { return; } this.StartHandlePosition = startHandlePosition; this.UpdateCurve(); }
    public void SetEndPosition( Vector3 endPosition ) { if( this.EndPosition == endPosition ) { return; } this.EndPosition = endPosition; this.UpdateCurve(); }
    public void SetEndHandlePosition( Vector3 endHandlePosition ) { if( this.EndHandlePosition == endHandlePosition ) { return; } this.EndHandlePosition = endHandlePosition; this.UpdateCurve(); }
    public void SetCurveStepSize( float curveStepSize ) { if( curveStepSize <= 0f ) { curveStepSize = 0.01f; } if( this.CurveStepSize == curveStepSize ) { return; } this.CurveStepSize = curveStepSize; this.UpdateCurve(); }
    public void SetCurve( Vector3 startPosition, Vector3 startHandlePosition, Vector3 endHandlePosition, Vector3 endPosition, float curveStepSize )
    {
        this.StartPosition = startPosition; this.StartHandlePosition = startHandlePosition;
        this.EndPosition = endPosition; this.EndHandlePosition = endHandlePosition;
        this.CurveStepSize = curveStepSize;
        this.UpdateCurve();
    }

    private void UpdateCurve()
    {
        Vector3 lastPos = this.StartPosition; // Init the starting position of the curve.
        float curveResolution = this.CurveStepSize; // Size of each step along the curve.
        int numberOfSteps = Mathf.FloorToInt( 1f / curveResolution ) + 1; // Number of steps along the curve.
        this.CurveLength = 0f; // Init the curve length.
        this.CurvePoints.Clear(); this.CurvePoints.Add( lastPos ); // Init curve points list.

        Vector3 startHandleVector = ( this.StartHandlePosition - this.StartPosition ).normalized;
        Vector3 endHandleVector = ( this.EndHandlePosition - this.EndPosition ).normalized;
        this.CurveNormals.Clear(); this.CurveNormals.Add( startHandleVector );


        for( int i = 0; i < numberOfSteps; i++ )
        {
            float dist = i * curveResolution; // Distance along curve.
            
            // Calculate the position along curve.
            float oneMinusT = 1f - dist;
            Vector3 Q = oneMinusT * this.StartPosition + dist * this.StartHandlePosition;
            Vector3 R = oneMinusT * this.StartHandlePosition + dist * this.EndHandlePosition;
            Vector3 S = oneMinusT * this.EndHandlePosition + dist * this.EndPosition;
            Vector3 P = oneMinusT * Q + dist * R;
            Vector3 T = oneMinusT * R + dist * S;
            Vector3 newPos = oneMinusT * P + dist * T;

            this.CurvePoints.Add( newPos );
            this.CurveNormals.Add( Vector3.Lerp( startHandleVector, endHandleVector, ( float )i / numberOfSteps ) );
            this.CurveLength = ( newPos - lastPos ).magnitude;

            lastPos = newPos;
        }
    }

    public Vector3 GetPositionAlongCurve( float position ) // position = 0->1
    {
        // Sanity checks.
        if( this.CurvePoints.Count <= 1 ) { this.UpdateCurve(); } 
        if( this.CurvePoints.Count <= 1 || this.CurveLength == 0f ) { return this.StartPosition; }
        if( position <= 0f ) { return this.StartPosition; }
        if( position >= 1f ) { return this.EndPosition; }

        // Calculate the position along curve.
        float oneMinusT = 1f - position;
        Vector3 Q = oneMinusT * this.StartPosition + position * this.StartHandlePosition;
        Vector3 R = oneMinusT * this.StartHandlePosition + position * this.EndHandlePosition;
        Vector3 S = oneMinusT * this.EndHandlePosition + position * this.EndPosition;
        Vector3 P = oneMinusT * Q + position * R;
        Vector3 T = oneMinusT * R + position * S;
        Vector3 newPos = oneMinusT * P + position * T;
        return newPos;
    }

    public Vector3 GetPointAlongCurveByDistance( float distance ) // distance in meters. Will return the end point if distance is greater than curve length.
        { return this.GetPositionAlongCurve( distance / this.CurveLength ); }

    public void DrawCurve( Color color, bool drawHandles = false, bool drawNormals = false, float duration = 0f )
    {
        // Sanity checks.
        if( this.CurvePoints.Count <= 1 ) { this.UpdateCurve(); }
        if( this.CurvePoints.Count <= 1 || this.CurveLength == 0f ) { return; }
        if( duration < 0f ) { duration = 0f; }

        // Loop through the curve points drawing each segment.
        for( int i = 1; i < this.CurvePoints.Count; i++ ) { Debug.DrawLine( this.CurvePoints[ i - 1 ], this.CurvePoints[ i ], color, duration ); }

        // Option to draw handles.
        if( drawHandles == true )
        {
            Debug.DrawLine( this.StartPosition, this.StartHandlePosition, Color.blue, duration );
            Debug.DrawLine( this.EndPosition, this.EndHandlePosition, Color.blue, duration );
        }

        // Option to draw normals.
        if( drawNormals == true && this.CurvePoints.Count == this.CurveNormals.Count )
            { for( int i = 0; i < this.CurvePoints.Count; i++ ) { Debug.DrawRay( this.CurvePoints[ i ], this.CurveNormals[ i ] * 0.25f, Color.yellow, duration ); } }
    }

    public float GetClosetPoint(Vector3 position )
    {
        float curveResolution = this.CurveStepSize; // Size of each step along the curve.
        int numberOfSteps = Mathf.FloorToInt( 1f / curveResolution ) + 1; // Number of steps along the curve.
        float stepSize =  1f/ numberOfSteps;

        //Debug.Log( $"[Bezier] {this.GetPositionAlongCurve( 0 )} ::{position}" );

        float closestCurvePosition = 0f;
        float closestDistance = float.MaxValue; // start max possible distance

        for (int i=0; i < numberOfSteps; i++ )
        {
            float t = i * stepSize;
            Vector3 curvePosition =  this.GetPositionAlongCurve(t);

            float distanceSqr = ( curvePosition - position).sqrMagnitude;

            if( distanceSqr < closestDistance ) 
            {
                closestDistance = distanceSqr;
                closestCurvePosition = t;
            }
        }

        return closestCurvePosition;
    }

    public bool RaycastCurve( out RaycastHit hit, bool autoUpdateCurve = false ) // If autoUpdateCurve = true then this curve will be modified if it gets a hit.
    {
        // Sanity checks.
        if( this.CurvePoints.Count <= 1 ) { this.UpdateCurve(); }
        if( this.CurvePoints.Count <= 1 || this.CurveLength == 0f ) { hit = new RaycastHit(); return false; }

        // Raycast along each curve segment.
        for( int i = 1; i < this.CurvePoints.Count; i++ )
        {
            if( Physics.Linecast( this.CurvePoints[ i - 1 ], this.CurvePoints[ i ], out RaycastHit hitInfo ) == true )
            {
                // Check to auto update the curve.
                if( autoUpdateCurve == true )
                {
                    this.SetCurve( this.StartPosition, this.StartHandlePosition
                        , hitInfo.point + ( hitInfo.normal * ( this.EndHandlePosition - this.EndPosition ).magnitude * ( ( float ) i / this.CurvePoints.Count ) ) // Keep the hit handle length the same as the set end handle length?
                        , hitInfo.point + ( hitInfo.normal * 0.01f ) // Offset the hit point from the surface by a small amount.
                        , this.CurveStepSize );
                }
                hit = hitInfo; // Copy hitInfo to hit (Eg return the hit info)
                return true;
            }
        }

        // Create some fake hit info.
        hit = new RaycastHit();
        hit.point = this.EndPosition;
        hit.normal = this.EndHandlePosition - this.EndPosition;

        return false;
    }
}

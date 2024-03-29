using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetInputs {
    
    // Attributes -------------------------------------------------------------
    // Ownship
    public int decoys { get; set; }
    public int missiles { get; set; }

    public float pitch { get; set; }
    public float roll { get; set; }
    public float yaw { get; set; }
    public float throttle { get; set; }

    public Vector3 position { get; set; }
    public Vector3 velocity { get; set; }
    public Vector3 acceleration { get; set; }
    public Vector3 rotation { get; set; }

    // Target
    public int decoysTarget { get; set; }
    public int missilesTarget { get; set; }

    public float aspectAngle { get; set; }
    public float antennaTrainAngle { get; set; }
    public float headingCrossingAngle { get; set; }
    public float relativeAltitude { get; set; }
    public float relativeDistance { get; set; }

    public Vector3 relativeVelocity { get; set; }
    public Vector3 relativeAcceleration { get; set; }

    // Decoys
    public Vector3 inRangeDecoyRelativePosition { get; set; }

    // Missile
    public float launchedMissileDistanceToTarget { get; set; }
    public float incomigMissileDistanceToOwnship { get; set; }
    public Vector3 incomingMissileRelativeDirection { get; set; }


}

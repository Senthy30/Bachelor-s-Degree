using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetInputs {
    
    // Attributes -------------------------------------------------------------
    // Ownship
    public bool areFlapsActive { get; set; }
    public bool areLandingGearActive { get; set; }

    public float decoys { get; set; }
    public float missiles { get; set; }

    public float pitch { get; set; }
    public float roll { get; set; }
    public float yaw { get; set; }
    public float flap { get; set; }
    public float throttle { get; set; }

    public Vector3 position { get; set; }
    public Vector3 velocity { get; set; }
    public Vector3 angularVelocity { get; set; }
    public Vector3 acceleration { get; set; }
    public Vector3 angularAcceleration { get; set; }
    public Quaternion rotation { get; set; }

    // Target

    public float decoysTarget { get; set; }
    public float missilesTarget { get; set; }

    public float aspectAngle { get; set; }
    public float antennaTrainAngle { get; set; }
    public float headingCrossingAngle { get; set; }
    public float relativeAltitude { get; set; }
    public float relativeDistance { get; set; }

    public Vector3 relativeVelocity { get; set; }
    public Vector3 relativeAngularVelocity { get; set; }
    public Vector3 relativeAcceleration { get; set; }
    public Vector3 relativeAngularAcceleration { get; set; }
    public Vector3 relativeDirection { get; set; }
    public Quaternion relativeRotation { get; set; }

    // Decoys
    public bool isDecoyInRange { get; set; }
    public float inRangeDecoyDistance { get; set; }
    public float inRangeDecoyAngle { get; set; }
    public Vector3 inRangeDecoyRelativeDirection { get; set; }

    // Missile
    public bool hasLaunchedMissileInRangeTarget { get; set; }
    public float launchedMissileDistanceToTarget { get; set; }

    public bool hasIncomingMissileInRangeOwnship { get; set; }
    public float incomigMissileDistanceToOwnship { get; set; }
    public float incomingMissileAngleToOwnship { get; set; }
    public Vector3 incomingMissileRelativeDirection { get; set; }
    public Vector3 incomingMissileRelativeVelocity { get; set; }
    public Vector3 incomingMissileRelativeAcceleration { get; set; }


}

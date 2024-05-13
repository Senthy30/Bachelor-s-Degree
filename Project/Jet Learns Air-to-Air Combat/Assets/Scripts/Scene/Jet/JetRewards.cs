using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetRewards {
    
    // Attributes -------------------------------------------------------------
    public float total { get; set; }

    // Neutral ----------------------------------------------------------------
    public float inbound { get; set; } // make sure that jet keep distance to boundaries

    // Advantage --------------------------------------------------------------
    public float ownshipAdvantageBehindTarget { get; set; } // ownship is behind target based on angle and distance
    public float ownshipHasTargetInRange { get; set; } // ownship has in range target based on angle and distance
    public float ownshipChasesTarget { get; set; } // ownship is chasing target based on angle and distance
    public float ownshipLaunchesMissile { get; set; } // ownship launches missile based on in range target
    public float ownshipDropsDecoy { get; set; } // ownship drops decoy based on incoming missiles
    public float ownshipShirksIncomingMissile { get; set; } // ownship shirks incoming missile
    //public float ownshipDestroysTargetByMissile { get; set; } // ownship destroys target by missile
    //public float targetDestroysHimself { get; set; } // target destroys himself

    // Disadvantage -----------------------------------------------------------
    public float targetAdvantageBehindOwnship { get; set; } // target is behind ownship based on angle and distance
    public float targetHasOwnshipInRange { get; set; } // target has in range ownship based on angle and distance
    public float targetChasesOwnship { get; set; } // target is chasing ownship based on angle and distance
    public float targetLaunchesMissile { get; set; } // target launches missile based on in range ownship
    public float targetShirksMissile { get; set; } // target shirks launched missile
    //public float targetDropsDecoy { get; set; } // target drops decoy based on fired missiles
    //public float targetDestroysOwnshipByMissile { get; set; } // target destroys ownship by missile
    //public float ownshipDestroysHimself { get; set; } // ownship destroys himself

    // Methods ----------------------------------------------------------------

    public void CalculateTotalReward() {
        total = 0.0f;

        // Neutral
        total += inbound;

        // Advantage
        total += ownshipAdvantageBehindTarget;
        total += ownshipHasTargetInRange;
        total += ownshipChasesTarget;
        total += ownshipLaunchesMissile;
        total += ownshipDropsDecoy;
        total += ownshipShirksIncomingMissile;

        // Target
        total += targetAdvantageBehindOwnship;
        total += targetHasOwnshipInRange;
        total += targetChasesOwnship;
        total += targetLaunchesMissile;
        total += targetShirksMissile;
    }

}

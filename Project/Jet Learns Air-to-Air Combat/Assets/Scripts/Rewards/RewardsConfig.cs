using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RewardsConfig : ScriptableObject {

    [Header("Positive Values")]

    public float jetInboundBox;
    public float advantageBehind;
    public float hasInRange;
    public float chase;
    public float launchMissile;
    public float dropDecoy;
    public float shirksIncomingMissile;
    public float targetDestroyedHimself;
    public float destroyTargetByMissile;

    [Header("Negative Values")]

    public float jetNotInboundBox;
    public float destroyHimself;
    public float targetLaunchMissile;
    public float targetShirksIncomingMissile;

    [Header("Restriction")]

    public float distanceToFloorNCeiling;
    public float distanceToWall;
    public float distanceAdvantageBehind;
    public float distanceHasInRange;
    public float distanceChase;
    public float distanceLaunchMissile;

    public float angleAdvantageBehind;
    public float angleHasInRange;
    public float angleChase;
    public float angleLaunchMissile;

}

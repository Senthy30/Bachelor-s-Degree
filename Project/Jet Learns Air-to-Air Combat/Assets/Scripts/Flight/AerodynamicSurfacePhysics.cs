using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class AerodynamicSurfacePhysics : MonoBehaviour {

    const float maxFlapAngle = 50f;

    [SerializeField]
    private AerodynamicSurfaceConfig surfaceConfig;

    public bool IsAtStall;
    public bool IsControlSurface;
    public float InputMultiplier = 1;
    public ControlInputType InputType;

    private float flapAngle;

    public void SetFlapAngle(float angle) {
        flapAngle = Mathf.Clamp(angle, -Mathf.Deg2Rad * maxFlapAngle, Mathf.Deg2Rad * maxFlapAngle);
    }

    public Tuple<Vector3, Vector3> CalculateForces(Vector3 airVelocity, float airDensity, Vector3 relativePosition) {
        float zeroLiftAoAInRad = surfaceConfig.zeroLiftAoA * Mathf.Deg2Rad;
        float stallAngleNegInRad = surfaceConfig.stallAngleNeg * Mathf.Deg2Rad;
        float stallAnglePosInRad = surfaceConfig.stallAnglePos * Mathf.Deg2Rad;
        float AR = surfaceConfig.aspectRatio;

        Vector3 forceApplied = Vector3.zero;
        Vector3 torqueApplied = Vector3.zero;

        if (!gameObject.activeInHierarchy || surfaceConfig == null)
            return new Tuple<Vector3, Vector3>(forceApplied, torqueApplied);

        // calculate lift-curve slope of a segment of finite aspect ratio surface
        float coefLiftCurve = surfaceConfig.liftCurve * AR / (AR + 2 * (AR + 4) / (AR + 2));

        // calculate flap effectiveness factor
        float theta_f = Mathf.Acos(2 * surfaceConfig.airfoilChordFraction - 1);
        float flapEffectFactor = 1 - (theta_f - Mathf.Sin(theta_f)) / Mathf.PI;
        // empirical factor to account for the effects of viscosity
        float viscosityFactor = Mathf.Lerp(0.8f, 0.4f, (Mathf.Abs(flapAngle) * Mathf.Rad2Deg - 10) / 50);
        float deltaCoefLift = coefLiftCurve * flapEffectFactor * viscosityFactor * flapAngle;

        // calculate zero-lift angle of attack
        float zeroLiftAoA = zeroLiftAoAInRad - deltaCoefLift / coefLiftCurve;

        // calculate maximum positive and negative lift coefficients
        float deltaMaxCoefLiftFactor = Mathf.Clamp01(1 - 0.5f * (surfaceConfig.airfoilChordFraction - 0.1f) / 0.3f);
        float deltaMaxCoefLift = deltaCoefLift * deltaMaxCoefLiftFactor;

        float maxPosCoefLift = coefLiftCurve * (stallAnglePosInRad - zeroLiftAoAInRad) + deltaMaxCoefLift;
        float maxNegCoefLift = coefLiftCurve * (stallAngleNegInRad - zeroLiftAoAInRad) + deltaMaxCoefLift;

        // calculate corresponding positive and negative stall angles
        float stallAnglePos = zeroLiftAoA + maxPosCoefLift / coefLiftCurve;
        float stallAngleNeg = zeroLiftAoA + maxNegCoefLift / coefLiftCurve;

        // calculate relative air velocity
        Vector3 relativeAirVelocity = transform.InverseTransformDirection(airVelocity);
        relativeAirVelocity.z = 0;

        // calculate drag and lift direction
        Vector3 dragDirection = transform.TransformDirection(relativeAirVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.forward);

        // calculate angle of attack
        float area = surfaceConfig.flapChord * surfaceConfig.span;
        float dynamicPressure = 0.5f * airDensity * relativeAirVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(relativeAirVelocity.y, -relativeAirVelocity.x);

        // calculate aerodynamic coefficients for forces
        Vector3 aerodynamicCoef = CalculateAerodynamicCoefficients(angleOfAttack, coefLiftCurve, zeroLiftAoA, stallAngleNeg, stallAnglePos);

        // calculate forces
        Vector3 liftForce = liftDirection * aerodynamicCoef.x * dynamicPressure * area;
        Vector3 dragForce = dragDirection * aerodynamicCoef.y * dynamicPressure * area;
        Vector3 torqueForce = -transform.forward * aerodynamicCoef.z * dynamicPressure * area * surfaceConfig.flapChord;

        // calculate force and torque applied on the unit
        forceApplied += liftForce + dragForce;
        torqueApplied += Vector3.Cross(relativePosition, forceApplied) + torqueForce;

#if UNITY_EDITOR
        IsAtStall = !(angleOfAttack < stallAnglePos && angleOfAttack > stallAngleNeg);
#endif

        return new Tuple<Vector3, Vector3>(forceApplied, torqueApplied);
    }

    private Vector3 CalculateAerodynamicCoefficients(float angleOfAttack, float coefLiftCurve, float zeroLiftAoA, float stallAngleNeg, float stallAnglePos) {
        Vector3 aerodynamicCoef;

        float highAngleMode = Mathf.Deg2Rad * Mathf.Lerp(15, 5, (Mathf.Rad2Deg * flapAngle + 50) / 100);
        float lowAngleMode = Mathf.Deg2Rad * Mathf.Lerp(15, 5, (-Mathf.Rad2Deg * flapAngle + 50) / 100);

        float posStallAngleMode = stallAnglePos + highAngleMode;
        float negStallAngleMode = stallAngleNeg - lowAngleMode;

        if (stallAngleNeg < angleOfAttack && angleOfAttack < stallAnglePos) {
            // calculate for low angle of attack
            aerodynamicCoef = CalculateAerodynamicCoefficientsAtLowAoA(angleOfAttack, coefLiftCurve, zeroLiftAoA);
        } else {
            if (angleOfAttack > posStallAngleMode || angleOfAttack < negStallAngleMode) {
                // calculate at stall mode
                aerodynamicCoef = CalculateAerodynamicCoefficientsAtStall(angleOfAttack, coefLiftCurve, zeroLiftAoA, stallAngleNeg, stallAnglePos);
            } else {
                float lerpValue;
                Vector3 aerodynamicCoefLowAoA;
                Vector3 aerodynamicCoefStall;

                if (angleOfAttack > stallAnglePos) {
                    lerpValue = (angleOfAttack - stallAnglePos) / (posStallAngleMode - stallAnglePos);
                    aerodynamicCoefLowAoA = CalculateAerodynamicCoefficientsAtLowAoA(stallAnglePos, coefLiftCurve, zeroLiftAoA);
                    aerodynamicCoefStall = CalculateAerodynamicCoefficientsAtStall(posStallAngleMode, coefLiftCurve, zeroLiftAoA, stallAngleNeg, stallAnglePos);
                } else {
                    lerpValue = (angleOfAttack - stallAngleNeg) / (negStallAngleMode - stallAngleNeg);
                    aerodynamicCoefLowAoA = CalculateAerodynamicCoefficientsAtLowAoA(stallAngleNeg, coefLiftCurve, zeroLiftAoA);
                    aerodynamicCoefStall = CalculateAerodynamicCoefficientsAtStall(negStallAngleMode, coefLiftCurve, zeroLiftAoA, stallAngleNeg, stallAnglePos);
                }

                aerodynamicCoef = Vector3.Lerp(aerodynamicCoefLowAoA, aerodynamicCoefStall, lerpValue);
            }
        }

        return aerodynamicCoef;
    }

    private Vector3 CalculateAerodynamicCoefficientsAtLowAoA(float angleOfAttack, float coefLiftCurve, float zeroLiftAoA) {
        Vector3 aerodynamicCoef;

        // calculate effective attack of angle based on induced attack of angle and lift coefficient
        float liftCoef = coefLiftCurve * (angleOfAttack - zeroLiftAoA);
        float inducedAoA = liftCoef / (Mathf.PI * surfaceConfig.aspectRatio);
        float effectiveAoA = angleOfAttack - zeroLiftAoA - inducedAoA;

        // precalculate sin and cos for effective angle
        float sinEffectiveAngle = Mathf.Sin(effectiveAoA);
        float cosEffectiveAngle = Mathf.Cos(effectiveAoA);

        float torqueCoef = 0.25f - 0.175f * (1 - 2 * Mathf.Abs(effectiveAoA) / Mathf.PI);
        // tangential force coefficients
        float tanCoef = surfaceConfig.skinFrictionCoef * cosEffectiveAngle;
        // normal force coefficients
        float normCoef = (liftCoef + tanCoef * sinEffectiveAngle) / cosEffectiveAngle;

        // lift force coefficient
        aerodynamicCoef.x = liftCoef;
        // drag force coefficient
        aerodynamicCoef.y = normCoef * sinEffectiveAngle + tanCoef * cosEffectiveAngle;
        // torque force coefficient
        aerodynamicCoef.z = -normCoef * torqueCoef;

        return aerodynamicCoef;
    }

    private Vector3 CalculateAerodynamicCoefficientsAtStall(float angleOfAttack, float coefLiftCurve, float zeroLiftAoA, float stallAngleNeg, float stallAnglePos) {
        Vector3 aerodynamicCoef;

        float liftCoefLowAoA;
        float lerpValue;
        float inducedAoA;
        float effectiveAoA;

        // calculate effective attack of angle based on induced attack of angle and lift coefficient at low angle of attack
        if (angleOfAttack > stallAnglePos) {
            liftCoefLowAoA = coefLiftCurve * (stallAnglePos - zeroLiftAoA);
            lerpValue = (Mathf.PI / 2 - Mathf.Clamp(angleOfAttack, -Mathf.PI / 2, Mathf.PI / 2)) / (Mathf.PI / 2 - stallAnglePos);
        } else {
            liftCoefLowAoA = coefLiftCurve * (stallAngleNeg - zeroLiftAoA);
            lerpValue = (-Mathf.PI / 2 - Mathf.Clamp(angleOfAttack, -Mathf.PI / 2, Mathf.PI / 2)) / (-Mathf.PI / 2 - stallAngleNeg);
        }
        inducedAoA = Mathf.Lerp(0, liftCoefLowAoA / (Mathf.PI * surfaceConfig.aspectRatio), lerpValue);
        effectiveAoA = angleOfAttack - zeroLiftAoA - inducedAoA;

        // precalculate sin and cos for effective angle
        float sinEffectiveAngle = Mathf.Sin(effectiveAoA);
        float cosEffectiveAngle = Mathf.Cos(effectiveAoA);

        // friction at 90 degree
        float frictionCoef = -4.26f * 0.01f * flapAngle * flapAngle + 2.1f * 0.1f * flapAngle + 1.98f;
        float factorCoef = 1f / (0.56f + 0.44f * Mathf.Abs(sinEffectiveAngle)) - 0.41f * (1 - Mathf.Exp(-17 / surfaceConfig.aspectRatio));
        float torqueCoef = 0.25f - 0.175f * (1 - 2 * Mathf.Abs(effectiveAoA) / Mathf.PI);

        // tangential force coefficients
        float tanCoef = 0.5f * surfaceConfig.skinFrictionCoef * cosEffectiveAngle;
        // normal force coefficients
        float normCoef = frictionCoef * sinEffectiveAngle * factorCoef;

        // lift force coefficient
        aerodynamicCoef.x = normCoef * cosEffectiveAngle - tanCoef * sinEffectiveAngle;
        // drag force coefficient
        aerodynamicCoef.y = normCoef * sinEffectiveAngle + tanCoef * cosEffectiveAngle;
        // torque force coefficient
        aerodynamicCoef.z = -normCoef * torqueCoef;

        return aerodynamicCoef;
    }

    public AerodynamicSurfaceConfig GetAerodynamicSurfaceConfig() {
        return surfaceConfig;
    }

    public float GetFlapAngle() { 
        return flapAngle; 
    }
}

public enum ControlInputType { Pitch, Yaw, Roll, Flap };

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AircraftPhysics : MonoBehaviour {

    const float PREDICTION_FRACTION = 0.5f;

    [SerializeField]
    private Vector3 localGForce;

    private Rigidbody rigidbody;

    [SerializeField]
    private float thrust;
    [SerializeField]
    private float thrustPercent;

    [SerializeField]
    private Vector3 forceApplied;
    [SerializeField] 
    private Vector3 torqueApplied;

    [SerializeField]
    private List<AerodynamicSurfacePhysics> aerodynamicSurfaces = null;

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void SetThrustPercent(float percent) {
        thrustPercent = percent;
    }

    public float GetThrustPercent() {
        return thrustPercent;
    }

    public float GetThrust() {
        return thrust;
    }

    public List<AerodynamicSurfacePhysics> GetAerodynamicSurfaces() {
        return aerodynamicSurfaces;
    }

    private void FixedUpdate() {

        return;

        Tuple<Vector3, Vector3> forceTorqueApplied = CalculateAerodynamicForces(
            rigidbody.velocity,
            rigidbody.angularVelocity,
            Vector3.zero,
            rigidbody.worldCenterOfMass,
            1.2f
        );

        forceApplied = forceTorqueApplied.Item1;
        torqueApplied = forceTorqueApplied.Item2;

        rigidbody.AddForce(forceApplied);
        rigidbody.AddTorque(torqueApplied);
        rigidbody.AddForce(transform.forward * thrust * thrustPercent);

        return;

        // calculate velocity prediction
        Vector3 force = forceTorqueApplied.Item1 + transform.forward * thrust * thrustPercent + Physics.gravity * rigidbody.mass;
        Vector3 velocityPrediction = rigidbody.velocity + Time.fixedDeltaTime * PREDICTION_FRACTION * force / rigidbody.mass;

        // calculate angular velocity prediction
        Quaternion inertiaTensorWorldRotation = rigidbody.rotation * rigidbody.inertiaTensorRotation;
        Vector3 torqueChange = Quaternion.Inverse(inertiaTensorWorldRotation) * forceTorqueApplied.Item2;
        Vector3 angularVelocityChange = new Vector3(
            torqueChange.x / rigidbody.inertiaTensor.x,
            torqueChange.y / rigidbody.inertiaTensor.y,
            torqueChange.z / rigidbody.inertiaTensor.z
        );
        Vector3 angularVelocityPrediction = rigidbody.angularVelocity + Time.fixedDeltaTime * PREDICTION_FRACTION * (inertiaTensorWorldRotation * angularVelocityChange);

        // calculate force and torque prediction applied
        Tuple<Vector3, Vector3> forceTorquePredictionApplied = CalculateAerodynamicForces(
            velocityPrediction,
            angularVelocityPrediction,
            Vector3.zero,
            rigidbody.worldCenterOfMass,
            1.2f
        );

        //Debug.Log(velocityPrediction);
        //Debug.Log(angularVelocityPrediction);

        // calculate force and torque that need to be applied
        forceApplied = (forceTorqueApplied.Item1 + forceTorquePredictionApplied.Item1) / 2f;
        torqueApplied = (forceTorqueApplied.Item2 + forceTorquePredictionApplied.Item2) / 2f;

        rigidbody.AddForce(forceApplied);
        rigidbody.AddTorque(torqueApplied);
        rigidbody.AddForce(transform.forward * thrust * thrustPercent);
    }

    public void ApplyForces(Vector3 forceApplied, Vector3 torqueApplied) {
        rigidbody.AddForce(forceApplied);
        rigidbody.AddTorque(torqueApplied);
        rigidbody.AddForce(transform.forward * thrust * thrustPercent);
    }

    private Tuple <Vector3, Vector3> CalculateAerodynamicForces (Vector3 velocity, Vector3 angularVelocity, Vector3 wind, Vector3 centerOfMass, float airDensity) {
        Vector3 forceApplied = Vector3.zero;
        Vector3 torqueApplied = Vector3.zero;

        foreach (AerodynamicSurfacePhysics surface in aerodynamicSurfaces) {
            if (surface.gameObject.active == false)
                continue;

            Vector3 relativePosition = surface.transform.position - centerOfMass;

            Tuple<Vector3, Vector3> localForceTorqueApplied = surface.CalculateForces(
                wind - velocity - Vector3.Cross(angularVelocity, relativePosition),
                airDensity,
                relativePosition
            );

            forceApplied += localForceTorqueApplied.Item1;
            torqueApplied += localForceTorqueApplied.Item2;
        }

        return new Tuple<Vector3, Vector3>(forceApplied, torqueApplied);
    }

    private void CalculateGForce() {
        localGForce = Vector3.Cross(rigidbody.angularVelocity, rigidbody.velocity);
    }

#if UNITY_EDITOR
    // For gizmos drawing.
    public void CalculateCenterOfLift(out Vector3 center, out Vector3 force, Vector3 displayAirVelocity, float displayAirDensity) {
        Vector3 com;
        Tuple<Vector3, Vector3> forceAndTorque;
        if (aerodynamicSurfaces == null) {
            center = Vector3.zero;
            force = Vector3.zero;
            return;
        }

        if (rigidbody == null) {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = CalculateAerodynamicForces(-displayAirVelocity, Vector3.zero, Vector3.zero, com, displayAirDensity);
        } else {
            com = rigidbody.worldCenterOfMass;
            forceAndTorque = new Tuple<Vector3, Vector3>(forceApplied, torqueApplied);
        }

        force = forceAndTorque.Item1;
        center = com + Vector3.Cross(forceAndTorque.Item1, forceAndTorque.Item2) / forceAndTorque.Item1.sqrMagnitude;
    }
#endif
}

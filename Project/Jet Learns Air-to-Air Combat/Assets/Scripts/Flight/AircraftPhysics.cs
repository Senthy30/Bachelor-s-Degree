using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class AircraftPhysics : MonoBehaviour {

    const float PREDICTION_FRACTION = 0.5f;

    [SerializeField]
    private Vector3 localGForce;

    private Rigidbody m_rigidbody;
    private event Action<Collision> m_onCollision;

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
    
    private bool aerodynamicSurfacesValuesUpdated = false;
    private Vector3 position;
    private Vector3 localScale;
    private Vector3 velocity;
    private Vector3 angularVelocity;
    private Vector3 worldCenterOfMass;
    private Quaternion rotation;

    private void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();
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
            m_rigidbody.velocity,
            m_rigidbody.angularVelocity,
            Vector3.zero,
            m_rigidbody.worldCenterOfMass,
            1.2f
        );

        forceApplied = forceTorqueApplied.Item1;
        torqueApplied = forceTorqueApplied.Item2;

        GetComponent<Rigidbody>().AddForce(forceApplied);
        GetComponent<Rigidbody>().AddTorque(torqueApplied);
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust * thrustPercent);

        return;

        // calculate velocity prediction
        Vector3 force = forceTorqueApplied.Item1 + transform.forward * thrust * thrustPercent + Physics.gravity * GetComponent<Rigidbody>().mass;
        Vector3 velocityPrediction = GetComponent<Rigidbody>().velocity + Time.fixedDeltaTime * PREDICTION_FRACTION * force / GetComponent<Rigidbody>().mass;

        // calculate angular velocity prediction
        Quaternion inertiaTensorWorldRotation = GetComponent<Rigidbody>().rotation * GetComponent<Rigidbody>().inertiaTensorRotation;
        Vector3 torqueChange = Quaternion.Inverse(inertiaTensorWorldRotation) * forceTorqueApplied.Item2;
        Vector3 angularVelocityChange = new Vector3(
            torqueChange.x / GetComponent<Rigidbody>().inertiaTensor.x,
            torqueChange.y / GetComponent<Rigidbody>().inertiaTensor.y,
            torqueChange.z / GetComponent<Rigidbody>().inertiaTensor.z
        );
        Vector3 angularVelocityPrediction = GetComponent<Rigidbody>().angularVelocity + Time.fixedDeltaTime * PREDICTION_FRACTION * (inertiaTensorWorldRotation * angularVelocityChange);

        // calculate force and torque prediction applied
        Tuple<Vector3, Vector3> forceTorquePredictionApplied = CalculateAerodynamicForces(
            velocityPrediction,
            angularVelocityPrediction,
            Vector3.zero,
            GetComponent<Rigidbody>().worldCenterOfMass,
            1.2f
        );

        //Debug.Log(velocityPrediction);
        //Debug.Log(angularVelocityPrediction);

        // calculate force and torque that need to be applied
        forceApplied = (forceTorqueApplied.Item1 + forceTorquePredictionApplied.Item1) / 2f;
        torqueApplied = (forceTorqueApplied.Item2 + forceTorquePredictionApplied.Item2) / 2f;

        GetComponent<Rigidbody>().AddForce(forceApplied);
        GetComponent<Rigidbody>().AddTorque(torqueApplied);
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust * thrustPercent);
    }

    public void ApplyForces(ref Vector3 forceApplied, ref Vector3 torqueApplied) {
        GetComponent<Rigidbody>().AddForce(forceApplied);
        GetComponent<Rigidbody>().AddTorque(torqueApplied);
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust * thrustPercent);
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

    public void UpdateValuesForPhysics() {
        position = transform.position;
        rotation = transform.rotation;
        localScale = transform.localScale;

        velocity = m_rigidbody.velocity;
        angularVelocity = m_rigidbody.angularVelocity;
        worldCenterOfMass = m_rigidbody.worldCenterOfMass;

        if (!aerodynamicSurfacesValuesUpdated) {
            aerodynamicSurfacesValuesUpdated = true;
            for (int i = 0; i < aerodynamicSurfaces.Count; i++) {
                aerodynamicSurfaces[i].UpdateValuesForPhysics(transform);
            }
        }
    }

    public void AddFuncInOnCollision(System.Action<Collision> func) {
        m_onCollision += func;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision == null) return;

        m_onCollision?.Invoke(collision);
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

        if (GetComponent<Rigidbody>() == null) {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = CalculateAerodynamicForces(-displayAirVelocity, Vector3.zero, Vector3.zero, com, displayAirDensity);
        } else {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = new Tuple<Vector3, Vector3>(forceApplied, torqueApplied);
        }

        force = forceAndTorque.Item1;
        center = com + Vector3.Cross(forceAndTorque.Item1, forceAndTorque.Item2) / forceAndTorque.Item1.sqrMagnitude;
    }
#endif

    public ref Vector3 GetPosition() {
        return ref position;
    }

    public ref Vector3 GetLocalScale() {
        return ref localScale;
    }

    public ref Vector3 GetVelocity() {
        return ref velocity;
    }

    public ref Vector3 GetAngularVelocity() {
        return ref angularVelocity;
    }

    public ref Vector3 GetWorldCenterOfMass() {
        return ref worldCenterOfMass;
    }

    public ref Quaternion GetRotation() {
        return ref rotation;
    }
}

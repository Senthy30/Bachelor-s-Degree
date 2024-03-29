using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestGizmos : MonoBehaviour {

    [SerializeField] private Transform m_targetTransform;
    [SerializeField] private MissilePhysics m_missilePhysics;

    private List<HeatEmission> heatEmissionArray;

    private void Start() {
        heatEmissionArray = new List<HeatEmission> {
            new HeatEmission(m_targetTransform, 100)
        };

        m_missilePhysics.SetHeatEmissionArray(heatEmissionArray);
        m_missilePhysics.AddComponent<Rigidbody>();
        m_missilePhysics.LaunchMissile();
    }

#if UNITY_EDITOR

    [SerializeField] private float radius = 1;
    [SerializeField] private float angle = 30;
    [SerializeField] private float length = 100;
    [SerializeField] private int numCirclesDivisions = 1;

    private void OnDrawGizmosSelected() {
        GizmosDraw.DrawCone(transform, angle, length, numCirclesDivisions, Color.red);
    }

#endif
}

using System;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class HeatEmission {
    public Transform transform;
    public int heat;

    public HeatEmission(Transform transform, int heat) {
        this.transform = transform;
        this.heat = heat;
    }
}

public class MissilePhysics : MonoBehaviour {

    // static List<HeatEmission> heatEmissionArray = new List<HeatEmission>();

    private bool missileLaunched = false;

    private MissileData missileData;
    private Rigidbody rb;
    private Transform missileParentTransform;

    private List<HeatEmission> heatEmissionArray;
    private event Action<MissileData> actionOnLaunched;
    private event Action<MissileData> actionOnDestroy;

    [SerializeField]
    private float lifeTime = 25f;
    private float currentTimeLife = 0;

    [Header("Target")]
    [SerializeField]
    private float minTimeToFindTarget = 0.2f;
    private float currentTimeWaitedToFindTarget = 0;
    [SerializeField]
    private float maxAngleToDetect = 45f;
    [SerializeField]
    private float maxDistanceToDetect = 1500f;

    [Header("Movement")]
    [SerializeField]
    private float currentSpeed = 15;
    [SerializeField]
    private float speedMultiplier = 25f;
    [SerializeField]
    private float maxSpeed = 150;
    [SerializeField]
    private float rotateSpeed = 95;

    [Header("Detach")]
    [SerializeField]
    private float timeDetachDown = 0.2f;
    private bool detachedDown = false;
    private float currentTimeDetachDown = 0;

    [Header("Prediction")]
    [SerializeField]
    private float maxDistancePredict = 100;
    [SerializeField]
    private float minDistancePredict = 5;
    [SerializeField]
    private float maxTimePrediction = 5;

    private Vector3 standardPrediction;
    private Vector3 deviatedPrediction;

    [Header("Deviation")]
    [SerializeField] 
    private float deviationAmount = 50;
    [SerializeField] 
    private float deviationSpeed = 2;

    [Header("Smoke")]
    [SerializeField]
    private GameObject smokeGameObject;

    private int targetHeat = 0;
    private Vector3 lastVelocity;
    private Vector3 acceleration;
    [SerializeField]
    private GameObject target;

    private void Awake() {
        smokeGameObject.SetActive(false);
        lastVelocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    private void FixedUpdate() {
        if (detachedDown)
            DetachedDownMovement();

        if (!missileLaunched)
            return;

        FindTarget(null);
        MissileMovement();

        if (missileLaunched) {
            currentTimeLife += Time.fixedDeltaTime;
            if (currentTimeLife >= lifeTime) {
                actionOnDestroy?.Invoke(missileData);
                Destroy(gameObject);
            }
        }

        acceleration = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = rb.velocity;
    }

    private void FindTarget(Transform ignoreTransform) {
        currentTimeWaitedToFindTarget -= Time.fixedDeltaTime;
        if (currentTimeWaitedToFindTarget <= 0)
            currentTimeWaitedToFindTarget = minTimeToFindTarget;
        else {
            return;
        }

        if (target != null) {
            AircraftController aircraftControllerBefore = target.GetComponent<AircraftController>();
            if (aircraftControllerBefore != null) {
                aircraftControllerBefore.SetIncomingMissileTransform(null);
            }
        }

        targetHeat = -1;
        target = null;
        foreach (HeatEmission heatEmission in heatEmissionArray) {
            if (heatEmission.transform == null || !heatEmission.transform.gameObject.activeSelf)
                continue;
            if (ignoreTransform != null && heatEmission.transform == ignoreTransform)
                continue;

            float angleToTarget = GetAngleToHeatEmission(heatEmission.transform.position);
            float distanceToTarget = Vector3.Distance(transform.position, heatEmission.transform.position);
            if (angleToTarget <= maxAngleToDetect && distanceToTarget <= maxDistanceToDetect && heatEmission.heat > targetHeat) {
                targetHeat = heatEmission.heat;
                target = heatEmission.transform.gameObject;
            }
        }

        if (target != null) {
            AircraftController aircraftControllerAfter = target.GetComponent<AircraftController>();
            if (aircraftControllerAfter != null) {
                aircraftControllerAfter.SetIncomingMissileTransform(transform);
            }
        }
    }

    private float GetAngleToHeatEmission(Vector3 targetPosition) {
        return Vector3.Angle(transform.forward, targetPosition - transform.position);
    }

    private void MissileMovement() {
        if (currentSpeed < maxSpeed)
            currentSpeed += speedMultiplier * Time.fixedDeltaTime;

        rb.velocity = transform.forward * currentSpeed;
        if (target == null)
            return;

        float leadTimePercentage = Mathf.InverseLerp(minDistancePredict, maxDistancePredict, Vector3.Distance(transform.position, target.transform.position));

        PredictMovement(leadTimePercentage);
        AddDeviation(leadTimePercentage);
        RotateRocket();
    }

    private void PredictMovement(float leadTimePercentage) {
        float predictionTime = Mathf.Lerp(0, maxTimePrediction, leadTimePercentage);

        standardPrediction = target.GetComponent<Rigidbody>().position + target.GetComponent<Rigidbody>().velocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage) {
        Vector3 deviation = new Vector3(Mathf.Cos(Time.time * deviationSpeed), 0, 0);
        Vector3 predictionOffset = transform.TransformDirection(deviation) * deviationAmount * leadTimePercentage;

        deviatedPrediction = standardPrediction + predictionOffset;
    }

    private void RotateRocket() {
        Vector3 heading = deviatedPrediction - transform.position;
        Quaternion rotation = Quaternion.LookRotation(heading);

        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.fixedDeltaTime));
    }

    private void DetachedDownMovement() {
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(transform.up * -currentSpeed + transform.forward * currentSpeed, ForceMode.Acceleration);

        currentTimeDetachDown += Time.fixedDeltaTime;
        if (currentTimeDetachDown >= timeDetachDown) {
            missileLaunched = true;
            rb.constraints = RigidbodyConstraints.None;

            gameObject.AddComponent<BoxCollider>();
            GetComponent<BoxCollider>().size = new Vector3(1.33f, 1.36f, 7.8f);

            detachedDown = false;
            rb.velocity = new Vector3(
                rb.velocity.x,
                0f,
                rb.velocity.z
            );
        }
    }

    public void CreateMissileData() {
        missileData = new MissileData(this);
    }

    public void LaunchMissile(Transform ignoreTransform) {
        rb = GetComponent<Rigidbody>();
        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude;

        actionOnLaunched?.Invoke(missileData);
        transform.parent = missileParentTransform;
        detachedDown = true;
        smokeGameObject.SetActive(true);

        FindTarget(ignoreTransform);
    }

    public void AddFuncInActionOnLaunch(System.Action<MissileData> func) {
        actionOnLaunched += func;
    }

    public void AddFuncInActionOnCollision(System.Action<MissileData> func) {
        actionOnDestroy += func;
    }

    public void SetHeatEmissionArray(List<HeatEmission> heatEmissionsArray) {
        heatEmissionArray = heatEmissionsArray;
    }

    public void SetMissileParentTransform(Transform missileParentTransform) {
        this.missileParentTransform = missileParentTransform;
    }

    public void TriggerFindTarget() {
        currentTimeWaitedToFindTarget = -1;
        FindTarget(null);
    }

    public bool GetMissileLaunched() {
        return missileLaunched;
    }

    public Vector3 GetVelocity() {
        if (rb == null)
            return Vector3.zero;
        return rb.velocity;
    }

    public Vector3 GetAcceleration() {
        return acceleration;
    }

    public MissileData GetMissileData() {
        return missileData;
    }

    public void SetTarget(GameObject target) {
        this.target = target;
    }

    public GameObject GetTarget() {
        return target;
    }

    private void OnCollisionEnter(Collision collision) {
        actionOnDestroy?.Invoke(missileData);
        Destroy(gameObject);
    }

#if UNITY_EDITOR

    [Header("Gizmos")]

    [SerializeField] private bool enableGizmos = true;
    [SerializeField] private bool displayOnlyWhenSelected = true;
    [SerializeField] private int numCirclesDivisions = 1;

    private void OnDrawGizmos() {
        if (!displayOnlyWhenSelected)
            DrawGizmos();
    }

    private void OnDrawGizmosSelected() {
        if (displayOnlyWhenSelected)
            DrawGizmos();
    }

    private void DrawGizmos() {
        if (!enableGizmos)
            return;

        Color color = target != null ? Color.red : Color.white;

        GizmosDraw.DrawCone(transform, maxAngleToDetect, maxDistanceToDetect, numCirclesDivisions, color);

        if (missileLaunched && target != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, standardPrediction);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(standardPrediction, deviatedPrediction);
        }
    }

#endif

}

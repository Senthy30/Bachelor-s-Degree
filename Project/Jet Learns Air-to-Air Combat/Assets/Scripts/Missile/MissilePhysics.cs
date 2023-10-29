using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public struct HeatEmission {
    public Transform transform;
    public int heat;

    public HeatEmission(Transform transform, int heat) {
        this.transform = transform;
        this.heat = heat;
    }
}

public class MissilePhysics : MonoBehaviour {

    static List<HeatEmission> heatEmissionArray = new List<HeatEmission>();

    private bool missileLaunched = false;
    private Rigidbody rigidbody;

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
    [SerializeField]
    private GameObject target;

    private void Awake() {
        smokeGameObject.SetActive(false);
    }

    private void Update() {
        if (missileLaunched) {
            currentTimeLife += Time.deltaTime;
            if (currentTimeLife >= lifeTime)
                Destroy(gameObject);
        }
    }

    private void FixedUpdate() {
        if (detachedDown)
            DetachedDownMovement();

        if (!missileLaunched)
            return;

        FindTarget();
        MissileMovement();
    }

    private void FindTarget() {
        currentTimeWaitedToFindTarget -= Time.deltaTime;
        if (currentTimeWaitedToFindTarget <= 0)
            currentTimeWaitedToFindTarget = minTimeToFindTarget;
        else {
            return;
        }

        targetHeat = -1;
        target = null;
        foreach (HeatEmission heatEmission in heatEmissionArray) {
            float angleToTarget = GetAngleToHeatEmission(heatEmission.transform.position);
            if (angleToTarget <= maxAngleToDetect && heatEmission.heat > targetHeat) {
                targetHeat = heatEmission.heat;
                target = heatEmission.transform.gameObject;
            }
        }
    }

    private float GetAngleToHeatEmission(Vector3 targetPosition) {
        return Vector3.Angle(transform.forward, targetPosition - transform.position);
    }

    private void MissileMovement() {
        if (currentSpeed < maxSpeed)
            currentSpeed += speedMultiplier * Time.deltaTime;

        rigidbody.velocity = transform.forward * currentSpeed;
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

        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.deltaTime));
    }

    private void DetachedDownMovement() {
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.AddForce(transform.up * -currentSpeed + transform.forward * currentSpeed, ForceMode.Acceleration);

        currentTimeDetachDown += Time.deltaTime;
        if (currentTimeDetachDown >= timeDetachDown) {
            missileLaunched = true;
            rigidbody.constraints = RigidbodyConstraints.None;

            gameObject.AddComponent<BoxCollider>();
            GetComponent<BoxCollider>().size = new Vector3(1.33f, 1.36f, 7.8f);

            detachedDown = false;
            rigidbody.velocity = new Vector3(
                rigidbody.velocity.x,
                0f, 
                rigidbody.velocity.z
            );
        }
    }

    public void LaunchMissile() {
        rigidbody = GetComponent<Rigidbody>();
        currentSpeed = rigidbody.velocity.magnitude;

        detachedDown = true;
        smokeGameObject.SetActive(true);
    }

    public static void SetHeatEmissionArray(List<HeatEmission> heatEmissionsArray) {
        heatEmissionArray = heatEmissionsArray;
    }

    private void OnCollisionEnter(Collision collision) {
        

        Destroy(gameObject);
    }

    private void OnDrawGizmos() {
        if (missileLaunched && target != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, standardPrediction);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(standardPrediction, deviatedPrediction);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightMechanics : MonoBehaviour {

    private Rigidbody rigidbody;

    [Header("Thrust")]
    public float maxThrust = 10f;
    public float inputThrustIncrease = 5f;

    [SerializeField]
    float inputThrust;
    [SerializeField]
    float timePressedThrust = 0f;
    [SerializeField]
    AnimationCurve inputThrustCurve;
    [SerializeField]
    AnimationCurve thrustCurve;
    [SerializeField]
    AnimationCurve relativeForceCurve;

    [Header("Drag")]
    [SerializeField]
    bool airBrakeActive;
    [SerializeField]
    float dragMultiplier;
    [SerializeField]
    float airBreakDrag;
    [SerializeField]
    AnimationCurve dragForward;
    [SerializeField]
    AnimationCurve dragBack;
    [SerializeField]
    AnimationCurve dragLeft;
    [SerializeField]
    AnimationCurve dragRight;
    [SerializeField]
    AnimationCurve dragTop;
    [SerializeField]
    AnimationCurve dragBottom;

    [Header("Lift")]
    [SerializeField]
    float inputAngleOfAttackX;
    [SerializeField]
    float liftXPower;
    [SerializeField]
    float liftYPower;
    [SerializeField]
    float inducedDrag;
    [SerializeField]
    AnimationCurve aoaLiftXCurve;
    [SerializeField]
    AnimationCurve inducedDragXCurve;
    [SerializeField]
    AnimationCurve aoaLiftYCurve;
    [SerializeField]
    AnimationCurve inducedDragYCurve;

    [Header("Controls")]
    [SerializeField]
    float rotationUpDownSpeed;

    [Header("State")]
    public Vector2 angleOfAttack;

    public Vector3 velocity;
    public Vector3 lastVelocity;
    public Vector3 localVelocity;
    public Vector3 localAngularVelocity;

    public Vector3 localGForce;

    private void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        float deltaTime = Time.fixedDeltaTime;

        CalculateInputThrust(deltaTime);

        CalculateState(deltaTime);
        CalculateGForce(deltaTime);

        UpdateThrust();
        UpdateLift();

        UpdateDrag();
    }

    private void CalculateState(float deltaTime) {
        Quaternion inverseRotation = Quaternion.Inverse(rigidbody.rotation);
        velocity = rigidbody.velocity;
        localVelocity = inverseRotation * velocity; 
        localAngularVelocity = inverseRotation * rigidbody.angularVelocity;

        CalculateAngleOfAttack();
    }

    private void CalculateAngleOfAttack() {
        if (localVelocity.sqrMagnitude < 0.1f) {
            angleOfAttack = new Vector2(0, 0);

            return;
        }

        angleOfAttack = new Vector2(
                Mathf.Atan2(-localVelocity.y, localVelocity.z),
                Mathf.Atan2(localVelocity.x, localVelocity.z)
            );
    }

    private void CalculateGForce(float deltaTime) {
        Quaternion inverseRotation = Quaternion.Inverse(rigidbody.rotation);
        Vector3 acceleration = (velocity - lastVelocity) / deltaTime;

        localGForce = inverseRotation * acceleration;
        lastVelocity = velocity;
    }

    private void CalculateInputThrust (float deltaTime) {
        airBrakeActive = false;

        if (Input.GetKey(KeyCode.W)) {
            timePressedThrust += inputThrustIncrease * deltaTime;
        } else if (Input.GetKey(KeyCode.S)) {
            airBrakeActive = true;
        }

        if (Input.GetKey(KeyCode.Keypad8)) {
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x + rotationUpDownSpeed,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z
            );
        } else if (Input.GetKey(KeyCode.Keypad2)) {
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x - rotationUpDownSpeed,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z
            );
        }

        inputThrust = inputThrustCurve.Evaluate(timePressedThrust);
        //inputThrust = 1;

        timePressedThrust -= deltaTime;
        timePressedThrust = Mathf.Clamp(timePressedThrust, 0, 1);
    }

    private void UpdateThrust() {
        float offset = 0.1f;
        float magnitude = NormalizeValueBetween01(velocity.magnitude, 0f, maxThrust);
        float forceApplied = relativeForceCurve.Evaluate(inputThrust * (magnitude + offset));

        rigidbody.AddRelativeForce(forceApplied * maxThrust * Vector3.forward);
    }

    private void UpdateDrag() {
        Vector3 localVelocityNormalized = localVelocity;
        float magnitude = localVelocityNormalized.sqrMagnitude;

        float airbrakeDrag = airBrakeActive ? airBreakDrag : 0;

        Vector3 coefficient = Scale6(
            localVelocityNormalized.normalized,
            dragRight.Evaluate(Mathf.Abs(localVelocityNormalized.x)), dragLeft.Evaluate(Mathf.Abs(localVelocityNormalized.x)),
            dragTop.Evaluate(Mathf.Abs(localVelocityNormalized.y)), dragBottom.Evaluate(Mathf.Abs(localVelocityNormalized.y)),
            dragForward.Evaluate(Mathf.Abs(localVelocityNormalized.z)) + airbrakeDrag,
            dragBack.Evaluate(Mathf.Abs(localVelocityNormalized.z))
        );

        Vector3 dragForce = coefficient.magnitude * magnitude * -localVelocityNormalized.normalized;

        rigidbody.AddRelativeForce(dragForce * dragMultiplier);
    }

    void UpdateLift() {
        Vector3 liftXForce = CalculateLift(angleOfAttack.y, Vector3.right, liftXPower, aoaLiftXCurve);
        //Vector3 liftYForce = CalculateLift(angleOfAttack.y, Vector3.up, liftYPower, aoaLiftYCurve);

        rigidbody.AddRelativeForce(liftXForce);
        //rigidbody.AddRelativeForce(liftYForce);
    }

    private Vector3 CalculateLift(float valAngleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve) {
        Vector3 liftVelocity = Vector3.ProjectOnPlane(localVelocity, rightAxis);
        float magnitude = (localVelocity.x + localVelocity.z) * (localVelocity.x + localVelocity.z);

        float liftCoefficient = aoaCurve.Evaluate(valAngleOfAttack * Mathf.Rad2Deg);
        float liftForce = magnitude * liftCoefficient * liftPower;

        Vector3 liftDirection = Vector3.Cross(localVelocity.normalized, rightAxis);
        Vector3 lift = liftDirection * liftForce;
        lift.x = 0;
        lift.z = 0;

        Debug.Log(lift);

        float dragForce = liftCoefficient * liftCoefficient * inducedDrag;
        Vector3 dragDirection = -localVelocity.normalized;
        Vector3 inducedDragNorm = dragDirection * magnitude * dragForce;

        return lift;
    }

    private Vector3 Scale6(Vector3 value, float posX, float negX, float posY, float negY, float posZ, float negZ) {
        Vector3 result = value;

        if (result.x > 0) {
            result.x *= posX;
        } else if (result.x < 0) {
            result.x *= negX;
        }

        if (result.y > 0) {
            result.y *= posY;
        } else if (result.y < 0) {
            result.y *= negY;
        }

        if (result.z > 0) {
            result.z *= posZ;
        } else if (result.z < 0) {
            result.z *= negZ;
        }

        return result;
    }

    private float NormalizeValueBetween01(float value, float minValue, float maxValue) {
        return (value - minValue) * (maxValue - minValue);
    }

    private bool AmIntrat = false;

    private void OnCollisionEnter(Collision collision) {
        if (collision == null) return;
        if (collision.gameObject.layer == 3) return;

        Debug.Log("Am intrat in ceva");
        AmIntrat = true;
    }

}

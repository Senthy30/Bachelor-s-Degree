using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AircraftController : MonoBehaviour {

    const float epsilon = 1e-5f;

    [SerializeField]
    List<AerodynamicSurfacePhysics> controlSurfaces = null;

    [SerializeField]
    List<WheelCollider> wheels = null;

    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    private float pitchLastInputSign = 0f;
    private float pitchInputTimePressed = 0f;
    [Range(-1, 1)]
    public float pitchInput;

    private float yawLastInputSign = 0f;
    private float yawInputTimePressed = 0f;
    [Range(-1, 1)]
    public float yawInput;

    private float rollLastInputSign = 0f;
    private float rollInputTimePressed = 0f;
    [Range(-1, 1)]
    public float rollInput;

    private bool flapInputWasPressed;
    private const float maxFlapInput = 0.3f;
    private float flapInputTimePressed = 0f;
    [Range(0, 1)]
    public float flapInput;

    [Range(0, 1)]
    public float thrustInput;

    [SerializeField]
    private float timePressedIncrease = 0.5f;
    [SerializeField]
    private float valueMultiplierIncrease = 3f;
    [SerializeField]
    private float timePressedDecrease = 0.35f;
    [SerializeField]
    private float valueMultiplierDecrease = 3f;
    [SerializeField]
    private AnimationCurve inputsEventIncreaseCurve;
    [SerializeField]
    private AnimationCurve inputsEventDecreaseCurve;

    [SerializeField]
    private Animator gearAnimator;
    [SerializeField]
    private Animator flapAnimator;
    [SerializeField]
    private Animator rollAnimator;
    [SerializeField]
    private Animator pitchAnimator;
    [SerializeField]
    private Animator yawAnimator;

    private bool gearIsClosed;
    private bool gearAnimationIsRunning;
    [SerializeField]
    private GameObject gearObject;
    [SerializeField]
    private GameObject closedGearObject;

    [SerializeField]
    private GameObject leftFlameObject;
    [SerializeField]
    private GameObject rightFlameObject;

    [Header("Missile")]
    [SerializeField]
    private GameObject missileStorageObject;
    private Transform sceneMissileParentObject;
    private List<GameObject> missileArray = new List<GameObject>();

    float brakesTorque;

    private AircraftPhysics aircraftPhysics;
    private GameObject sceneObject;
    private Rigidbody rb;
    //private Animator animator;

    private void Start() {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;

        gearIsClosed = false;
        gearAnimationIsRunning = false;

        Material engineFlameMaterial = leftFlameObject.GetComponent<Renderer>().material;
        leftFlameObject.GetComponent<Renderer>().material = new Material(engineFlameMaterial);
        rightFlameObject.GetComponent<Renderer>().material = new Material(engineFlameMaterial);
    }

    private void Update() {
        HandleInputsEvent();

        if (Input.GetKeyDown(KeyCode.B)) {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }
    }

    private void FixedUpdate() {
        SetControlSurfecesAngles(pitchInput, rollInput, yawInput, flapInput);

        aircraftPhysics.SetThrustPercent(thrustInput);

        foreach (var wheel in wheels) {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap) {
        foreach (var surface in controlSurfaces) {
            if (surface == null || !surface.IsControlSurface) 
                continue;

            switch (surface.InputType) {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplier);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplier);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplier);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(flapInput * surface.InputMultiplier);
                    break;
            }
        }
    }

    private void HandleInputsEvent() {
        HandleSpecificInputEvent("Vertical", ref pitchInputTimePressed, ref pitchInput, ref pitchLastInputSign);
        HandleSpecificInputEvent("Horizontal", ref rollInputTimePressed, ref rollInput, ref rollLastInputSign);
        HandleSpecificInputEvent("Yaw", ref yawInputTimePressed, ref yawInput, ref yawLastInputSign);

        HandleFlapInputEvent();

        HandleThrustInputEvent();

        HandleInputsAnimation();
        HandleGearInputEvent();

        HandleAttackInputEvent();
    }

    private void HandleSpecificInputEvent(string nameInputEvent, ref float valueInputTimePressed, ref float valueInput, ref float lastInputSign) {
        float valueInputSign = GetValueInputAxis(nameInputEvent);

        if (valueInputSign != lastInputSign) {
            valueInputTimePressed = 0;
            lastInputSign = valueInputSign;
        }

        if (valueInputSign != 0) {
            valueInputTimePressed += timePressedIncrease * Time.deltaTime;
            valueInputTimePressed = Mathf.Clamp(valueInputTimePressed, 0f, 1f);

            valueInput += valueInputSign * inputsEventIncreaseCurve.Evaluate(valueInputTimePressed) * Time.deltaTime * valueMultiplierIncrease;
        } else if (valueInput != 0f) {
            //DecreaseInput(ref valueInputTimePressed);
            valueInputTimePressed += timePressedDecrease * Time.deltaTime;

            if (valueInput < 0f) {
                valueInput += inputsEventDecreaseCurve.Evaluate(Mathf.Abs(valueInput)) * Time.deltaTime * valueMultiplierDecrease;
                if (valueInput > 0f)
                    valueInput = 0f;
            } else {
                valueInput -= inputsEventDecreaseCurve.Evaluate(Mathf.Abs(valueInput)) * Time.deltaTime * valueMultiplierDecrease;
                if (valueInput < 0f)
                    valueInput = 0f;
            }
        }

        if (Mathf.Abs(valueInput) < epsilon)
            valueInput = 0f;
        else if (valueInput < -1f)
            valueInput = -1f;
        else if (valueInput > 1f)
            valueInput = 1f;
    }

    /*
    private void HandleSpecificInputEvent(string nameInputEvent, ref float valueInputTimePressed, ref float valueInput) {
        float valueInputSign = 0;
        float valueInputAxis = GetValueInputAxis(nameInputEvent);

        if (valueInputAxis > 0)
            valueInputSign = 1f;
        else if (valueInputAxis < 0) 
            valueInputSign = -1f;

        if (valueInputSign != 0) {
            valueInputTimePressed += valueInputSign * timePressedIncrease * Time.deltaTime;
            valueInputTimePressed = Mathf.Clamp(valueInputTimePressed, -1f, 1f);
        } else {
            DecreaseInput(ref valueInputTimePressed);
        }

        valueInput = inputsEventCurve.Evaluate(valueInputTimePressed);

        if (Mathf.Abs(valueInput) < epsilon)
            valueInput = 0f;
        else if (valueInput < -1f)
            valueInput = -1f;
        else if (valueInput > 1f)
            valueInput = 1f;
    }
    */

    private void HandleFlapInputEvent() {
        if (Input.GetKeyDown(KeyCode.F))
            flapInputWasPressed = !flapInputWasPressed;

        if (flapInputWasPressed) {
            flapInputTimePressed += timePressedIncrease * Time.deltaTime;
        } else {
            flapInputTimePressed -= timePressedDecrease * Time.deltaTime;
        }

        flapInputTimePressed = Mathf.Clamp01(flapInputTimePressed);
        flapInput = inputsEventIncreaseCurve.Evaluate(flapInputTimePressed) * maxFlapInput;

        if (Mathf.Abs(flapInput) < epsilon)
            flapInput = 0f;
    }

    private void HandleThrustInputEvent() {
        float valueInputAxis = GetValueInputAxis("Thrust");

        if (valueInputAxis > 0f) {
            thrustInput += timePressedIncrease * Time.deltaTime;
        } else if (valueInputAxis < 0f) {
            thrustInput -= timePressedDecrease * Time.deltaTime;
        }

        thrustInput = Mathf.Clamp01(thrustInput);
    }

    private void HandleGearInputEvent() {
        if (Input.GetKeyDown(KeyCode.G) && !gearAnimationIsRunning) {
            if (!gearIsClosed) {
                gearAnimator.SetBool("startCloseGearAnimation", true);
            } else {
                gearObject.SetActive(true);
                closedGearObject.SetActive(false);

                gearAnimator.SetBool("startOpenGearAnimation", true);
            }

            gearAnimationIsRunning = true;
            return;
        }

        if (gearAnimationIsRunning) {
            if (!gearIsClosed && gearAnimator.GetBool("startCloseGearAnimation") && !gearAnimator.GetBool("enterClosedGearState")) {
                gearAnimator.SetBool("startCloseGearAnimation", false);
                gearAnimator.SetTrigger("enterClosedGearState");
                gearAnimator.SetTrigger("idleClosedGearState");

                gearObject.SetActive(false);
                closedGearObject.SetActive(true);

                gearIsClosed = true;
                gearAnimationIsRunning = false;
            }

            if (gearIsClosed && gearAnimator.GetBool("startOpenGearAnimation") && !gearAnimator.GetBool("enterOpenedGearState")) {
                gearAnimator.SetBool("startOpenGearAnimation", false);
                gearAnimator.SetTrigger("enterOpenedGearState");

                gearIsClosed = false;
                gearAnimationIsRunning = false;
            }
        }
    }

    private void HandleInputsAnimation() {
        leftFlameObject.GetComponent<Renderer>().material.SetFloat("_Thrust", 1f - thrustInput / 2f);
        rightFlameObject.GetComponent<Renderer>().material.SetFloat("_Thrust", 1f - thrustInput / 2f);

        flapAnimator.SetFloat("flapInput", flapInput / maxFlapInput);
        flapAnimator.SetFloat("flapInputAbsolute", Mathf.Abs(flapInput / maxFlapInput));

        rollAnimator.SetFloat("rollInput", rollInput);
        rollAnimator.SetFloat("rollInputAbsolute", Mathf.Abs(rollInput));

        pitchAnimator.SetFloat("pitchInput", pitchInput);
        pitchAnimator.SetFloat("pitchInputAbsolute", Mathf.Abs(pitchInput));

        yawAnimator.SetFloat("yawInput", yawInput);
        yawAnimator.SetFloat("yawInputAbsolute", Mathf.Abs(yawInput));
    }

    private float GetValueInputAxis(string nameInputEvent) {
        if (nameInputEvent == "Vertical") {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                return -1f;
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                return 1f;
        }

        if (nameInputEvent == "Horizontal") {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                return -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                return 1f;
        }

        if (nameInputEvent == "Yaw") {
            if (Input.GetKey(KeyCode.E))
                return -1f;
            else if (Input.GetKey(KeyCode.Q))
                return 1f;
        }

        if (nameInputEvent == "Thrust") {
            if (Input.GetKey(KeyCode.LeftControl))
                return -1f;
            else if (Input.GetKey(KeyCode.Space))
                return 1f;
        }

        return 0f;
    }

    private void HandleAttackInputEvent() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            LaunchMissile();
        }
    }

    private void LaunchMissile() {
        missileArray[missileArray.Count - 1].transform.parent = sceneMissileParentObject;
        missileArray[missileArray.Count - 1].AddComponent<Rigidbody>();

        Rigidbody missileRigidbody = missileArray[missileArray.Count - 1].GetComponent<Rigidbody>();
        missileRigidbody.velocity = rb.velocity;
        missileRigidbody.angularVelocity = Vector3.zero;
        //missileRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        missileRigidbody.useGravity = false;

        missileArray[missileArray.Count - 1].GetComponent<MissilePhysics>().LaunchMissile();
        missileArray.RemoveAt(missileArray.Count - 1);
    }

    public void SetSceneMissileParentObject(Transform transform) {
        sceneMissileParentObject = transform;
    }

    public void SetSceneObject(GameObject sceneObject) {
        this.sceneObject = sceneObject;
    }

    public void AddMissilesInArray() {
        foreach (Transform child in missileStorageObject.transform)
            missileArray.Add(child.gameObject);
    }
}

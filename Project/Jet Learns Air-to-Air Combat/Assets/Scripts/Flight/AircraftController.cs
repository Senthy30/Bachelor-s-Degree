using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AircraftController : MonoBehaviour {

    private enum InputType { HORIZONTAL, VERTICAL, YAW, THRUST }

    private static TheaterData theaterData;

    const float epsilon = 1e-5f;

    [SerializeField]
    List<AerodynamicSurfacePhysics> controlSurfaces = null;

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
    private List <AerodynamicSurfacePhysics> pitchSurfacesArray;

    private float yawLastInputSign = 0f;
    private float yawInputTimePressed = 0f;
    [Range(-1, 1)]
    public float yawInput;
    private List<AerodynamicSurfacePhysics> yawSurfacesArray;

    private float rollLastInputSign = 0f;
    private float rollInputTimePressed = 0f;
    [Range(-1, 1)]
    public float rollInput;
    private List<AerodynamicSurfacePhysics> rollSurfacesArray;

    private bool flapInputWasPressed;
    private const float maxFlapInput = 0.3f;
    private float flapInputSign;
    private float flapInputTimePressed = 0f;
    [Range(0, 1)]
    public float flapInput;
    private List<AerodynamicSurfacePhysics> flapSurfacesArray;

    [Range(0, 1)]
    public float thrustInput;

    private bool decoyInputWasPressed;

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

    private bool gearInputWasPressed;
    private bool gearIsClosed;
    private bool gearAnimationIsRunning;
    [SerializeField]
    private bool closeGearAfterTakeOffAutomatically = true;
    private int closeGearAfterTakeOffAutomaticallyCountWheels = 0;
    [SerializeField]
    private float closeGearAfterTakeOffTime = 4f;
    private float closeGearAfterTakeOffCurrentTime = 0f;
    [SerializeField]
    private GameObject gearObject;
    [SerializeField]
    private GameObject closedGearObject;

    private const float gearTimeTransition = 2.5f;
    private const float distanceGearTransition = 2f;
    private const float waitTimeToOpenGear = 0.2f;
    private float gearCurrentTimeTransition = 0.0f;
    private float waitCurrentTimeToOpenGear = 0.0f;
    private float gearPositionYWhenGearIsOpened;
    [SerializeField] private Transform wheelsParentObject;

    [SerializeField]
    private GameObject leftFlameObject;
    [SerializeField]
    private GameObject rightFlameObject;

    [Header("Missile")]
    private bool attackInputWasPressed;
    [SerializeField] private GameObject missileStorageObject;
    private Transform sceneMissileParentObject;
    private Transform incomingMissileTransform;
    private List<GameObject> missileArray = new List<GameObject>();

    [SerializeField] public bool m_useFakeDecoyData = false;
    [SerializeField] public bool m_useFakeMissileData = false;

    private int skipHandleInputsEvent;
    private Team team;
    private AircraftPhysics aircraftPhysics;
    private JetData jetData;
    private SceneData sceneData;
    private Rigidbody rb;

    private float rollInputSign;
    private float pitchInputSign;
    private float yawInputSign;
    private float thrustInputSign;

    public Vector3 acceleration;
    public Vector3 angularAcceleration;
    public Vector3 lastVelocity = Vector3.zero;
    public Vector3 lastAngularVelocity = Vector3.zero;

    private void Awake() {
        gearInputWasPressed = false;
        gearIsClosed = false;
        gearAnimationIsRunning = false;

        flapInputWasPressed = false;
        attackInputWasPressed = false;
        flapInputSign = 0.0f;
        gearPositionYWhenGearIsOpened = wheelsParentObject.localPosition.y;
    }

    private void Start() {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;

        Material engineFlameMaterial = leftFlameObject.GetComponent<Renderer>().material;
        leftFlameObject.GetComponent<Renderer>().material = new Material(engineFlameMaterial);
        rightFlameObject.GetComponent<Renderer>().material = new Material(engineFlameMaterial);

        PrecalculateAndStoreSufaces();

        JetAgent jetAgent = gameObject.GetComponent<JetAgent>();
        if (jetAgent != null) {
            jetAgent.ConfigureOnStart(this);
        }
    }

    private void FixedUpdate() {
        SetControlSurfecesAngles();

        aircraftPhysics.SetThrustPercent(thrustInput);

        acceleration = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        angularAcceleration = (rb.angularVelocity - lastAngularVelocity) / Time.fixedDeltaTime;
        lastVelocity = rb.velocity;
        lastAngularVelocity = rb.angularVelocity;

        HandleInputsEvent(false);
    }

    public void SetControlSurfecesAngles() {
        // pitch
        foreach (AerodynamicSurfacePhysics surface in pitchSurfacesArray) { 
            surface.SetFlapAngle(pitchInput * pitchControlSensitivity * surface.InputMultiplier);
        }

        // roll
        foreach (AerodynamicSurfacePhysics surface in rollSurfacesArray) {
            surface.SetFlapAngle(rollInput * rollControlSensitivity * surface.InputMultiplier);
        }

        // yaw
        foreach (AerodynamicSurfacePhysics surface in yawSurfacesArray) {
            surface.SetFlapAngle(yawInput * yawControlSensitivity * surface.InputMultiplier);
        }

        // flap 
        foreach (AerodynamicSurfacePhysics surface in flapSurfacesArray) {
            surface.SetFlapAngle(flapInput * surface.InputMultiplier);
        }
    }

    public void HandleInputsEvent(bool shouldSkipNext) {
        if (skipHandleInputsEvent > 0) {
            skipHandleInputsEvent--;
            return;
        }
        if (shouldSkipNext)
            skipHandleInputsEvent++;

        HandleSpecificInputEvent(pitchInputSign, ref pitchInputTimePressed, ref pitchInput, ref pitchLastInputSign);
        HandleSpecificInputEvent(rollInputSign, ref rollInputTimePressed, ref rollInput, ref rollLastInputSign);
        HandleSpecificInputEvent(yawInputSign, ref yawInputTimePressed, ref yawInput, ref yawLastInputSign);

        HandleFlapInputEvent();

        HandleThrustInputEvent();

        HandleInputsAnimation();
        HandleGearInputEvent();

        HandleAttackInputEvent();
        HandleDecoyInputEvent();
    }

    private void HandleSpecificInputEvent(float valueInputSign, ref float valueInputTimePressed, ref float valueInput, ref float lastInputSign) {
        if (valueInputSign != lastInputSign) {
            valueInputTimePressed = 0;
            lastInputSign = valueInputSign;
        }

        if (valueInputSign != 0) {
            valueInputTimePressed += timePressedIncrease * Time.fixedDeltaTime;
            valueInputTimePressed = Mathf.Clamp(valueInputTimePressed, 0f, 1f);

            valueInput += valueInputSign * inputsEventIncreaseCurve.Evaluate(valueInputTimePressed) * Time.fixedDeltaTime * valueMultiplierIncrease;
        } else if (valueInput != 0f) {
            valueInputTimePressed += timePressedDecrease * Time.fixedDeltaTime;

            if (valueInput < 0f) {
                valueInput += inputsEventDecreaseCurve.Evaluate(Mathf.Abs(valueInput)) * Time.fixedDeltaTime * valueMultiplierDecrease;

                if (valueInput > -1e-4f) {
                    valueInput = 0f;
                    return;
                }
            } else {
                valueInput -= inputsEventDecreaseCurve.Evaluate(Mathf.Abs(valueInput)) * Time.fixedDeltaTime * valueMultiplierDecrease;

                if (valueInput < 1e-4f) {
                    valueInput = 0f;
                    return;
                }
            }
        }

        if (valueInput < -1f)
            valueInput = -1f;
        else if (valueInput > 1f)
            valueInput = 1f;
    }

    private void HandleFlapInputEvent() {
        if (!flapInputWasPressed && flapInput == 0f)
            return;

        if (flapInputWasPressed) {
            flapInputSign = 1f - flapInputSign;
            flapInputWasPressed = false;
        }

        if (flapInputSign > 0f) {
            flapInputTimePressed += timePressedIncrease * Time.fixedDeltaTime;
            flapInputTimePressed = Mathf.Clamp01(flapInputTimePressed);
            flapInput += inputsEventIncreaseCurve.Evaluate(flapInputTimePressed) * maxFlapInput * timePressedDecrease * Time.fixedDeltaTime;
        } else {
            flapInputTimePressed -= timePressedDecrease * Time.fixedDeltaTime;
            flapInputTimePressed = Mathf.Clamp01(flapInputTimePressed);
            flapInput -= inputsEventDecreaseCurve.Evaluate(flapInputTimePressed) * maxFlapInput * timePressedDecrease * Time.fixedDeltaTime;
        }

        if (flapInput >= maxFlapInput)
            flapInput = maxFlapInput;
        if (flapInput < epsilon)
            flapInput = 0f;
    }

    private void HandleThrustInputEvent() {
        if (thrustInputSign > 0f) {
            thrustInput += timePressedIncrease * Time.fixedDeltaTime;
        } else if (thrustInputSign < 0f) {
            thrustInput -= timePressedDecrease * Time.fixedDeltaTime;
        }

        thrustInput = Mathf.Clamp01(thrustInput);
    }

    private void HandleGearInputEvent() {
        if (closeGearAfterTakeOffAutomatically) {
            if (transform.localPosition.y >= SceneRewards.rewardsConfig.distanceToFloorNCeiling) {
                if (!gearIsClosed && !gearAnimationIsRunning)
                    gearInputWasPressed = true;
                closeGearAfterTakeOffAutomatically = false;
            } else {
                closeGearAfterTakeOffCurrentTime = 0;
            }
            //if (closeGearAfterTakeOffAutomaticallyCountWheels > 0) {
            //    closeGearAfterTakeOffCurrentTime = 0;
            //} else {
            //    closeGearAfterTakeOffCurrentTime += Time.fixedDeltaTime;
            //    if (closeGearAfterTakeOffCurrentTime >= closeGearAfterTakeOffTime) {
            //        if (!gearIsClosed && !gearAnimationIsRunning)
            //            gearInputWasPressed = true;
            //        closeGearAfterTakeOffAutomatically = false;
            //    }
            //}
        }

        if (gearInputWasPressed && !gearAnimationIsRunning) {
            if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
                if (!gearIsClosed) {
                    gearAnimator.SetBool("startCloseGearAnimation", true);
                } else {
                    gearObject.SetActive(true);
                    closedGearObject.SetActive(false);

                    gearAnimator.SetBool("startOpenGearAnimation", true);
                }
            }

            gearAnimationIsRunning = true;
            gearInputWasPressed = false;

            gearCurrentTimeTransition = 0.0f;
            waitCurrentTimeToOpenGear = 0.0f;

            return;
        }

        if (gearAnimationIsRunning) {
            RunGearTransition();

            if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
                if (!gearIsClosed && gearAnimator.GetBool("startCloseGearAnimation") && !gearAnimator.GetBool("enterClosedGearState")) {
                    gearAnimator.SetBool("startCloseGearAnimation", false);
                    gearAnimator.SetTrigger("enterClosedGearState");
                    gearAnimator.SetTrigger("idleClosedGearState");

                    gearIsClosed = true;
                    gearAnimationIsRunning = false;

                    FinishGearTransition();

                    gearObject.SetActive(false);
                    closedGearObject.SetActive(true);
                }

                if (gearIsClosed && gearAnimator.GetBool("startOpenGearAnimation") && !gearAnimator.GetBool("enterOpenedGearState")) {
                    gearAnimator.SetBool("startOpenGearAnimation", false);
                    gearAnimator.SetTrigger("enterOpenedGearState");
                    ReconfigGearToInitialState();

                    gearIsClosed = false;
                    gearAnimationIsRunning = false;

                    FinishGearTransition();
                }
            } else {
                if (gearCurrentTimeTransition >= gearTimeTransition) {
                    gearIsClosed = !gearIsClosed;
                    gearAnimationIsRunning = false;

                    FinishGearTransition();
                }
            }
        }
    }

    private void HandleInputsAnimation() {
        if (TheaterData.GetResolution() == Resolution.LOW_RESOLUTION)
            return;

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

    private void HandleAttackInputEvent() {
        if (attackInputWasPressed) {
            LaunchMissile();
            attackInputWasPressed = false;
        }
    }

    private void HandleDecoyInputEvent() {
        if (decoyInputWasPressed) {
            sceneData.DropDecoy(team);
            decoyInputWasPressed = false;
        }
    }

    // Reconfig

    public void ReconfigToInitialState() {
        ReconfigPitchToInitialState();
        ReconfigYawToInitialState();
        ReconfigRollToInitialState();
        ReconfigFlapsToInitialState();
        ReconfigThrustToInitialState();
        ReconfigGearToInitialState();
        closeGearAfterTakeOffAutomatically = true;

        HandleInputsAnimation();
    }

    // Collision

    private void OnCollisionEnter(Collision collision) {
        closeGearAfterTakeOffAutomaticallyCountWheels += 1;
    }

    private void OnCollisionExit(Collision collision) {
        closeGearAfterTakeOffAutomaticallyCountWheels -= 1;
        if (closeGearAfterTakeOffAutomaticallyCountWheels < 0)
            closeGearAfterTakeOffAutomaticallyCountWheels = 0;
    }

    // Transitions

    private void RunGearTransition() {
        if (gearCurrentTimeTransition >= gearTimeTransition)
            return;

        float moveWith = distanceGearTransition / gearTimeTransition * Time.fixedDeltaTime;
        gearCurrentTimeTransition += Time.fixedDeltaTime;

        if (!gearIsClosed) {
            //wheelsParentObject.position = new Vector3(wheelsParentObject.position.x, wheelsParentObject.position.y + moveWith, wheelsParentObject.position.z);
            wheelsParentObject.localPosition = new Vector3(
                wheelsParentObject.localPosition.x, 
                wheelsParentObject.localPosition.y + moveWith, 
                wheelsParentObject.localPosition.z
            );
        } else if (gearIsClosed) {
            if (waitCurrentTimeToOpenGear >= waitTimeToOpenGear) {
                wheelsParentObject.localPosition = new Vector3(
                    wheelsParentObject.localPosition.x,
                    wheelsParentObject.localPosition.y - moveWith,
                    wheelsParentObject.localPosition.z
                );
            } else {
                waitCurrentTimeToOpenGear += Time.fixedDeltaTime;
                gearCurrentTimeTransition = 0;
            }
        }
    }

    private void FinishGearTransition() {
        if (!gearIsClosed)
            wheelsParentObject.localPosition = new Vector3(wheelsParentObject.localPosition.x, gearPositionYWhenGearIsOpened, wheelsParentObject.localPosition.z);
        else if (gearIsClosed)
            wheelsParentObject.localPosition = new Vector3(wheelsParentObject.localPosition.x, gearPositionYWhenGearIsOpened + distanceGearTransition, wheelsParentObject.localPosition.z);    
    }

    private void LaunchMissile() {
        if (jetData.GetNumUnlaunchedMissiles() <= 0) {
            return;
        }

        missileArray[missileArray.Count - 1].transform.parent = sceneMissileParentObject;
        missileArray[missileArray.Count - 1].AddComponent<Rigidbody>();

        Rigidbody missileRigidbody = missileArray[missileArray.Count - 1].GetComponent<Rigidbody>();
        missileRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        missileRigidbody.velocity = rb.velocity;
        missileRigidbody.angularVelocity = Vector3.zero;
        missileRigidbody.useGravity = false;

        missileArray[missileArray.Count - 1].GetComponent<MissilePhysics>().LaunchMissile(transform);
        missileArray.RemoveAt(missileArray.Count - 1);
    }

    private void ReconfigPitchToInitialState() {
        pitchInput = 0;
        pitchInputSign = 0;
        pitchInputTimePressed = 0;
        pitchLastInputSign = 0;

        if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
            pitchAnimator.Rebind();
            pitchAnimator.Update(0f);
        }
    }

    private void ReconfigYawToInitialState() {
        yawInput = 0;
        yawInputSign = 0;
        yawInputTimePressed = 0;
        yawLastInputSign = 0;

        if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
            yawAnimator.Rebind();
            yawAnimator.Update(0f);
        }
    }

    private void ReconfigRollToInitialState() {
        rollInput = 0;
        rollInputSign = 0;
        rollInputTimePressed = 0;
        rollLastInputSign = 0;

        if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
            rollAnimator.Rebind();
            rollAnimator.Update(0f);
        }
    }

    private void ReconfigFlapsToInitialState() {
        flapInputWasPressed = false;
        flapInputTimePressed = 0;
        flapInput = 0;

        if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
            flapAnimator.Rebind();
            flapAnimator.Update(0f);
        }
    }

    private void ReconfigThrustToInitialState() {
        thrustInput = 0;
        thrustInputSign = 0;
    }

    private void ReconfigGearToInitialState() {
        gearObject.SetActive(true);
        closedGearObject.SetActive(false);

        wheelsParentObject.localPosition = new Vector3(wheelsParentObject.localPosition.x, gearPositionYWhenGearIsOpened, wheelsParentObject.localPosition.z);
        gearIsClosed = false;
        gearInputWasPressed = false;

        closeGearAfterTakeOffCurrentTime = 0;
        closeGearAfterTakeOffAutomaticallyCountWheels = 0;

        gearAnimationIsRunning = false;

        if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
            gearAnimator.Rebind();
            gearAnimator.Update(0f);
        }
    }

    private void PrecalculateAndStoreSufaces() {
        pitchSurfacesArray = new List<AerodynamicSurfacePhysics>();
        rollSurfacesArray = new List<AerodynamicSurfacePhysics>();
        yawSurfacesArray = new List<AerodynamicSurfacePhysics>();
        flapSurfacesArray = new List<AerodynamicSurfacePhysics>();

        foreach (AerodynamicSurfacePhysics surface in controlSurfaces) {
            switch (surface.InputType) {
                case ControlInputType.Pitch:
                    pitchSurfacesArray.Add(surface);
                    break;
                case ControlInputType.Roll:
                    rollSurfacesArray.Add(surface);
                    break;
                case ControlInputType.Yaw:
                    yawSurfacesArray.Add(surface);
                    break;
                case ControlInputType.Flap:
                    flapSurfacesArray.Add(surface);
                    break;
            }
        }
    }

    public void SetSceneMissileParentObject(Transform transform) {
        sceneMissileParentObject = transform;
    }

    public void SetSceneObject(SceneData sceneData) {
        this.sceneData = sceneData;
    }

    public void SetTeam(Team team) {
        this.team = team;
    }

    public void SetJetData(JetData jetData) {
        this.jetData = jetData;
    }

    public void SetIncomingMissileTransform(Transform mTransform) {
        incomingMissileTransform = mTransform;
    }

    public Transform GetIncomingMissileTransform() {
        return incomingMissileTransform;
    }

    public JetData GetJetData() {
        return jetData;
    }

    public SceneData GetSceneData() {
        return sceneData;
    }

    public void AddMissilesInArray(List <GameObject> missileArrayTemp) {
        missileArray = new List<GameObject>(missileArrayTemp);
    }

    public static void SetTheaterData(TheaterData theaterData) {
        AircraftController.theaterData = theaterData;
    }

    // Scenario

    public void SetThrustInputForScenario(float value) {
        thrustInput = value;
    }

    public void SetGearClosed() {
        gearInputWasPressed = false;
        closeGearAfterTakeOffAutomatically = false;
        gearIsClosed = true;
        gearAnimationIsRunning = false;

        FinishGearTransition();

        gearObject.SetActive(false);
        closedGearObject.SetActive(true);
    }

    // Input getters

    public float GetPitchInput() {
        return pitchInputSign;
    }

    public float GetRollInput() {
        return rollInputSign;
    }

    public float GetYawInput() {
        return yawInputSign;
    }

    public float GetFlapInputSign() {
        return flapInputSign;
    }

    public float GetThrustInput() {
        return thrustInputSign;
    }
    
    public bool GetFlapInput() {
        return flapInputWasPressed;
    }

    public bool GetGearInput() {
        return gearInputWasPressed;
    }

    public bool GetGearIsClosed() {
        return gearIsClosed;
    }

    public bool GetDecoyInput() {
        return decoyInputWasPressed;
    }

    public bool GetAttackInput() {
        return attackInputWasPressed;
    }


    // Input setters 

    public void SetPitchInput(float value) {
        pitchInputSign = value;
    }

    public void SetRollInput(float value) {
        rollInputSign = value;
    }

    public void SetYawInput(float value) {
        yawInputSign = value;
    }

    public void SetThrustInput(float value) {
        thrustInputSign = value;
    }

    public void TriggerFlapInput() {
        flapInputWasPressed = true;
    }

    public void TriggerGearInput() {
        gearInputWasPressed = true;
    }

    public void TriggerDecoyInput() {
        decoyInputWasPressed = true;
    }

    public void TriggerAttackInput() {
        attackInputWasPressed = true;
    }


}

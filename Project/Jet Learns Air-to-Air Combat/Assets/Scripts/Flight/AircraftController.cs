using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AircraftController : MonoBehaviour {

    private enum InputType { HORIZONTAL, VERTICAL, YAW, THRUST }

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
    private GameObject gearObject;
    [SerializeField]
    private GameObject closedGearObject;

    private const float gearTimeTransition = 2.5f;
    private const float distanceGearTransition = 2f;
    private const float waitTimeToOpenGear = 0.2f;
    private float gearCurrentTimeTransition = 0.0f;
    private float waitCurrentTimeToOpenGear = 0.0f;
    private float targetYWheelsPosition;
    [SerializeField] private Transform wheelsParentObject;

    [SerializeField]
    private GameObject leftFlameObject;
    [SerializeField]
    private GameObject rightFlameObject;

    [Header("Missile")]
    private bool attackInputWasPressed;
    [SerializeField] private GameObject missileStorageObject;
    private Transform sceneMissileParentObject;
    private List<GameObject> missileArray = new List<GameObject>();

    private Team team;
    private AircraftPhysics aircraftPhysics;
    private SceneData sceneData;
    private Rigidbody rb;

    private float rollInputSign;
    private float pitchInputSign;
    private float yawInputSign;
    private float thrustInputSign;

    private void Start() {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;

        gearInputWasPressed = false;
        gearIsClosed = false;
        gearAnimationIsRunning = false;

        flapInputWasPressed = false;
        attackInputWasPressed = false;

        Material engineFlameMaterial = leftFlameObject.GetComponent<Renderer>().material;
        leftFlameObject.GetComponent<Renderer>().material = new Material(engineFlameMaterial);
        rightFlameObject.GetComponent<Renderer>().material = new Material(engineFlameMaterial);

        PrecalculateAndStoreSufaces();
    }

    private void Update() {
        HandleInputsEvent();
    }

    private void FixedUpdate() {
        SetControlSurfecesAngles();

        aircraftPhysics.SetThrustPercent(thrustInput);
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

    private void HandleInputsEvent() {
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
            valueInputTimePressed += timePressedIncrease * Time.deltaTime;
            valueInputTimePressed = Mathf.Clamp(valueInputTimePressed, 0f, 1f);

            valueInput += valueInputSign * inputsEventIncreaseCurve.Evaluate(valueInputTimePressed) * Time.deltaTime * valueMultiplierIncrease;
        } else if (valueInput != 0f) {
            valueInputTimePressed += timePressedDecrease * Time.deltaTime;

            if (valueInput < 0f) {
                valueInput += inputsEventDecreaseCurve.Evaluate(Mathf.Abs(valueInput)) * Time.deltaTime * valueMultiplierDecrease;

                if (valueInput > 0f) {
                    valueInput = 0f;
                    return;
                }
            } else {
                valueInput -= inputsEventDecreaseCurve.Evaluate(Mathf.Abs(valueInput)) * Time.deltaTime * valueMultiplierDecrease;

                if (valueInput < 0f) {
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
        if (thrustInputSign > 0f) {
            thrustInput += timePressedIncrease * Time.deltaTime;
        } else if (thrustInputSign < 0f) {
            thrustInput -= timePressedDecrease * Time.deltaTime;
        }

        thrustInput = Mathf.Clamp01(thrustInput);
    }

    private void HandleGearInputEvent() {
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
            if (!gearIsClosed) {
                targetYWheelsPosition = wheelsParentObject.localPosition.y + distanceGearTransition;
            } else {
                targetYWheelsPosition = wheelsParentObject.localPosition.y - distanceGearTransition;
            }

            return;
        }

        if (gearAnimationIsRunning) {
            RunGearTransition();

            if (TheaterData.GetResolution() == Resolution.HIGH_RESOLUTION) {
                if (!gearIsClosed && gearAnimator.GetBool("startCloseGearAnimation") && !gearAnimator.GetBool("enterClosedGearState")) {
                    FinishGearTransition();

                    gearAnimator.SetBool("startCloseGearAnimation", false);
                    gearAnimator.SetTrigger("enterClosedGearState");
                    gearAnimator.SetTrigger("idleClosedGearState");

                    gearObject.SetActive(false);
                    closedGearObject.SetActive(true);

                    gearIsClosed = true;
                    gearAnimationIsRunning = false;
                }

                if (gearIsClosed && gearAnimator.GetBool("startOpenGearAnimation") && !gearAnimator.GetBool("enterOpenedGearState")) {
                    FinishGearTransition();

                    gearAnimator.SetBool("startOpenGearAnimation", false);
                    gearAnimator.SetTrigger("enterOpenedGearState");

                    gearIsClosed = false;
                    gearAnimationIsRunning = false;
                }
            } else {
                if (gearCurrentTimeTransition >= gearTimeTransition) {
                    FinishGearTransition();

                    gearIsClosed = !gearIsClosed;
                    gearAnimationIsRunning = false;
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

    // Transitions

    private void RunGearTransition() {
        if (gearCurrentTimeTransition >= gearTimeTransition)
            return;

        float moveWith = distanceGearTransition / gearTimeTransition * Time.deltaTime;
        gearCurrentTimeTransition += Time.deltaTime;

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
                waitCurrentTimeToOpenGear += Time.deltaTime;
                gearCurrentTimeTransition = 0;
            }
        }
    }

    private void FinishGearTransition() {
        wheelsParentObject.localPosition = new Vector3(wheelsParentObject.localPosition.x, targetYWheelsPosition, wheelsParentObject.localPosition.z);
    }

    private void LaunchMissile() {
        missileArray[missileArray.Count - 1].transform.parent = sceneMissileParentObject;
        missileArray[missileArray.Count - 1].AddComponent<Rigidbody>();

        Rigidbody missileRigidbody = missileArray[missileArray.Count - 1].GetComponent<Rigidbody>();
        missileRigidbody.velocity = rb.velocity;
        missileRigidbody.angularVelocity = Vector3.zero;
        missileRigidbody.useGravity = false;

        missileArray[missileArray.Count - 1].GetComponent<MissilePhysics>().LaunchMissile();
        missileArray.RemoveAt(missileArray.Count - 1);
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

    public void AddMissilesInArray() {
        foreach (Transform child in missileStorageObject.transform)
            missileArray.Add(child.gameObject);
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

    public float GetThrustInput() {
        return thrustInputSign;
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
        flapInputWasPressed = !flapInputWasPressed;
    }

    public void TriggerGearInput() {
        gearInputWasPressed = !gearInputWasPressed;
    }

    public void TriggerDecoyInput() {
        decoyInputWasPressed = true;
    }

    public void TriggerAttackInput() {
        attackInputWasPressed = !attackInputWasPressed;
    }


}

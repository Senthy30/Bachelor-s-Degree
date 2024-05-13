using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JetController : MonoBehaviour {

    private static JetController jetController;

    private const float positiveInput = 1.0f;
    private const float zeroInput = 0.0f;
    private const float negativeInput = -1.0f;

    private AircraftController m_currentAircraftController;

    private void Awake() {
        jetController = this;
    }

    // Input Actions --------------------------------------------
    public void PitchDown(InputAction.CallbackContext context) {
        if (context.performed) {
            PitchDownInputDown(m_currentAircraftController);
        } 
        else if (context.canceled) {
            PitchDownInputRelease(m_currentAircraftController);
        }
    }

    public void PitchUp(InputAction.CallbackContext context) {
        if (context.performed) {
            PitchUpInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            PitchUpInputRelease(m_currentAircraftController);
        }
    }

    public void RollLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            RollLeftInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            RollLeftInputRelease(m_currentAircraftController);
        }
    }

    public void RollRight(InputAction.CallbackContext context) {
        if (context.performed) {
            RollRightInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            RollRightInputRelease(m_currentAircraftController);
        }
    }

    public void YawLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            YawLeftInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            YawLeftInputRelease(m_currentAircraftController);
        }
    }

    public void YawRight(InputAction.CallbackContext context) {
        if (context.performed) {
            YawRightInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            YawRightInputRelease(m_currentAircraftController);
        }
    }

    public void ThrustUp(InputAction.CallbackContext context) {
        if (context.performed) {
            ThrustUpInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            ThrustUpInputRelease(m_currentAircraftController);
        }
    }

    public void ThrustDown(InputAction.CallbackContext context) {
        if (context.performed) {
            ThrustDownInputDown(m_currentAircraftController);
        } else if (context.canceled) {
            ThrustDownInputRelease(m_currentAircraftController);
        }
    }

    public void Flap(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerFlapInput(m_currentAircraftController);
        }
    }

    public void Gear(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerGearInput(m_currentAircraftController);
        }
    }

    public void Attack(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerAttackInput(m_currentAircraftController);
        }
    }

    public void Decoy(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerDecoyInput(m_currentAircraftController);
        }
    }

    // Input Methods --------------------------------------------

    // Pitch ----------------------------------------------------
    public void PitchDownInputDown(AircraftController aircraftController) {
        SetPitchInputSign(negativeInput, negativeInput, aircraftController);
    }

    public void PitchDownInputRelease(AircraftController aircraftController) {
        SetPitchInputSign(zeroInput, positiveInput, aircraftController);
    }

    public void PitchUpInputDown(AircraftController aircraftController) {
        SetPitchInputSign(positiveInput, positiveInput, aircraftController);
    }

    public void PitchUpInputRelease(AircraftController aircraftController) {
        SetPitchInputSign(zeroInput, negativeInput, aircraftController);
    }

    private void SetPitchInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetPitchInput() == lastValue)
            return;

        aircraftController.SetPitchInput(value);
    }

    // Roll -----------------------------------------------------
    public void RollLeftInputDown(AircraftController aircraftController) {
        SetRollInputSign(negativeInput, negativeInput, aircraftController);
    }

    public void RollLeftInputRelease(AircraftController aircraftController) {
        SetRollInputSign(zeroInput, positiveInput, aircraftController);
    }

    public void RollRightInputDown(AircraftController aircraftController) {
        SetRollInputSign(positiveInput, positiveInput, aircraftController);
    }

    public void RollRightInputRelease(AircraftController aircraftController) {
        SetRollInputSign(zeroInput, negativeInput, aircraftController);
    }

    private void SetRollInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetRollInput() == lastValue)
            return;

        aircraftController.SetRollInput(value);
    }

    // Yaw ------------------------------------------------------
    public void YawLeftInputDown(AircraftController aircraftController) {
        SetYawInputSign(positiveInput, positiveInput, aircraftController);
    }

    public void YawLeftInputRelease(AircraftController aircraftController) {
        SetYawInputSign(zeroInput, negativeInput, aircraftController);
    }

    public void YawRightInputDown(AircraftController aircraftController) {
        SetYawInputSign(negativeInput, negativeInput, aircraftController);
    }

    public void YawRightInputRelease(AircraftController aircraftController) {
        SetYawInputSign(zeroInput, positiveInput, aircraftController);
    }

    private void SetYawInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetYawInput() == lastValue)
            return;

        aircraftController.SetYawInput(value);
    }

    // Thrust ---------------------------------------------------
    public void ThrustUpInputDown(AircraftController aircraftController) {
        SetThrustInputSign(positiveInput, positiveInput, aircraftController);
    }

    public void ThrustUpInputRelease(AircraftController aircraftController) {
        SetThrustInputSign(zeroInput, negativeInput, aircraftController);
    }

    public void ThrustDownInputDown(AircraftController aircraftController) {
        SetThrustInputSign(negativeInput, negativeInput, aircraftController);
    }

    public void ThrustDownInputRelease(AircraftController aircraftController) {
        SetThrustInputSign(zeroInput, positiveInput, aircraftController);
    }

    private void SetThrustInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetThrustInput() == lastValue)
            return;

        aircraftController.SetThrustInput(value);
    }

    // Flap ----------------------------------------------------
    public void TriggerFlapInput(AircraftController aircraftController) {
        if (aircraftController == null)
            return;

        aircraftController.TriggerFlapInput();
    }

    // Gear ----------------------------------------------------
    public void TriggerGearInput(AircraftController aircraftController) {
        if (aircraftController == null)
            return;

        aircraftController.TriggerGearInput();
    }

    // Attack --------------------------------------------------
    public void TriggerAttackInput(AircraftController aircraftController) {
        if (aircraftController == null)
            return;

        aircraftController.TriggerAttackInput();
    }

    // Decoy ---------------------------------------------------

    public void TriggerDecoyInput(AircraftController aircraftController) {
        if (aircraftController == null)
            return;

        aircraftController.TriggerDecoyInput();
    }

    // Peform --------------------------------------------------

    public static void PerformJetActions(AircraftController aircraftController, JetAction jetAction) {
        // Pitch
        JetAction.PitchAction pitchAction = jetAction.GetPitchAction();
        if (pitchAction == JetAction.PitchAction.Up) {
            jetController.PitchUpInputDown(aircraftController);
        } else if (pitchAction == JetAction.PitchAction.Down) {
            jetController.PitchDownInputDown(aircraftController);
        } else if (pitchAction == JetAction.PitchAction.None) {
            jetController.PitchUpInputRelease(aircraftController);
            jetController.PitchDownInputRelease(aircraftController);
        }

        // Roll
        JetAction.RollAction rollAction = jetAction.GetRollAction();
        if (rollAction == JetAction.RollAction.Left) {
            jetController.RollLeftInputDown(aircraftController);
        } else if (rollAction == JetAction.RollAction.Right) {
            jetController.RollRightInputDown(aircraftController);
        } else if (rollAction == JetAction.RollAction.None) {
            jetController.RollLeftInputRelease(aircraftController);
            jetController.RollRightInputRelease(aircraftController);
        }

        // Yaw
        JetAction.YawAction yawAction = jetAction.GetYawAction();
        if (yawAction == JetAction.YawAction.Left) {
            jetController.YawLeftInputDown(aircraftController);
        } else if (yawAction == JetAction.YawAction.Right) {
            jetController.YawRightInputDown(aircraftController);
        } else if (yawAction == JetAction.YawAction.None) {
            jetController.YawLeftInputRelease(aircraftController);
            jetController.YawRightInputRelease(aircraftController);
        }

        // Thrust
        JetAction.ThrustAction thrustAction = jetAction.GetThrustAction();
        if (thrustAction == JetAction.ThrustAction.Increase) {
            jetController.ThrustUpInputDown(aircraftController);
        } else if (thrustAction == JetAction.ThrustAction.Decrease) {
            jetController.ThrustDownInputDown(aircraftController);
        } else if (thrustAction == JetAction.ThrustAction.None) {
            jetController.ThrustUpInputRelease(aircraftController);
            jetController.ThrustDownInputRelease(aircraftController);
        }

        // Flap
        JetAction.FlapsAction flapsAction = jetAction.GetFlapsAction();
        if (flapsAction == JetAction.FlapsAction.Trigger) {
            jetController.TriggerFlapInput(aircraftController);
        }

        // Launch Missile
        JetAction.LaunchMissileAction launchMissileAction = jetAction.GetLaunchMissileAction();
        if (launchMissileAction == JetAction.LaunchMissileAction.Launch) {
            if (aircraftController.GetJetData().GetNumUnlaunchedMissiles() > 0) {
                jetController.TriggerAttackInput(aircraftController);
            } else {
                jetAction.SetLaunchMissileAction(JetAction.LaunchMissileAction.None);
            }
        }

        // Drop Decoy
        JetAction.DropDecoyAction dropDecoyAction = jetAction.GetDropDecoyAction();
        if (dropDecoyAction == JetAction.DropDecoyAction.Drop) {
            if (aircraftController.GetJetData().GetNumDecoys() > 0) {
                jetController.TriggerDecoyInput(aircraftController);
            } else {
                jetAction.SetDropDecoyAction(JetAction.DropDecoyAction.None);
            }
        }

        aircraftController.HandleInputsEvent(true);
    }

    // Setters --------------------------------------------
    public void SetCurrentAircraftController(AircraftController aircraftController) {
        m_currentAircraftController = aircraftController;
    }

}

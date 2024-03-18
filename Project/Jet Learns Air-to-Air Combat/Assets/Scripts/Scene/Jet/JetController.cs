using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JetController : MonoBehaviour {

    private const float positiveInput = 1.0f;
    private const float zeroInput = 0.0f;
    private const float negativeInput = -1.0f;

    private AircraftController m_currentAircraftController;

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
        aircraftController.TriggerFlapInput();
    }

    // Gear ----------------------------------------------------
    public void TriggerGearInput(AircraftController aircraftController) {
        aircraftController.TriggerGearInput();
    }

    // Attack --------------------------------------------------
    public void TriggerAttackInput(AircraftController aircraftController) {
        aircraftController.TriggerAttackInput();
    }

    // Decoy ---------------------------------------------------

    public void TriggerDecoyInput(AircraftController aircraftController) {
        aircraftController.TriggerDecoyInput();
    }

    // Setters --------------------------------------------
    public void SetCurrentAircraftController(AircraftController aircraftController) {
        m_currentAircraftController = aircraftController;
    }

}

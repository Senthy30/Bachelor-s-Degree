using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JetController : MonoBehaviour {

    private const float positiveInput = 1.0f;
    private const float zeroInput = 0.0f;
    private const float negativeInput = -1.0f;

    private AircraftController currentAircraftController;

    public void PitchDown(InputAction.CallbackContext context) {
        if (context.performed) {
            SetPitchInputSign(negativeInput, negativeInput, currentAircraftController);
        } 
        else if (context.canceled) {
            SetPitchInputSign(zeroInput, positiveInput, currentAircraftController);
        }
    }

    public void PitchUp(InputAction.CallbackContext context) {
        if (context.performed) {
            SetPitchInputSign(positiveInput, positiveInput, currentAircraftController);
        } else if (context.canceled) {
            SetPitchInputSign(zeroInput, negativeInput, currentAircraftController);
        }
    }

    public void RollLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            SetRollInputSign(negativeInput, negativeInput, currentAircraftController);
        } else if (context.canceled) {
            SetRollInputSign(zeroInput, positiveInput, currentAircraftController);
        }
    }

    public void RollRight(InputAction.CallbackContext context) {
        if (context.performed) {
            SetRollInputSign(positiveInput, positiveInput, currentAircraftController);
        } else if (context.canceled) {
            SetRollInputSign(zeroInput, negativeInput, currentAircraftController);
        }
    }

    public void YawLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            SetYawInputSign(positiveInput, positiveInput, currentAircraftController);
        } else if (context.canceled) {
            SetYawInputSign(zeroInput, negativeInput, currentAircraftController);
        }
    }

    public void YawRight(InputAction.CallbackContext context) {
        if (context.performed) {
            SetYawInputSign(negativeInput, negativeInput, currentAircraftController);
        } else if (context.canceled) {
            SetYawInputSign(zeroInput, positiveInput, currentAircraftController);
        }
    }

    public void Flap(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerFlapInput(currentAircraftController);
        }
    }

    public void Gear(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerGearInput(currentAircraftController);
        }
    }

    public void ThrustUp(InputAction.CallbackContext context) {
        if (context.performed) {
            SetThrustInputSign(positiveInput, positiveInput, currentAircraftController);
        } else if (context.canceled) {
            SetThrustInputSign(zeroInput, negativeInput, currentAircraftController);
        }
    }

    public void ThrustDown(InputAction.CallbackContext context) {
        if (context.performed) {
            SetThrustInputSign(negativeInput, negativeInput, currentAircraftController);
        } else if (context.canceled) {
            SetThrustInputSign(zeroInput, positiveInput, currentAircraftController);
        }
    }

    public void Attack(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggetAttackInput(currentAircraftController);
        }
    }

    private void SetPitchInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetPitchInput() == lastValue)
            return;

        aircraftController.SetPitchInput(value);
    }

    private void SetRollInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetRollInput() == lastValue)
            return;

        aircraftController.SetRollInput(value);
    }

    private void SetYawInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetYawInput() == lastValue)
            return;

        aircraftController.SetYawInput(value);
    }

    private void SetThrustInputSign(float value, float lastValue, AircraftController aircraftController) {
        if (aircraftController == null || aircraftController.GetThrustInput() == lastValue)
            return;

        aircraftController.SetThrustInput(value);
    }

    private void TriggerFlapInput(AircraftController aircraftController) { 
        aircraftController.TriggerFlapInput();
    }

    private void TriggerGearInput(AircraftController aircraftController) {
        aircraftController.TriggerGearInput();
    }

    public void TriggetAttackInput(AircraftController aircraftController) {
        aircraftController.TriggerAttackInput();
    }

    // Setters --------------------------------------------

    public void SetCurrentAircraftController(AircraftController aircraftController) {
        currentAircraftController = aircraftController;
    }

}

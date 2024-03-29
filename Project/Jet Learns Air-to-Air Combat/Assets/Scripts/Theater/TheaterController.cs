using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TheaterController : MonoBehaviour {

    private bool m_leftShiftPressed = false;
    [SerializeField] private TheaterData m_theaterData;

    public void ChangeViewMode(InputAction.CallbackContext context) {
        if (context.performed) {
            if (!m_leftShiftPressed) {
                m_theaterData.ChangeViewMode();
            } else {
                m_theaterData.ChangeViewModeToMissile();
            }
        }
    }

    public void ChangeViewModeToMissile(InputAction.CallbackContext context) {
        if (context.performed)
            m_theaterData.ChangeViewModeToMissile();
    }

    public void ChangeToNextWatching(InputAction.CallbackContext context) {
        if (context.performed) {
            if (!m_leftShiftPressed) {
                m_theaterData.ChangeToSceneWatchingAtOffset(1);
            } else {
                m_theaterData.ChangeToMissileWatchingAtOffset(1);
            }
        }
    }

    public void ChangeToPreviousWatching(InputAction.CallbackContext context) {
        if (context.performed) {
            if (!m_leftShiftPressed) {
                m_theaterData.ChangeToSceneWatchingAtOffset(-1);
            } else {
                m_theaterData.ChangeToMissileWatchingAtOffset(-1);
            }
        }
    }

    public void ChangeTeamWatching(InputAction.CallbackContext context) {
        if (context.performed)
            m_theaterData.ChangeTeamWatching();
    }

    public void ShiftPressed(InputAction.CallbackContext context) {
        if (context.performed)
            m_leftShiftPressed = true;
        else if (context.canceled)
            m_leftShiftPressed = false;
    }

}

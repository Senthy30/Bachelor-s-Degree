using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TheaterController : MonoBehaviour {

    [SerializeField] private TheaterData m_theaterData;

    public void ChangeViewMode(InputAction.CallbackContext context) {
        if (context.performed)
            m_theaterData.ChangeViewMode();
    }

    public void ChangeSceneWatching(InputAction.CallbackContext context) {
        if (context.performed)
            m_theaterData.ChangeSceneWatching();
    }

    public void ChangeTeamWatching(InputAction.CallbackContext context) {
        if (context.performed)
            m_theaterData.ChangeTeamWatching();
    }

}

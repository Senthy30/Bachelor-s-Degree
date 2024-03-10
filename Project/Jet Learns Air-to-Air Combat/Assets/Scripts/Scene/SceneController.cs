using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneController {

    private SceneData m_sceneData;
    private GameObject m_sceneDataObject;

    SceneController(GameObject sceneDataObject) {
        m_sceneDataObject = sceneDataObject;
        m_sceneData = m_sceneDataObject.GetComponent<SceneData>();
    }

    public void Sprint(InputAction.CallbackContext context) {
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneScenario {

    // Public methods --------------------------------------------------------
    public static void SetScenario(SceneData sceneData) {
        Scenario scenario = sceneData.GetScenario();

        switch (scenario) {
            case Scenario.TAKE_OFF:
                SetScenarioTakeOff(sceneData);
                break;
            case Scenario.BLUE_DEFENDING:
                SetScenarioDefending(sceneData, Team.BLUE);
                break;
            case Scenario.RED_DEFENDING:
                SetScenarioDefending(sceneData, Team.RED);
                break;
            case Scenario.FLIGHTING:
                SetScenarioFlighting(sceneData);
                break;
            case Scenario.FINAL:
                SetScenarioFinal(sceneData);
                break;
            default:
                Debug.LogError("Invalid scenario");
                break;
        }
    }

    // Private methods -------------------------------------------------------
    // Take-off scenario -----------------------------------------------------
    private static void SetScenarioTakeOff(SceneData sceneData) {
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

        
    }

    // DEFENDING scenario ----------------------------------------------------
    private static void SetScenarioDefending(SceneData sceneData, Team team) {
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

    }

    // FLIGHTING scenario ----------------------------------------------------
    private static void SetScenarioFlighting(SceneData sceneData) {
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

    }

    // Final scenario --------------------------------------------------------
    private static void SetScenarioFinal(SceneData sceneData) {
        // pass
    }

}

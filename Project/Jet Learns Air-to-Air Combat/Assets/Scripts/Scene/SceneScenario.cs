using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneScenario {

    // Public methods --------------------------------------------------------
    public static void SetScenario(SceneData sceneData, Scenario scenario) {
        switch (scenario) {
            case Scenario.TAKE_OFF:
                SetScenarioTakeOff(sceneData);
                break;
            case Scenario.BLUE_CHASING:
                SetScenarioChasing(sceneData, Team.BLUE);
                break;
            case Scenario.RED_CHASING:
                SetScenarioChasing(sceneData, Team.RED);
                break;
            case Scenario.BLUE_ATTACKING:
                SetScenarioAttacking(sceneData, Team.BLUE);
                break;
            case Scenario.RED_ATTACKING:
                SetScenarioAttacking(sceneData, Team.RED);
                break;
            case Scenario.FLIGHTING:
                SetScenarioFlighting(sceneData);
                break;
            default:
                Debug.LogError("Invalid scenario");
                break;
        }
    }

    // Private methods -------------------------------------------------------
    // Take-off scenario -----------------------------------------------------
    private static void SetScenarioTakeOff(SceneData sceneData) {
        // do nothing        
    }

    // DEFENDING scenario ----------------------------------------------------
    private static void SetScenarioChasing(SceneData sceneData, Team team) {
        SceneConfig sceneConfig = sceneData.GetSceneConfig();
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

        EnemyChunksData enemyChunksData = sceneComponents.GetEnemyChunksData();
        enemyChunksData.GenerateJetsChunk(TheaterData.GetNumTeams());
        SetEnemyChunksDataForJets(sceneComponents, enemyChunksData);

        float distance = UnityEngine.Random.Range(sceneConfig.minMaxDistanceJetsOnSpawn.x, sceneConfig.minMaxDistanceJetsOnSpawn.y);
        Team chasedTeam = JetAgent.GetTargetTeam(team);

        GameObject chasingJetObject = sceneComponents.GetJetData(team).GetObject();
        GameObject chasedJetObject = sceneComponents.GetJetData(chasedTeam).GetObject();

        chasingJetObject.transform.LookAt(chasedJetObject.transform.position);
        chasingJetObject.transform.localPosition = chasedJetObject.transform.localPosition - distance * chasingJetObject.transform.forward;

        float zRotation = UnityEngine.Random.Range(-180f, 180f);
        chasingJetObject.transform.Rotate(0, 0, zRotation);

        SetRandomJetsAttributes(sceneComponents, sceneConfig);
    }

    private static void SetScenarioAttacking(SceneData sceneData, Team team) {
        SceneConfig sceneConfig = sceneData.GetSceneConfig();
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

        EnemyChunksData enemyChunksData = sceneComponents.GetEnemyChunksData();

        bool foundValidPosition = false;
        float minDistanceAllowed = (sceneConfig.minMaxDistanceJetsOnSpawn.x + sceneConfig.minMaxDistanceJetsOnSpawn.y) / 2.0f;
        Vector3 defenderPosition = Vector3.zero;
        Vector3 attackerPosition = Vector3.zero;
        Quaternion defenderRotation = Quaternion.identity;

        while (!foundValidPosition) {
            defenderPosition = enemyChunksData.GetRandomPositionWithinScene();
            defenderRotation = enemyChunksData.GetRandomRotation();

            Vector3 defenderBack = defenderRotation * Vector3.back;
            Vector3 minPositionAllowed = defenderPosition + minDistanceAllowed * defenderBack;

            foundValidPosition = enemyChunksData.IsPositionWithinAChunk(minPositionAllowed);
            if (foundValidPosition) {
                bool repeat = true;
                float maxDistanceAllowed = sceneConfig.minMaxDistanceJetsOnSpawn.y;
                while (repeat) {
                    float distance = UnityEngine.Random.Range(sceneConfig.minMaxDistanceJetsOnSpawn.x, maxDistanceAllowed);
                    attackerPosition = defenderPosition + distance * defenderBack;
                    if (enemyChunksData.IsPositionWithinAChunk(attackerPosition)) {
                        repeat = false;
                    } else {
                        maxDistanceAllowed = distance;
                    }
                }
            }
        }

        Team defendingTeam = JetAgent.GetTargetTeam(team);
        GameObject defendingJetObject = sceneComponents.GetJetData(defendingTeam).GetObject();
        GameObject attackingJetObject = sceneComponents.GetJetData(team).GetObject();

        defendingJetObject.transform.localPosition = defenderPosition;
        defendingJetObject.transform.rotation = defenderRotation;

        attackingJetObject.transform.localPosition = attackerPosition;
        attackingJetObject.transform.LookAt(defendingJetObject.transform.position);

        float zRotation = UnityEngine.Random.Range(-180f, 180f);
        attackingJetObject.transform.Rotate(0, 0, zRotation);

        SetRandomJetsAttributes(sceneComponents, sceneConfig);
    }

    // FLIGHTING scenario ----------------------------------------------------
    private static void SetScenarioFlighting(SceneData sceneData) {
        SceneConfig sceneConfig = sceneData.GetSceneConfig();
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

        EnemyChunksData enemyChunksData = sceneComponents.GetEnemyChunksData();
        enemyChunksData.GenerateJetsChunk(TheaterData.GetNumTeams());
        SetEnemyChunksDataForJets(sceneComponents, enemyChunksData);
        SetRandomJetsAttributes(sceneComponents, sceneConfig);
    }

    // General
    private static void SetEnemyChunksDataForJets(SceneComponents sceneComponents, EnemyChunksData enemyChunksData) {
        foreach (Team team in System.Enum.GetValues(typeof(Team))) {
            GameObject jetObject = sceneComponents.GetJetData(team).GetObject();
            EnemyChunksData.ComponentInChunkData dataObject = enemyChunksData.GetJetData(team);

            jetObject.transform.localPosition = dataObject.position;
            jetObject.transform.rotation = dataObject.rotation;
        }
    }

    private static void SetRandomJetsAttributes(SceneComponents sceneComponents, SceneConfig sceneConfig) {
        foreach (Team team in System.Enum.GetValues(typeof(Team))) {
            JetData jetData = sceneComponents.GetJetData(team);
            GameObject jetObject = jetData.GetObject();

            float speed = UnityEngine.Random.Range(sceneConfig.minMaxSpeedJetOnSpawn.x, sceneConfig.minMaxSpeedJetOnSpawn.y);
            jetObject.GetComponent<Rigidbody>().velocity = jetObject.transform.forward * speed;

            float thrust = UnityEngine.Random.Range(0.75f, 1.0f);
            jetObject.GetComponent<AircraftController>().SetThrustInputForScenario(thrust);
            jetObject.GetComponent<AircraftController>().SetGearClosed();

            int numMissiles = UnityEngine.Random.Range(2, sceneConfig.numMissilesPerJet);
            jetData.SetNumUnlaunchedMissiles(numMissiles);

            int numDecoys = UnityEngine.Random.Range(1, sceneConfig.numDecoysPerJet);
            jetData.SetNumDecoys(numDecoys);
        }
    }

}

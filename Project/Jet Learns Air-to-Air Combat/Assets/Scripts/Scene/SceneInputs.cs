using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneInputs {
    
    // Public methods ---------------------------------------------------------
    public static JetInputs RequestJetInputs(SceneData sceneData, Team team) {
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

        JetData ownshipJetData = sceneComponents.GetJetData(team);
        JetInputs jetInputs = new JetInputs();

        GameObject ownshipJetObject = ownshipJetData.GetObject();
        Transform ownshipJetTransform = ownshipJetObject.transform;
        AircraftController ownshipAircraftPhysics = ownshipJetObject.GetComponent<AircraftController>();

        JetData targetJetData = sceneComponents.GetJetData(GetTargetTeam(team));

        GameObject targetJetObject = targetJetData.GetObject();
        Transform targetJetTransform = targetJetObject.transform;

        // Ownship
        // int
        jetInputs.decoys = GetOwnshipDecoys(ownshipJetData);
        jetInputs.missiles = GetOwnshipMissiles(ownshipJetData);

        // float
        jetInputs.pitch = GetOwnshipPitch(ownshipAircraftPhysics);
        jetInputs.roll = GetOwnshipRoll(ownshipAircraftPhysics);
        jetInputs.yaw = GetOwnshipYaw(ownshipAircraftPhysics);
        jetInputs.throttle = GetOwnshipThrottle(ownshipAircraftPhysics);

        // Vector3
        jetInputs.position = GetOwnshipPosition(ownshipJetTransform);
        jetInputs.velocity = GetOwnshipVelocity(ownshipJetObject);
        jetInputs.acceleration = GetOwnshipAcceleration(ownshipAircraftPhysics);
        jetInputs.rotation = GetOwnshipRotation(ownshipJetTransform);

        // Target
        // int
        jetInputs.decoysTarget = GetTargetDecoys(targetJetData);
        jetInputs.missilesTarget = GetTargetMissiles(targetJetData);

        // float
        jetInputs.aspectAngle = GetAspectAngle(ownshipJetTransform, targetJetTransform);
        jetInputs.antennaTrainAngle = GetAntennaTrainAngle(ownshipJetTransform, targetJetTransform);
        jetInputs.headingCrossingAngle = GetHeadingCrossingAngle(ownshipJetTransform, targetJetTransform);
        jetInputs.relativeAltitude = GetTargetRelativeAltitude(ownshipJetTransform, targetJetTransform);
        jetInputs.relativeDistance = GetTargetRelativeDistance(ownshipJetTransform, targetJetTransform);

        // Vector3
        jetInputs.relativeVelocity = GetTargetRelativeVelocity(ownshipJetObject, targetJetObject);
        jetInputs.relativeAcceleration = GetTargetRelativeAcceleration(ownshipAircraftPhysics, targetJetObject.GetComponent<AircraftController>());

        // Decoys
        // Vector3
        jetInputs.inRangeDecoyRelativePosition = GetInRangeDecoyRelativePosition(sceneComponents, ownshipJetTransform, sceneData.GetMaxDecoyViewAngle());

        // Missiles
        // float
        jetInputs.launchedMissileDistanceToTarget = GetLaunchedMissileDistanceToTarget(sceneComponents, targetJetObject);
        jetInputs.incomigMissileDistanceToOwnship = GetIncomingMissileDistanceToOwnship(sceneComponents, ownshipJetObject);

        // Vector3
        jetInputs.incomingMissileRelativeDirection = GetIncomingMissileRelativeDirection(sceneComponents, ownshipJetObject);

        return jetInputs;
    }

    // Private methods --------------------------------------------------------
    private static Team GetTargetTeam(Team ownshipTeam) {
        if (ownshipTeam == Team.BLUE) {
            return Team.RED;
        } else {
            return Team.BLUE;
        }
    }

    // Ownship ----------------------------------------------------------------
    private static int GetOwnshipDecoys(JetData jetData) {
        return jetData.GetNumDecoys();
    }

    private static int GetOwnshipMissiles(JetData jetData) {
        return jetData.GetNumUnlaunchedMissiles();
    }

    private static float GetOwnshipPitch(AircraftController aircraftController) {
        return aircraftController.pitchInput;
    }

    private static float GetOwnshipRoll(AircraftController aircraftController) {
        return aircraftController.rollInput;
    }

    private static float GetOwnshipYaw(AircraftController aircraftController) {
        return aircraftController.yawInput;
    }

    private static float GetOwnshipThrottle(AircraftController aircraftController) {
        return aircraftController.thrustInput;
    }

    private static Vector3 GetOwnshipPosition(Transform ownshipJetTransform) {
        return ownshipJetTransform.localPosition;
    }

    private static Vector3 GetOwnshipVelocity(GameObject ownshipJetObject) {
        return ownshipJetObject.GetComponent<Rigidbody>().velocity;
    }

    private static Vector3 GetOwnshipAcceleration(AircraftController aircraftController) {
        return aircraftController.acceleration;
    }

    private static Vector3 GetOwnshipRotation(Transform ownshipJetTransform) {
        return ownshipJetTransform.localEulerAngles;
    }

    // Target -----------------------------------------------------------------
    private static int GetTargetDecoys(JetData jetData) {
        return jetData.GetNumDecoys();
    }

    private static int GetTargetMissiles(JetData jetData) {
        return jetData.GetNumUnlaunchedMissiles();
    }

    private static float GetAspectAngle(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 v = targetJetTransform.localPosition - ownshipJetTransform.localPosition;
        Vector3 u = -targetJetTransform.forward;

        return Vector3.Angle(u, v);
    }

    private static float GetAntennaTrainAngle(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 v = ownshipJetTransform.localPosition - targetJetTransform.localPosition;
        Vector3 u = ownshipJetTransform.forward;

        return Vector3.Angle(u, v);
    }

    private static float GetHeadingCrossingAngle(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 v = targetJetTransform.forward;
        Vector3 u = ownshipJetTransform.forward;

        return Vector3.Angle(u, v);
    }

    public static float GetTargetRelativeAltitude(Transform ownshipJetTransform, Transform targetJetTransform) {
        return targetJetTransform.localPosition.y - ownshipJetTransform.localPosition.y;
    }

    private static float GetTargetRelativeDistance(Transform ownshipJetTransform, Transform targetJetTransform) {
        return Vector3.Distance(ownshipJetTransform.localPosition, targetJetTransform.localPosition);
    }

    private static Vector3 GetTargetRelativeVelocity(GameObject ownshipJetObject, GameObject targetJetObject) {
        Vector3 ownshipVelocity = ownshipJetObject.GetComponent<Rigidbody>().velocity;
        Vector3 targetVelocity = targetJetObject.GetComponent<Rigidbody>().velocity;

        return ownshipJetObject.transform.InverseTransformDirection(targetVelocity - ownshipVelocity);// - ownshipJetObject.transform.InverseTransformDirection(targetVelocity);
    }

    private static Vector3 GetTargetRelativeAcceleration(AircraftController ownshipAircraftPhysics, AircraftController targetAircraftPhysics) {
        Vector3 ownshipAcceleration = ownshipAircraftPhysics.acceleration;
        Vector3 targetAcceleration = targetAircraftPhysics.acceleration;

        return ownshipAircraftPhysics.transform.InverseTransformDirection(targetAcceleration - ownshipAcceleration);
    }

    // Decoys -----------------------------------------------------------------
    private static Vector3 GetInRangeDecoyRelativePosition(SceneComponents sceneComponents, Transform ownshipJetTransform, float maxViewAngle) {
        float minDistanceToDecoy = float.MaxValue;
        Vector3 closestDecoyPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        List <DecoyData> dropedDecoysData = sceneComponents.GetDropedDecoyData();

        for (int index = 0; index < dropedDecoysData.Count; index++) {
            Vector3 decoyPosition = dropedDecoysData[index].GetObject().transform.position;
            Vector3 transformDecoyVector = decoyPosition - ownshipJetTransform.position;
            
            float angle = Vector3.Angle(ownshipJetTransform.forward, transformDecoyVector);
            float distance = Vector3.Distance(ownshipJetTransform.position, decoyPosition);

            if (angle <= maxViewAngle && distance < minDistanceToDecoy) {
                closestDecoyPosition = ownshipJetTransform.InverseTransformPoint(decoyPosition);
                minDistanceToDecoy = distance;
            }
        }

        return closestDecoyPosition;
    }

    // Missiles ---------------------------------------------------------------
    private static float GetLaunchedMissileDistanceToTarget(SceneComponents sceneComponents, GameObject targetJetObject) {
        float minDistanceToMissile = float.MaxValue;
        List<MissileData> launchedMissilesData = sceneComponents.GetLaunchedMissilesData();

        for (int index = 0; index < launchedMissilesData.Count; index++) {
            MissilePhysics missilePhysics = launchedMissilesData[index].GetMissilePhysics();
            if (missilePhysics.GetTarget() != targetJetObject)
                continue;

            float distance = Vector3.Distance(targetJetObject.transform.position, missilePhysics.transform.position);
            if (distance < minDistanceToMissile) {
                minDistanceToMissile = distance;
            }
        }

        return minDistanceToMissile;
    }

    private static float GetIncomingMissileDistanceToOwnship(SceneComponents sceneComponents, GameObject ownshipJetObject) {
        float minDistanceToMissile = float.MaxValue;
        List<MissileData> launchedMissilesData = sceneComponents.GetLaunchedMissilesData();

        for (int index = 0; index < launchedMissilesData.Count; index++) {
            MissilePhysics missilePhysics = launchedMissilesData[index].GetMissilePhysics();
            if (missilePhysics.GetTarget() != ownshipJetObject)
                continue;

            float distance = Vector3.Distance(ownshipJetObject.transform.position, missilePhysics.transform.position);
            if (distance < minDistanceToMissile) {
                minDistanceToMissile = distance;
            }
        }

        return minDistanceToMissile;
    }

    private static Vector3 GetIncomingMissileRelativeDirection(SceneComponents sceneComponents, GameObject ownshipJetObject) {
        float minDistanceToMissile = float.MaxValue;
        Vector3 closestIncomingMissileDirection = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        List<MissileData> launchedMissilesData = sceneComponents.GetLaunchedMissilesData();
        
        for (int index = 0; index < launchedMissilesData.Count; index++) {
            MissilePhysics missilePhysics = launchedMissilesData[index].GetMissilePhysics();
            if (missilePhysics.GetTarget() != ownshipJetObject)
                continue;

            Vector3 missilePosition = missilePhysics.transform.position;
            float distance = Vector3.Distance(ownshipJetObject.transform.position, missilePosition);

            if (distance < minDistanceToMissile) {
                Vector3 ownshipMissileVector = missilePosition - ownshipJetObject.transform.position;

                closestIncomingMissileDirection = ownshipJetObject.transform.InverseTransformDirection(ownshipMissileVector).normalized;
                minDistanceToMissile = distance;
            }
        }

        return closestIncomingMissileDirection;
    }
}



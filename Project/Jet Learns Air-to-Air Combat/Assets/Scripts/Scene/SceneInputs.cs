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
        // bool
        jetInputs.areFlapsActive = GetOwnshipFlapsActive(ownshipAircraftPhysics);
        jetInputs.areLandingGearActive = GetOwnshipLandingGearActive(ownshipAircraftPhysics);

        // int
        jetInputs.decoys = GetOwnshipDecoys(ownshipJetData);
        jetInputs.missiles = GetOwnshipMissiles(ownshipJetData);

        // float
        jetInputs.pitch = GetOwnshipPitch(ownshipAircraftPhysics);
        jetInputs.roll = GetOwnshipRoll(ownshipAircraftPhysics);
        jetInputs.yaw = GetOwnshipYaw(ownshipAircraftPhysics);
        jetInputs.flap = GetOwnshipFlap(ownshipAircraftPhysics);
        jetInputs.throttle = GetOwnshipThrottle(ownshipAircraftPhysics);

        // Vector3
        jetInputs.position = GetOwnshipPosition(ownshipJetTransform);
        jetInputs.velocity = GetOwnshipVelocity(ownshipJetObject);
        jetInputs.angularVelocity = GetOwnshipAngularVelocity(ownshipJetObject);
        jetInputs.acceleration = GetOwnshipAcceleration(ownshipAircraftPhysics);
        jetInputs.angularAcceleration = GetOwnshipAngularAcceleration(ownshipAircraftPhysics);
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
        jetInputs.relativeAngularVelocity = GetTargetRelativeAngularVelocity(ownshipJetObject, targetJetObject);
        jetInputs.relativeAcceleration = GetTargetRelativeAcceleration(ownshipAircraftPhysics, targetJetObject.GetComponent<AircraftController>());
        jetInputs.relativeAngularAcceleration = GetTargetRelativeAngularAcceleration(ownshipAircraftPhysics, targetJetObject.GetComponent<AircraftController>());
        jetInputs.relativeDirection = GetTargetRelativeDirection(ownshipJetTransform, targetJetTransform);
        jetInputs.relativeRotation = GetTargetRelativeRotation(ownshipJetTransform, targetJetTransform);

        // Decoys
        CreateFakeDecoyData(sceneData, ownshipJetTransform);

        bool isDecoyInRange = false;
        DecoyData decoyData = GetClosestDecoyData(sceneComponents, ownshipJetTransform, sceneData.GetMaxDecoyViewAngle(), ref isDecoyInRange);
        if (decoyData == null) {
            jetInputs.isDecoyInRange = false;
            jetInputs.inRangeDecoyDistance = 0;
            jetInputs.inRangeDecoyAngle = 0;
            jetInputs.inRangeDecoyRelativeDirection = Vector3.zero;
        } else {
            jetInputs.isDecoyInRange = isDecoyInRange;
            jetInputs.inRangeDecoyDistance = GetInRangeDecoyDistance(ownshipJetTransform, decoyData);
            jetInputs.inRangeDecoyAngle = GetInRangeDecoyAngle(ownshipJetTransform, decoyData);
            jetInputs.inRangeDecoyRelativeDirection = GetInRangeDecoyRelativeDirection(ownshipJetTransform, decoyData);
        }

        // Missiles
        CreateFakeMissileData(sceneData, ownshipJetTransform);

        MissileData closestMissileDataToOwnship = GetClosestMissile(sceneComponents, ownshipJetObject);
        MissileData closestMissileDataToTarget = GetClosestMissile(sceneComponents, targetJetObject);

        if (closestMissileDataToOwnship == null) {
            jetInputs.hasIncomingMissileInRangeOwnship = false;
            jetInputs.incomigMissileDistanceToOwnship = 0;
            jetInputs.incomingMissileAngleToOwnship = 0;
            jetInputs.incomingMissileRelativeDirection = Vector3.zero;
            jetInputs.incomingMissileRelativeVelocity = Vector3.zero;
            jetInputs.incomingMissileRelativeAcceleration = Vector3.zero;
        } else {
            jetInputs.hasIncomingMissileInRangeOwnship = true;
            jetInputs.incomigMissileDistanceToOwnship = GetIncomingMissileDistanceToOwnship(ownshipJetTransform, closestMissileDataToOwnship);
            jetInputs.incomingMissileAngleToOwnship = GetIncomingMissileAngleToOwnship(ownshipJetTransform, closestMissileDataToOwnship);
            jetInputs.incomingMissileRelativeDirection = GetIncomingMissileRelativeDirection(ownshipJetTransform, closestMissileDataToOwnship);
            jetInputs.incomingMissileRelativeVelocity = GetIncomingMissileRelativeVelocity(ownshipJetTransform, closestMissileDataToOwnship);
            jetInputs.incomingMissileRelativeAcceleration = GetIncomingMissileRelativeAcceleration(ownshipJetTransform, closestMissileDataToOwnship);
        }

        if (closestMissileDataToTarget == null) {
            jetInputs.hasLaunchedMissileInRangeTarget = false;
            jetInputs.launchedMissileDistanceToTarget = 0;
        } else {
            jetInputs.hasLaunchedMissileInRangeTarget = true;
            jetInputs.launchedMissileDistanceToTarget = GetLaunchedMissileDistanceToTarget(targetJetTransform, closestMissileDataToTarget);
        }
        
        NormalizedJetInputs(sceneData.GetSceneConfig(), jetInputs);

        ownshipJetData.SetLastJetInputs(ownshipJetData.GetJetInputs());
        ownshipJetData.SetJetInputs(jetInputs);

        return jetInputs;
    }

    // Private methods --------------------------------------------------------
    // Ownship ----------------------------------------------------------------

    private static bool GetOwnshipFlapsActive(AircraftController ownshipAircraftController) {
        float value = ownshipAircraftController.GetFlapInputSign();
        if (value <= 1e-6) {
            return false;
        } else {
            return true;
        }
    }

    private static bool GetOwnshipLandingGearActive(AircraftController ownshipAircraftController) {
        return ownshipAircraftController.GetGearIsClosed();
    }

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

    private static float GetOwnshipFlap(AircraftController aircraftController) {
        return aircraftController.flapInput;
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

    private static Vector3 GetOwnshipAngularVelocity(GameObject ownshipJetObject) {
        return ownshipJetObject.GetComponent<Rigidbody>().angularVelocity;
    }

    private static Vector3 GetOwnshipAcceleration(AircraftController aircraftController) {
        return aircraftController.acceleration;
    }

    private static Vector3 GetOwnshipAngularAcceleration(AircraftController aircraftController) {
        return aircraftController.angularAcceleration;
    }

    private static Quaternion GetOwnshipRotation(Transform ownshipJetTransform) {
        return ownshipJetTransform.rotation;
    }

    // Target -----------------------------------------------------------------
    private static int GetTargetDecoys(JetData jetData) {
        return jetData.GetNumDecoys();
    }

    private static int GetTargetMissiles(JetData jetData) {
        return jetData.GetNumUnlaunchedMissiles();
    }

    private static float GetAspectAngle(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 v = targetJetTransform.position - ownshipJetTransform.position;
        Vector3 u = -targetJetTransform.forward;

        return Vector3.Angle(u, v);
    }

    private static float GetAntennaTrainAngle(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 v = ownshipJetTransform.position - targetJetTransform.position;
        Vector3 u = ownshipJetTransform.forward;

        return Vector3.Angle(u, v);
    }

    private static float GetHeadingCrossingAngle(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 v = targetJetTransform.forward;
        Vector3 u = ownshipJetTransform.forward;

        return Vector3.Angle(u, v);
    }

    public static float GetTargetRelativeAltitude(Transform ownshipJetTransform, Transform targetJetTransform) {
        return targetJetTransform.position.y - ownshipJetTransform.position.y;
    }

    private static float GetTargetRelativeDistance(Transform ownshipJetTransform, Transform targetJetTransform) {
        return Vector3.Distance(ownshipJetTransform.position, targetJetTransform.position);
    }

    private static Vector3 GetTargetRelativeVelocity(GameObject ownshipJetObject, GameObject targetJetObject) {
        Vector3 ownshipVelocity = ownshipJetObject.GetComponent<Rigidbody>().velocity;
        Vector3 targetVelocity = targetJetObject.GetComponent<Rigidbody>().velocity;

        return ownshipJetObject.transform.InverseTransformDirection(targetVelocity - ownshipVelocity);// - ownshipJetObject.transform.InverseTransformDirection(targetVelocity);
    }

    private static Vector3 GetTargetRelativeAngularVelocity(GameObject ownshipJetObject, GameObject targetJetObject) {
        Vector3 ownshipAngularVelocity = ownshipJetObject.GetComponent<Rigidbody>().angularVelocity;
        Vector3 targetAngularVelocity = targetJetObject.GetComponent<Rigidbody>().angularVelocity;

        return ownshipJetObject.transform.InverseTransformDirection(targetAngularVelocity - ownshipAngularVelocity);
    }

    private static Vector3 GetTargetRelativeAcceleration(AircraftController ownshipAircraftPhysics, AircraftController targetAircraftPhysics) {
        Vector3 ownshipAcceleration = ownshipAircraftPhysics.acceleration;
        Vector3 targetAcceleration = targetAircraftPhysics.acceleration;

        return ownshipAircraftPhysics.transform.InverseTransformDirection(targetAcceleration - ownshipAcceleration);
    }

    private static Vector3 GetTargetRelativeAngularAcceleration(AircraftController ownshipAircraftPhysics, AircraftController targetAircraftPhysics) {
        Vector3 ownshipAngularAcceleration = ownshipAircraftPhysics.angularAcceleration;
        Vector3 targetAngularAcceleration = targetAircraftPhysics.angularAcceleration;

        return ownshipAircraftPhysics.transform.InverseTransformDirection(targetAngularAcceleration - ownshipAngularAcceleration);
    }

    private static Vector3 GetTargetRelativeDirection(Transform ownshipJetTransform, Transform targetJetTransform) {
        Vector3 targetPosition = targetJetTransform.position;
        Vector3 ownshipPosition = ownshipJetTransform.position;

        return ownshipJetTransform.InverseTransformDirection(targetPosition - ownshipPosition).normalized;
    }

    private static Quaternion GetTargetRelativeRotation(Transform ownshipJetTransform, Transform targetJetTransform) {
        Quaternion targetRotation = targetJetTransform.rotation;
        Quaternion ownshipRotation = ownshipJetTransform.rotation;

        Quaternion targetOwnshipRotation = ownshipRotation * Quaternion.Inverse(targetRotation);

        return targetOwnshipRotation;
    }

    // Decoys -----------------------------------------------------------------
    private static DecoyData GetClosestDecoyData(SceneComponents sceneComponents, Transform ownshipJetTransform, float maxViewAngle, ref bool isInRange) {
        isInRange = false;
        float minAngleToDecoy = float.MaxValue;
        DecoyData closestDecoyData = null;
        List<DecoyData> dropedDecoysData = sceneComponents.GetDropedDecoyData();

        for (int index = 0; index < dropedDecoysData.Count; index++) {
            if (dropedDecoysData[index].GetObject() == null)
                continue;

            Vector3 decoyPosition = dropedDecoysData[index].GetObject().transform.position;
            Vector3 transformDecoyVector = decoyPosition - ownshipJetTransform.position;

            float angle = Vector3.Angle(ownshipJetTransform.forward, transformDecoyVector);

            if (angle < minAngleToDecoy) {
                closestDecoyData = dropedDecoysData[index];
                minAngleToDecoy = angle;
                if (angle <= maxViewAngle)
                    isInRange = true;
            }
        }

        return closestDecoyData;
    }

    private static float GetInRangeDecoyDistance(Transform ownshipJetTransform, DecoyData decoyData) {
        return Vector3.Distance(ownshipJetTransform.position, decoyData.GetObject().transform.position);
    }

    private static float GetInRangeDecoyAngle(Transform ownshipJetTransform, DecoyData decoyData) {
        Vector3 decoyPosition = decoyData.GetObject().transform.position;
        Vector3 transformDecoyVector = decoyPosition - ownshipJetTransform.position;

        return Vector3.Angle(ownshipJetTransform.forward, transformDecoyVector);
    }

    private static Vector3 GetInRangeDecoyRelativeDirection(Transform ownshipJetTransform, DecoyData decoyData) {
        Vector3 decoyPosition = decoyData.GetObject().transform.position;
        Vector3 transformDecoyVector = decoyPosition - ownshipJetTransform.position;

        return ownshipJetTransform.InverseTransformDirection(transformDecoyVector).normalized;
    }

    // Missiles ---------------------------------------------------------------
    private static MissileData GetClosestMissile(SceneComponents sceneComponents, GameObject jetObject) {
        float minDistanceToMissile = float.MaxValue;
        MissileData closestMissileData = null;
        List<MissileData> launchedMissilesData = sceneComponents.GetLaunchedMissilesData();

        for (int index = 0; index < launchedMissilesData.Count; index++) {
            MissilePhysics missilePhysics = launchedMissilesData[index].GetMissilePhysics();
            if (missilePhysics.GetTarget() != jetObject)
                continue;

            float distance = Vector3.Distance(jetObject.transform.position, missilePhysics.transform.position);
            if (distance < minDistanceToMissile) {
                minDistanceToMissile = distance;
                closestMissileData = launchedMissilesData[index];
            }
        }

        return closestMissileData;
    }

    private static float GetLaunchedMissileDistanceToTarget(Transform targetJetTransform, MissileData missileData) {
        return Vector3.Distance(targetJetTransform.position, missileData.GetMissilePhysics().transform.position);
    }

    private static float GetIncomingMissileDistanceToOwnship(Transform ownshipJetTransform, MissileData missileData) {
        return Vector3.Distance(ownshipJetTransform.position, missileData.GetMissilePhysics().transform.position);
    }

    private static float GetIncomingMissileAngleToOwnship(Transform ownshipJetTransform, MissileData missileData) {
        Vector3 ownshipMissileVector = missileData.GetMissilePhysics().transform.position - ownshipJetTransform.position;

        return Vector3.Angle(ownshipJetTransform.forward, ownshipMissileVector);
    }

    private static Vector3 GetIncomingMissileRelativeDirection(Transform ownshipJetTransform, MissileData missileData) {
        Vector3 ownshipMissileVector = missileData.GetMissilePhysics().transform.position - ownshipJetTransform.position;

        return ownshipJetTransform.InverseTransformDirection(ownshipMissileVector).normalized;
    }

    private static Vector3 GetIncomingMissileRelativeVelocity(Transform ownshipJetTransform, MissileData missileData) {
        Vector3 ownshipVelocity = ownshipJetTransform.GetComponent<Rigidbody>().velocity;
        Vector3 missileVelocity = missileData.GetMissilePhysics().GetVelocity();

        return ownshipJetTransform.InverseTransformDirection(missileVelocity - ownshipVelocity);
    }

    private static Vector3 GetIncomingMissileRelativeAcceleration(Transform ownshipJetTransform, MissileData missileData) {
        Vector3 ownshipAcceleration = ownshipJetTransform.GetComponent<AircraftController>().acceleration;
        Vector3 missileAcceleration = missileData.GetMissilePhysics().GetAcceleration();

        return ownshipJetTransform.InverseTransformDirection(missileAcceleration - ownshipAcceleration);
    }

    // General ----------------------------------------------------------------
    private static Team GetTargetTeam(Team ownshipTeam) {
        if (ownshipTeam == Team.BLUE) {
            return Team.RED;
        } else {
            return Team.BLUE;
        }
    }

    #region Normalization

    private static void NormalizedJetInputs(SceneConfig sceneConfig, JetInputs jetInputs) {
        // Ownship
        jetInputs.decoys = NormalizeDecoys(sceneConfig, jetInputs.decoys);
        jetInputs.missiles = NormalizeMissiles(sceneConfig, jetInputs.missiles);

        jetInputs.position = NormalizePosition(sceneConfig, jetInputs.position);
        jetInputs.velocity = NormalizeVelocity(sceneConfig, jetInputs.velocity);
        jetInputs.angularVelocity = NormalizeAngularVelocity(sceneConfig, jetInputs.angularVelocity);
        jetInputs.acceleration = NormalizeAcceleration(sceneConfig, jetInputs.acceleration);
        jetInputs.angularAcceleration = NormalizeAngularAcceleration(sceneConfig, jetInputs.angularAcceleration);

        // Target
        jetInputs.decoysTarget = NormalizeDecoys(sceneConfig, jetInputs.decoysTarget);
        jetInputs.missilesTarget = NormalizeMissiles(sceneConfig, jetInputs.missilesTarget);

        jetInputs.aspectAngle = NormalizeAngle(jetInputs.aspectAngle);
        jetInputs.antennaTrainAngle = NormalizeAngle(jetInputs.antennaTrainAngle);
        jetInputs.headingCrossingAngle = NormalizeAngle(jetInputs.headingCrossingAngle);
        jetInputs.relativeAltitude = NormalizeAltitude(sceneConfig, jetInputs.relativeAltitude);
        jetInputs.relativeDistance = NormalizeDistance(sceneConfig, jetInputs.relativeDistance);

        jetInputs.relativeVelocity = NormalizeVelocity(sceneConfig, jetInputs.relativeVelocity);
        jetInputs.relativeAngularVelocity = NormalizeAngularVelocity(sceneConfig, jetInputs.relativeAngularVelocity);
        jetInputs.relativeAcceleration = NormalizeAcceleration(sceneConfig, jetInputs.relativeAcceleration);
        jetInputs.relativeAngularAcceleration = NormalizeAngularAcceleration(sceneConfig, jetInputs.relativeAngularAcceleration);

        // Decoys
        jetInputs.inRangeDecoyDistance = NormalizeDistance(sceneConfig, jetInputs.inRangeDecoyDistance);
        jetInputs.inRangeDecoyAngle = NormalizeAngle(jetInputs.inRangeDecoyAngle);

        // Missile
        jetInputs.launchedMissileDistanceToTarget = NormalizeDistance(sceneConfig, jetInputs.launchedMissileDistanceToTarget);
        jetInputs.incomigMissileDistanceToOwnship = NormalizeDistance(sceneConfig, jetInputs.incomigMissileDistanceToOwnship);
        jetInputs.incomingMissileAngleToOwnship = NormalizeAngle(jetInputs.incomingMissileAngleToOwnship);

        jetInputs.incomingMissileRelativeVelocity = NormalizeVelocity(sceneConfig, jetInputs.incomingMissileRelativeVelocity);
        jetInputs.incomingMissileRelativeAcceleration = NormalizeAcceleration(sceneConfig, jetInputs.incomingMissileRelativeAcceleration);
    }

    private static float NormalizeDecoys(SceneConfig sceneConfig, float decoys) {
        return decoys / sceneConfig.numDecoysPerJet;
    }

    private static float NormalizeMissiles(SceneConfig sceneConfig, float missiles) {
        return missiles / sceneConfig.numMissilesPerJet;
    }

    private static float NormalizeAngle(float angle) {
        return angle / 180f;
    }

    private static float NormalizeAltitude(SceneConfig sceneConfig, float altitude) {
        return altitude / (sceneConfig.sceneSize.y * 10);
    }

    private static float NormalizeDistance(SceneConfig sceneConfig, float distance) {
        return distance / 7145f; // precalculated max distance
    }

    private static Vector3 NormalizePosition(SceneConfig sceneConfig, Vector3 position) {
        return new Vector3(
            position.x / (sceneConfig.sceneSize.x * 5f),
            position.y / (sceneConfig.sceneSize.y * 10f),
            position.z / (sceneConfig.sceneSize.z * 5f)
        );
    }

    private static Vector3 NormalizeVelocity(SceneConfig sceneConfig, Vector3 velocity) {
        return velocity / sceneConfig.maxVelocityJet;
    }

    private static Vector3 NormalizeAngularVelocity(SceneConfig sceneConfig, Vector3 angularVelocity) {
        return angularVelocity / sceneConfig.maxAngularVelocityJet;
    }

    private static Vector3 NormalizeAcceleration(SceneConfig sceneConfig, Vector3 acceleration) {
        return acceleration / sceneConfig.maxVelocityJet;
    }

    private static Vector3 NormalizeAngularAcceleration(SceneConfig sceneConfig, Vector3 angularAcceleration) {
        return angularAcceleration / sceneConfig.maxAngularVelocityJet;
    }


    #endregion

    private static void CreateFakeDecoyData(SceneData sceneData, Transform jetTransform) {
        AircraftController aircraftController = jetTransform.GetComponent<AircraftController>();
        
        if (aircraftController.m_useFakeDecoyData) {
            DecoyData decoyData = new DecoyData(sceneData.GetSceneConfig().decoyPrefab, jetTransform.parent, jetTransform.position + jetTransform.forward * 10, sceneData);
            decoyData.GetObject().GetComponent<Rigidbody>().useGravity = false;
            sceneData.GetSceneComponents().AddDecoyData(decoyData);

            aircraftController.m_useFakeDecoyData = false;
        }
    }

    private static void CreateFakeMissileData(SceneData sceneData, Transform jetTransform) {
        AircraftController aircraftController = jetTransform.GetComponent<AircraftController>();

        if (aircraftController.m_useFakeMissileData) {
            GameObject missileObject = GameObject.Instantiate(sceneData.GetSceneConfig().missileGameObject, jetTransform.position + jetTransform.forward * 10, jetTransform.rotation, jetTransform.parent);
            MissilePhysics missilePhysics = missileObject.GetComponent<MissilePhysics>();
            MissileData missileData = new MissileData(missilePhysics);
            missilePhysics.SetTarget(jetTransform.gameObject);
            sceneData.GetSceneComponents().AddLaunchedMissileData(missileData);

            aircraftController.m_useFakeMissileData = false;
        }
    }
}



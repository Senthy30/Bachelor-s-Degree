using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneRewards {

    public static RewardsConfig rewardsConfig;
    
    // Public methods ---------------------------------------------------------
    public static JetRewards RequestJetReward(SceneData sceneData, EnemiesData enemiesData) {
        SceneComponents sceneComponents = sceneData.GetSceneComponents();

        JetRewards ownshipJetRewards = new JetRewards();

        // Neutral
        ownshipJetRewards.inbound = GetInboundReward(sceneComponents, enemiesData.ownshipJetObject);

        // Advantage
        ownshipJetRewards.ownshipAdvantageBehindTarget = GetOwnshipAdvantageBehindTarget(enemiesData);
        ownshipJetRewards.ownshipHasTargetInRange = GetOwnshipHasTargetInRange(enemiesData);
        ownshipJetRewards.ownshipChasesTarget = GetOwnshipChasesTarget(enemiesData);
        ownshipJetRewards.ownshipLaunchesMissile = GetOwnshipLaunchesMissile(enemiesData);
        ownshipJetRewards.ownshipDropsDecoy = GetOwnshipDropDecoy(enemiesData);
        ownshipJetRewards.ownshipShirksIncomingMissile = GetOwnshipShirksIncomingMissile(enemiesData);

        // Disadvantage
        ownshipJetRewards.targetAdvantageBehindOwnship = GetTargetAdvantageBehindOwnship(enemiesData);
        ownshipJetRewards.targetHasOwnshipInRange = GetTargetHasOwnshipInRange(enemiesData);
        ownshipJetRewards.targetChasesOwnship = GetTargetChasesOwnship(enemiesData);
        ownshipJetRewards.targetLaunchesMissile = GetTargetLaunchesMissile(enemiesData);
        ownshipJetRewards.targetShirksMissile = GetTargetShirksMissile(enemiesData);

        ownshipJetRewards.CalculateTotalReward();

        return ownshipJetRewards;
    }

    public static RewardsConfig GetRewardsConfig() {
        return rewardsConfig;
    }

    // Private methods --------------------------------------------------------
    // Reward calculation -----------------------------------------------------
    // Neutral ----------------------------------------------------------------
    private static float GetInboundReward(SceneComponents sceneComponents, GameObject ownshipJetObject) {
        Vector3 boxScale = sceneComponents.GetBoxData().GetObject().transform.localScale;
        Vector3 jetPosition = ownshipJetObject.transform.localPosition + new Vector3(boxScale.x / 2f, 0f, boxScale.z / 2f);

        if (jetPosition.y < rewardsConfig.distanceToFloorNCeiling) {
            return rewardsConfig.jetNotInboundBox;
        } else if (jetPosition.y > boxScale.y - rewardsConfig.distanceToFloorNCeiling) {
            return rewardsConfig.jetNotInboundBox;
        } else if (jetPosition.x < rewardsConfig.distanceToWall) {
            return rewardsConfig.jetNotInboundBox;
        } else if (jetPosition.x > boxScale.x - rewardsConfig.distanceToWall) {
            return rewardsConfig.jetNotInboundBox;
        } else if (jetPosition.z < rewardsConfig.distanceToWall) {
            return rewardsConfig.jetNotInboundBox;
        } else if (jetPosition.z > boxScale.z - rewardsConfig.distanceToWall) {
            return rewardsConfig.jetNotInboundBox;
        }

        return rewardsConfig.jetInboundBox;
    }

    // Advantage --------------------------------------------------------------
    private static float GetOwnshipAdvantageBehindTarget(EnemiesData enemiesData) {
        if (enemiesData.distance > rewardsConfig.distanceAdvantageBehind) {
            return 0.0f;
        }

        if (enemiesData.angleOwnshipBehindTarget < rewardsConfig.angleAdvantageBehind) {
            return rewardsConfig.advantageBehind;
        } 
            
        return 0.0f;
    }

    private static float GetOwnshipHasTargetInRange(EnemiesData enemiesData) {
        if (enemiesData.distance > rewardsConfig.distanceAdvantageBehind) {
            return 0.0f;
        }

        if (enemiesData.angleOwnshipToTarget < rewardsConfig.angleHasInRange) {
            return rewardsConfig.hasInRange;
        }

        return 0.0f;
    }

    private static float GetOwnshipChasesTarget(EnemiesData enemiesData) {
        if (enemiesData.distance > rewardsConfig.distanceAdvantageBehind) {
            return -rewardsConfig.chase;
        }

        if (enemiesData.angleOwnshipToTarget < rewardsConfig.angleChase) {
            return rewardsConfig.chase;
        }

        return -rewardsConfig.chase;
    }

    private static float GetOwnshipLaunchesMissile(EnemiesData enemiesData) {
        if (enemiesData.ownshipJetAction.GetLaunchMissileAction() == JetAction.LaunchMissileAction.None) {
            return 0.0f;
        }

        if (enemiesData.distance > rewardsConfig.distanceLaunchMissile || 
            enemiesData.angleOwnshipToTarget > rewardsConfig.angleLaunchMissile || 
            !(enemiesData.targetAircraftController.GetIncomingMissileTransform() != null && 
            !enemiesData.targetIncomingMissile)) 
        {
            return -rewardsConfig.launchMissile;
        }
        return rewardsConfig.launchMissile;
    }

    private static float GetOwnshipDropDecoy(EnemiesData enemiesData) {
        if (enemiesData.ownshipJetAction.GetDropDecoyAction() == JetAction.DropDecoyAction.None) {
            return 0.0f;
        }

        if (enemiesData.ownshipAircraftController.GetIncomingMissileTransform() != null ||
            !enemiesData.ownshipIncomingMissile) {
            return -rewardsConfig.dropDecoy;
        }
        return rewardsConfig.dropDecoy;
    }

    private static float GetOwnshipShirksIncomingMissile(EnemiesData enemiesData) {
        if (enemiesData.ownshipJetAction.GetDropDecoyAction() == JetAction.DropDecoyAction.Drop) {
            return 0.0f;
        }

        if (enemiesData.ownshipAircraftController.GetIncomingMissileTransform() == null && enemiesData.ownshipIncomingMissile)
            return rewardsConfig.shirksIncomingMissile;

        return 0.0f;
    }

    // Disadvantage -----------------------------------------------------------
    private static float GetTargetAdvantageBehindOwnship(EnemiesData enemiesData) {
        if (enemiesData.distance > rewardsConfig.distanceAdvantageBehind) {
            return 0.0f;
        }

        if (enemiesData.angleTargetBehindOwnship < rewardsConfig.angleAdvantageBehind) {
            return -rewardsConfig.advantageBehind;
        } 
            
        return 0.0f;
    }

    private static float GetTargetHasOwnshipInRange(EnemiesData enemiesData) {
        if (enemiesData.distance > rewardsConfig.distanceAdvantageBehind) {
            return 0.0f;
        }

        if (enemiesData.angleTargetToOwnship < rewardsConfig.angleHasInRange) {
            return -rewardsConfig.hasInRange;
        } 
            
        return 0.0f;
    }

    private static float GetTargetChasesOwnship(EnemiesData enemiesData) {
        if (enemiesData.distance > rewardsConfig.distanceAdvantageBehind) {
            return 0.0f;
        }

        if (enemiesData.angleTargetToOwnship < rewardsConfig.angleChase) {
            return -rewardsConfig.chase;
        }

        return rewardsConfig.chase;
    }

    private static float GetTargetLaunchesMissile(EnemiesData enemiesData) {
        if (!enemiesData.ownshipIncomingMissile && enemiesData.ownshipAircraftController.GetIncomingMissileTransform() != null)
            return rewardsConfig.targetLaunchMissile;
        return 0.0f;
    }

    private static float GetTargetShirksMissile(EnemiesData enemiesData) {
        if (enemiesData.targetJetAction.GetDropDecoyAction() == JetAction.DropDecoyAction.Drop) {
            return 0.0f;
        }

        if (enemiesData.targetAircraftController.GetIncomingMissileTransform() == null && enemiesData.targetIncomingMissile)
            return rewardsConfig.targetShirksIncomingMissile;

        return 0.0f;
    }

}

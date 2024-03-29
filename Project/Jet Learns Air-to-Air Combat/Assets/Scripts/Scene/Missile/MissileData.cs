using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileData {
    
    private Team m_launchingTeam;
    private MissilePhysics m_missilePhysics;

    public MissileData (MissilePhysics missilePhysics) {
        m_missilePhysics = missilePhysics;
    }

    public GameObject GetObject() {
        return m_missilePhysics.gameObject;
    }

    public MissilePhysics GetMissilePhysics() {
        return m_missilePhysics;
    }

    public Team GetLaunchingTeam() {
        return m_launchingTeam;
    }

    public void SetLaunchingTeam(Team team) {
        m_launchingTeam = team;
    }

}

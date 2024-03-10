using UnityEngine;

public interface IAircraftCarrierData {
    public int GetIdScene();
    public Team GetTeam();
    public Transform GetTransformJetSpawnPoint();
    public GameObject GetObject();
}

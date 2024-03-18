using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneData : MonoBehaviour {

    private static int nextIdScene = 0;
    private int idScene;

    [SerializeField] private SceneConfig sceneConfig;
    private SceneBuilder sceneBuilder = null;
    private SceneComponents sceneComponents = null;

    void Awake() {
        BuildScene();
    }

    public void BuildScene() {
        idScene = nextIdScene++;
        SetResolutionSceneConfig();
        SceneComponents.SetSceneConfig(sceneConfig);

        sceneBuilder = new SceneBuilder(idScene, gameObject, sceneConfig);
    }

    public void DropDecoy(Team team) {
        JetData jetData = sceneComponents.GetJetData(team);

        if (jetData.GetNumDecoys() > 0) {
            jetData.DecrementNumDecoys();

            DecoyData decoyObject = sceneBuilder.BuildDecoy(jetData.GetDecoySpawnPointTransform().position);
            DecoyPhysics decoyPhysics = decoyObject.GetObject().GetComponent<DecoyPhysics>();

            decoyPhysics.AddJetCollision(jetData.GetColliderParentObject());
            decoyPhysics.SetVelocity(jetData.GetObject().GetComponent<Rigidbody>().velocity);
            decoyPhysics.SetCollisionsIgnoreAdded();

            sceneComponents.TriggerMissilesFindTarget();
        }
    }

    public void DestroyDecoy(DecoyData decoyData) {
        sceneComponents.RemoveHeatEmission(decoyData.GetHeatEmission());
    }

    // Setters ------------------------------------------------

    public void SetSceneComponents(SceneComponents sceneComponents) {
        this.sceneComponents = sceneComponents;
    }

    public void SetResolutionSceneConfig() {
        if (sceneConfig.resolution != TheaterData.GetResolution())
            sceneConfig.resolution = TheaterData.GetResolution();
    }

    public static void SetNextIdScene(int id) {
        nextIdScene = id;
    }

    // Getters ------------------------------------------------

    public SceneComponents GetSceneComponents() {
        return sceneComponents;
    }

    public SceneConfig GetSceneConfig() {
        return sceneConfig;
    }
}

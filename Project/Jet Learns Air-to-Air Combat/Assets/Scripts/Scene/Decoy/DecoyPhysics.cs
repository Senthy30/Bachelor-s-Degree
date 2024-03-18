using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoyPhysics : MonoBehaviour {

    private static SceneConfig sceneConfig;

    private DecoyData m_decoyData;
    private Collider m_collider;
    private Rigidbody m_rigidbody;
    private List<Collider> m_collisionsIgnored;

    [Header("Detach")]
    [SerializeField] private float m_timeDetach = 0.2f;
    [SerializeField] private float m_speedAtDetach = 10;
    private bool m_detachedCompleted = false;
    private bool m_collisionsIgnoreAdded = false;
    private float m_currentTimeDetach = 0;

    private void Awake() {
        m_collider = gameObject.GetComponent<Collider>();
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
        m_collisionsIgnored = new List<Collider>();
    }

    private void Update() {
        if (!m_detachedCompleted) {
            m_currentTimeDetach += Time.deltaTime;
            if (m_currentTimeDetach >= m_timeDetach) {
                for (int i = 0; i < m_collisionsIgnored.Count; i++) {
                    Physics.IgnoreCollision(m_collider, m_collisionsIgnored[i], false);
                }
                m_collisionsIgnored.Clear();
                m_detachedCompleted = true;
            }
        }
    }

    public void AddJetCollision(GameObject collisionObject) {
        Collider collider = collisionObject.GetComponent<Collider>();
        if (collider != null) {
            Physics.IgnoreCollision(m_collider, collider);
            m_collisionsIgnored.Add(collider);
        }

        if (collisionObject.transform.childCount > 0) {
            for (int i = 0; i < collisionObject.transform.childCount; i++)
                AddJetCollision(collisionObject.transform.GetChild(i).gameObject);
        }
    }

    public void SetVelocity(Vector3 velocity) {
        m_rigidbody.velocity = velocity - transform.up * m_speedAtDetach;
    }

    public void SetCollisionsIgnoreAdded() {
        m_collisionsIgnoreAdded = true;
    }

    public void SetDecoyData(DecoyData decoyData) {
        m_decoyData = decoyData;
    }

    private void OnCollisionEnter(Collision collision) {
        if (!m_collisionsIgnoreAdded) {
            return;
        }

        m_decoyData.OnDestroy();
        Destroy(gameObject);
    }

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }

}

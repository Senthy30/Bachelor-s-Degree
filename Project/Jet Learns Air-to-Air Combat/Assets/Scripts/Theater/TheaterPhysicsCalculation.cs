using System.Collections.Generic;
using UnityEngine;

public class TheaterPhysicsCalculation : MonoBehaviour {

    const int numSurfaces = 10;

    struct AerodynamicSurfaceConfigShader {
        public float liftCurve;
        public float skinFrictionCoef;
        public float zeroLiftAoA;
        public float stallAnglePos;
        public float stallAngleNeg;
        public float flapChord;
        public float airfoilChordFraction;
        public float span;
        public float aspectRatio;
    };

    struct SurfaceInfoShader {
        public Vector3 airVelocity;
        public Vector3 relativePosition;
    };

    struct ForcesShader {
        public Vector3 force;
        public Vector3 torque;
    }

    [SerializeField]
    private ComputeShader aircraftPhysicsShader;

    [SerializeField]
    private AircraftPhysics aircraftPhysics;

    [SerializeField]
    private List <AerodynamicSurfaceConfig> aerodynamicSurfaceConfigs;

    private AerodynamicSurfaceConfigShader[] aerodynamicSurfaceConfigShader;
    private SurfaceInfoShader[] surfaceInfoShader;
    private ForcesShader[] forcesShader;
    private float[] flapAngleShader;

    private ComputeBuffer flapAnglesComputeBuffer;
    private ComputeBuffer surfaceInfoComputeBuffer;
    private ComputeBuffer forcesComputeBuffer;

    private void Awake() {
        InitializeShader();
    }

    private void FixedUpdate() {
        UpdateFlapAnglesShader();
        UpdateSurfaceInfoShader();

        StartExecutionGPU();
        CollectDataGPU();

        ClearComputeBuffers();
    }

    private void StartExecutionGPU() {
        int totalMemory = sizeof(float) * 6;
        forcesShader = new ForcesShader[numSurfaces];
        forcesComputeBuffer = new ComputeBuffer(numSurfaces, totalMemory);
        forcesComputeBuffer.SetData(forcesShader);

        aircraftPhysicsShader.SetBuffer(0, "_forcesArray", forcesComputeBuffer);

        aircraftPhysicsShader.Dispatch(0, 1, 1, numSurfaces);
    }

    private void CollectDataGPU() {
        forcesComputeBuffer.GetData(forcesShader);

        /*
        for (int i = 0; i < numSurfaces; i++) {
            Debug.Log(forcesShader[i].force);
            Debug.Log(forcesShader[i].torque);
        }
        Debug.Log("Done GPU");
        */
    }

    private void UpdateFlapAnglesShader() {
        flapAngleShader = new float[numSurfaces];

        List <AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = aircraftPhysics.GetAerodynamicSurfaces();
        for (int i = 0; i < numSurfaces; i++) 
            flapAngleShader[i] = aerodynamicSurfacePhysics[i].GetFlapAngle();
        
        int totalMemory = sizeof(float);
        flapAnglesComputeBuffer = new ComputeBuffer(numSurfaces, totalMemory);
        flapAnglesComputeBuffer.SetData(flapAngleShader);

        aircraftPhysicsShader.SetBuffer(0, "_flapAngleArray", flapAnglesComputeBuffer);
    }

    private void UpdateSurfaceInfoShader() {
        surfaceInfoShader = new SurfaceInfoShader[numSurfaces];

        List<AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = aircraftPhysics.GetAerodynamicSurfaces();
        Rigidbody rigidbody = aircraftPhysics.gameObject.GetComponent<Rigidbody>();
        Vector3 velocity = rigidbody.velocity;
        Vector3 angularVelocity = rigidbody.angularVelocity;
        Vector3 centerOfMass = rigidbody.worldCenterOfMass;

        for (int i = 0; i < numSurfaces; i++) {
            Vector3 relativePosition = aerodynamicSurfacePhysics[i].transform.position - centerOfMass;

            surfaceInfoShader[i].airVelocity = -velocity - Vector3.Cross(angularVelocity, relativePosition);
            surfaceInfoShader[i].relativePosition = relativePosition;
        }

        int totalMemory = sizeof(float) * 6;
        surfaceInfoComputeBuffer = new ComputeBuffer(numSurfaces, totalMemory);
        surfaceInfoComputeBuffer.SetData(surfaceInfoShader);

        aircraftPhysicsShader.SetBuffer(0, "_surfaceInfoArray", surfaceInfoComputeBuffer);
    }

    private void ClearComputeBuffers() {
        flapAnglesComputeBuffer.Dispose();
        surfaceInfoComputeBuffer.Dispose();
        forcesComputeBuffer.Dispose();
    }

    private void InitializeShader() {
        int numAerodynamicSurfaces = aerodynamicSurfaceConfigs.Count;
        int totalMemory = sizeof(float) * 9;

        aerodynamicSurfaceConfigShader = new AerodynamicSurfaceConfigShader[numAerodynamicSurfaces];
        for (int i = 0; i < numAerodynamicSurfaces; i++) {
            aerodynamicSurfaceConfigShader[i].liftCurve = aerodynamicSurfaceConfigs[i].liftCurve;
            aerodynamicSurfaceConfigShader[i].skinFrictionCoef = aerodynamicSurfaceConfigs[i].skinFrictionCoef;
            aerodynamicSurfaceConfigShader[i].zeroLiftAoA = aerodynamicSurfaceConfigs[i].zeroLiftAoA;
            aerodynamicSurfaceConfigShader[i].stallAnglePos = aerodynamicSurfaceConfigs[i].stallAnglePos;
            aerodynamicSurfaceConfigShader[i].stallAngleNeg = aerodynamicSurfaceConfigs[i].stallAngleNeg;
            aerodynamicSurfaceConfigShader[i].flapChord = aerodynamicSurfaceConfigs[i].flapChord;
            aerodynamicSurfaceConfigShader[i].airfoilChordFraction = aerodynamicSurfaceConfigs[i].airfoilChordFraction;
            aerodynamicSurfaceConfigShader[i].span = aerodynamicSurfaceConfigs[i].span;
            aerodynamicSurfaceConfigShader[i].aspectRatio = aerodynamicSurfaceConfigs[i].aspectRatio;
        }

        ComputeBuffer computeBuffer = new ComputeBuffer(numAerodynamicSurfaces, totalMemory);
        computeBuffer.SetData(aerodynamicSurfaceConfigShader);

        aircraftPhysicsShader.SetBuffer(0, "_surfaceConfigArray", computeBuffer);
    }

    public void SetAircraftPhysics(AircraftPhysics aircraftPhysics) {
        this.aircraftPhysics = aircraftPhysics;
    }

}

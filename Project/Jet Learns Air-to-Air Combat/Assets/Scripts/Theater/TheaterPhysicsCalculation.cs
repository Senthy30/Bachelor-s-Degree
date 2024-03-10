using Cinemachine.Utility;
using System.Collections.Generic;
using UnityEngine;

public class TheaterPhysicsCalculation : MonoBehaviour {

    const float PREDICTION_FRACTION = 0.5f;
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
        public Quaternion rotation;
    };

    struct ForcesShader {
        public Vector3 force;
        public Vector3 torque;
    }

    int numAircraftPhysics;
    [SerializeField]
    private ComputeShader aircraftPhysicsShader;

    [SerializeField]
    private List<AircraftPhysics> aircraftPhysicsArray;

    [SerializeField]
    private List <AerodynamicSurfaceConfig> aerodynamicSurfaceConfigs;

    private AerodynamicSurfaceConfigShader[] aerodynamicSurfaceConfigShader;
    private SurfaceInfoShader[] surfaceInfoShader;
    private ForcesShader[] forcesShader;
    private float[] flapAngleShader;

    private ComputeBuffer flapAnglesComputeBuffer;
    private ComputeBuffer surfaceInfoComputeBuffer;
    private ComputeBuffer forcesComputeBuffer;

    private ForcesShader[] sumForcesShaderPerAircraft;
    private Vector3[] velocityPredictionPerAircraft;
    private Vector3[] angularVelocityPredictionPerAircraft;
    private ForcesShader[] sumForcesShaderPredictionPerAircraft;

    private void Start() {
        // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace);
        InitializeShader();
    }

    private void FixedUpdate() {
        float lastTime = Time.realtimeSinceStartup;

        InitializeArrays();

        CalculateSurfaceInfoShaders(false);

        // Debug.Log("Time: " + ((Time.realtimeSinceStartup - lastTime) * 1000));
        // lastTime = Time.realtimeSinceStartup;

        UpdateFlapAnglesShader();
        UpdateSurfaceInfoShader();

        StartExecutionGPU();

        CollectDataGPU(false);

        // Debug.Log("GPU time: " + ((Time.realtimeSinceStartup - lastTime) * 1000));

        ApplyForces();

        ClearComputeBuffers();
    }

    private void StartExecutionGPU() {
        int totalMemory = sizeof(float) * 6;

        forcesShader = new ForcesShader[numSurfaces * numAircraftPhysics];
        forcesComputeBuffer = new ComputeBuffer(numSurfaces * numAircraftPhysics, totalMemory);
        forcesComputeBuffer.SetData(forcesShader);

        aircraftPhysicsShader.SetBuffer(0, "_forcesArray", forcesComputeBuffer);

        int divideBy = 96;
        int xDisplatches = Mathf.CeilToInt(1f * numAircraftPhysics / divideBy);

        aircraftPhysicsShader.Dispatch(0, xDisplatches, 10, 1);
    }

    private void CollectDataGPU(bool usePrediction) {
        forcesComputeBuffer.GetData(forcesShader);

        for (int j = 0; j < numAircraftPhysics; j++) {
            for (int i = 0; i < numSurfaces; i++) {
                if (forcesShader[j * numSurfaces + i].force.IsNaN() == false) {
                    if (!usePrediction) {
                        sumForcesShaderPerAircraft[j].force += forcesShader[j * numSurfaces + i].force;
                    } else {
                        sumForcesShaderPredictionPerAircraft[j].force += forcesShader[j * numSurfaces + i].force;
                    }
                }

                if (forcesShader[j * numSurfaces + i].torque.IsNaN() == false) {
                    if (!usePrediction) {
                        sumForcesShaderPerAircraft[j].torque += forcesShader[j * numSurfaces + i].torque;
                    } else {
                        sumForcesShaderPredictionPerAircraft[j].torque += forcesShader[j * numSurfaces + i].torque;
                    }
                }
            }
        }
    }

    private void CalculatePrediction() {
        float thrustPercent;
        float thrust;
        Transform _transform;
        Rigidbody _rigidbody;

        for (int currAircraft = 0; currAircraft < numAircraftPhysics; currAircraft++) {
            thrustPercent = aircraftPhysicsArray[currAircraft].GetThrustPercent();
            thrust = aircraftPhysicsArray[currAircraft].GetThrust();
            _transform = aircraftPhysicsArray[currAircraft].transform;
            _rigidbody = aircraftPhysicsArray[currAircraft].GetComponent<Rigidbody>();

            // calculate velocity prediction
            Vector3 force = sumForcesShaderPerAircraft[currAircraft].force + _transform.forward * thrust * thrustPercent + Physics.gravity * _rigidbody.mass;
            velocityPredictionPerAircraft[currAircraft] = _rigidbody.velocity + Time.fixedDeltaTime * PREDICTION_FRACTION * force / _rigidbody.mass;

            // calculate angular velocity prediction
            Quaternion inertiaTensorWorldRotation = _rigidbody.rotation * _rigidbody.inertiaTensorRotation;
            Vector3 torqueChange = Quaternion.Inverse(inertiaTensorWorldRotation) * sumForcesShaderPerAircraft[currAircraft].torque;
            Vector3 angularVelocityChange = new Vector3(
                torqueChange.x / _rigidbody.inertiaTensor.x,
                torqueChange.y / _rigidbody.inertiaTensor.y,
                torqueChange.z / _rigidbody.inertiaTensor.z
            );

            angularVelocityPredictionPerAircraft[currAircraft] = _rigidbody.angularVelocity + Time.fixedDeltaTime * PREDICTION_FRACTION * (inertiaTensorWorldRotation * angularVelocityChange);
        }
    }

    private void UpdateFlapAnglesShader() {
        flapAngleShader = new float[numSurfaces * numAircraftPhysics];

        int currDim = 0;
        for (int currAircraft = 0; currAircraft < numAircraftPhysics; currAircraft++) {
            List<AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = aircraftPhysicsArray[currAircraft].GetAerodynamicSurfaces();

            for (int currSurface = 0; currSurface < numSurfaces; currSurface++)
                flapAngleShader[currDim + currSurface] = aerodynamicSurfacePhysics[currSurface].GetFlapAngle();
            
            currDim += numSurfaces;
        }

        int totalMemory = sizeof(float);
        flapAnglesComputeBuffer = new ComputeBuffer(numAircraftPhysics * numSurfaces, totalMemory);
        flapAnglesComputeBuffer.SetData(flapAngleShader);

        aircraftPhysicsShader.SetBuffer(0, "_flapAngleArray", flapAnglesComputeBuffer);
    }

    private void UpdateSurfaceInfoShader() {
        int totalMemory = sizeof(float) * 10;

        surfaceInfoComputeBuffer = new ComputeBuffer(numAircraftPhysics * numSurfaces, totalMemory);
        surfaceInfoComputeBuffer.SetData(surfaceInfoShader);

        aircraftPhysicsShader.SetBuffer(0, "_surfaceInfoArray", surfaceInfoComputeBuffer);
    }

    private void CalculateSurfaceInfoShaders(bool usePrediction) {
        int currDim = 0;

        surfaceInfoShader = new SurfaceInfoShader[numSurfaces * numAircraftPhysics];
        for (int currAircraft = 0; currAircraft < numAircraftPhysics; currAircraft++) {
            List<AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = aircraftPhysicsArray[currAircraft].GetAerodynamicSurfaces();
            Rigidbody rigidbody = aircraftPhysicsArray[currAircraft].gameObject.GetComponent<Rigidbody>();
            Vector3 velocity = (!usePrediction) ? rigidbody.velocity : velocityPredictionPerAircraft[currAircraft];
            Vector3 angularVelocity = (!usePrediction) ? rigidbody.angularVelocity : angularVelocityPredictionPerAircraft[currAircraft];
            Vector3 centerOfMass = rigidbody.worldCenterOfMass;

            for (int currSurface = 0; currSurface < numSurfaces; currSurface++) {
                Vector3 relativePosition = aerodynamicSurfacePhysics[currSurface].transform.position - centerOfMass;

                surfaceInfoShader[currDim + currSurface].airVelocity = -velocity - Vector3.Cross(angularVelocity, relativePosition);
                surfaceInfoShader[currDim + currSurface].relativePosition = relativePosition;
                surfaceInfoShader[currDim + currSurface].rotation = aerodynamicSurfacePhysics[currSurface].transform.rotation;
            }

            currDim += numSurfaces;
        }

        if (usePrediction) {
            surfaceInfoComputeBuffer.Dispose();
        }
    }

    private void ApplyForces() {
        for (int currAircraft = 0; currAircraft < numAircraftPhysics; currAircraft++) {
            //Vector3 forceApplied = (sumForcesShaderPerAircraft[currAircraft].force + sumForcesShaderPredictionPerAircraft[currAircraft].force) / 2f;
            //Vector3 torqueApplied = (sumForcesShaderPerAircraft[currAircraft].torque + sumForcesShaderPredictionPerAircraft[currAircraft].torque) / 2f;

            Vector3 forceApplied = sumForcesShaderPerAircraft[currAircraft].force;
            Vector3 torqueApplied = sumForcesShaderPerAircraft[currAircraft].torque;

            aircraftPhysicsArray[currAircraft].ApplyForces(forceApplied, torqueApplied);
        }
    }

    private void ClearComputeBuffers() {
        flapAnglesComputeBuffer.Dispose();
        surfaceInfoComputeBuffer.Dispose();
        forcesComputeBuffer.Dispose();
    }

    private void InitializeArrays() {
        sumForcesShaderPerAircraft = new ForcesShader[numAircraftPhysics];
        sumForcesShaderPredictionPerAircraft = new ForcesShader[numAircraftPhysics];
        velocityPredictionPerAircraft = new Vector3[numAircraftPhysics];
        angularVelocityPredictionPerAircraft = new Vector3[numAircraftPhysics];
    }

    private void InitializeShader() {
        int numAerodynamicSurfaces = aerodynamicSurfaceConfigs.Count;
        int totalMemory = sizeof(float) * 9;

        numAircraftPhysics = aircraftPhysicsArray.Count;
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

    public void AddAircraftPhysics(AircraftPhysics aircraftPhysics) {
        this.aircraftPhysicsArray.Add(aircraftPhysics);
    }

    public void ClearAircraftPhysics() {
        this.aircraftPhysicsArray.Clear();
    }

}

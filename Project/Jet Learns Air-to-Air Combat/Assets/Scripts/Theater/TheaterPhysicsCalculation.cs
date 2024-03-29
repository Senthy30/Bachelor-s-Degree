using Cinemachine.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using static TheaterPhysicsCalculation;

public class TheaterPhysicsCalculation : MonoBehaviour {

    private DebugMode debugMode;

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

    public struct SurfaceInfoShader {
        public Vector3 airVelocity;
        public Vector3 relativePosition;
        public Quaternion rotation;
    };

    public struct ForcesShader {
        public Vector3 force;
        public Vector3 torque;
    }

    private bool theaterWasBuilt = false;
    private int numAircraftPhysics;
    private float waitTimeBeforeStart = 1f;

    [SerializeField] private int m_numCalculationThreads;
    [SerializeField] private ComputeShader aircraftPhysicsShader;

    [SerializeField] private List <AircraftPhysics> aircraftPhysicsArray;
    [SerializeField] private List <AerodynamicSurfaceConfig> aerodynamicSurfaceConfigs;

    private AerodynamicSurfaceConfigShader[] aerodynamicSurfaceConfigShader;
    private SurfaceInfoShader[] surfaceInfoShader;
    private ForcesShader[] forcesShader;
    private float[] flapAngleShader;

    private ComputeBuffer skipAircraftComputeBuffer;
    private ComputeBuffer flapAnglesComputeBuffer;
    private ComputeBuffer surfaceInfoComputeBuffer;
    private ComputeBuffer aerodynamicSurfaceComputeBuffer;
    private ComputeBuffer forcesComputeBuffer;

    private int[] skipAircraftArray;
    private ForcesShader[] sumForcesShaderPerAircraft;
    private Vector3[] velocityPredictionPerAircraft; // TOBEDELETED
    private Vector3[] angularVelocityPredictionPerAircraft; // TOBEDELETED
    private ForcesShader[] sumForcesShaderPredictionPerAircraft; // TOBEDELETED

    private TheaterPhysicsCalculationThread theaterPhysicsCalculationThread;

    // Debug

    private int countTimeForJetPhysics;
    private float sumTimeForJetPhysics;

    private int countTimeForGPU;
    private float sumTimeForGPU;

    private void Start() {
        // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace);
        // InitializeShader();
        debugMode = FindObjectOfType<DebugMode>();
    }

    private void FixedUpdate() {
        if (!theaterWasBuilt || waitTimeBeforeStart > 0) {
            waitTimeBeforeStart -= Time.fixedDeltaTime;
            return;
        }

        float lastTime = Time.realtimeSinceStartup;

        ResetArrays();
        UpdateDataGPU();

        StartExecutionGPU();
        CollectDataGPU(false);

        ApplyForces();

        if (debugMode.IsActive()) {
            sumTimeForJetPhysics += (Time.realtimeSinceStartup - lastTime) * 1000;
            countTimeForJetPhysics++;
            debugMode.SetAverageTimeJetPhysics(sumTimeForJetPhysics / countTimeForJetPhysics);
            debugMode.SetTimeJetPhysics((Time.realtimeSinceStartup - lastTime) * 1000);
        }
    }

    private void UpdateDataGPU() {
        theaterPhysicsCalculationThread.UpdateValuesForPhysics();

        theaterPhysicsCalculationThread.StartThread(m_numCalculationThreads);
        theaterPhysicsCalculationThread.WaitForThreads();

        theaterPhysicsCalculationThread.SetDataInFlapAnglesComputeBuffer(flapAnglesComputeBuffer);
        theaterPhysicsCalculationThread.SetDataInSurfaceInfoComputeBuffer(surfaceInfoComputeBuffer);

        aircraftPhysicsShader.SetBuffer(0, "_flapAngleArray", flapAnglesComputeBuffer);
        aircraftPhysicsShader.SetBuffer(0, "_surfaceInfoArray", surfaceInfoComputeBuffer);
    }

    /*
    private void UpdateDataGPU() {
        DataJob dataJob = new DataJob {
            skipAircraftArray = skipAircraftArray,
            flapAngleShader = flapAngleShader,
            surfaceInfoShader = surfaceInfoShader,
            aircraftPhysicsArray = aircraftPhysicsArray
        };
        NativeArray<DataJob> dataJobArray = new NativeArray<DataJob>(1, Allocator.TempJob);
        dataJobArray[0] = dataJob;

        UpdateGPUDataJob updateGPUDataJob = new UpdateGPUDataJob {
            m_numSurfaces = numSurfaces,
            dataJobArray = dataJobArray
        };

        JobHandle jobHandle = updateGPUDataJob.Schedule(numAircraftPhysics, 10);
        jobHandle.Complete();

        flapAnglesComputeBuffer.SetData(updateGPUDataJob.dataJobArray[0].flapAngleShader);
        aircraftPhysicsShader.SetBuffer(0, "_flapAngleArray", flapAnglesComputeBuffer);

        surfaceInfoComputeBuffer.SetData(updateGPUDataJob.dataJobArray[0].surfaceInfoShader);
        aircraftPhysicsShader.SetBuffer(0, "_surfaceInfoArray", surfaceInfoComputeBuffer);
    }
    */

    /*
    private void FixedUpdate() {
        if (!theaterWasBuilt || waitTimeBeforeStart > 0) {
            waitTimeBeforeStart -= Time.fixedDeltaTime;
            return;
        }

        ResetArrays();

        float lastTime = Time.realtimeSinceStartup;

        CalculateSurfaceInfoShaders(false);
        UpdateFlapAnglesShader();
        UpdateSurfaceInfoShader();

        Debug.Log("CPU Time: " + ((Time.realtimeSinceStartup - lastTime) * 1000));

        StartExecutionGPU();
        CollectDataGPU(false);

        ApplyForces();
    }
    */

    private void StartExecutionGPU() {
        forcesComputeBuffer.SetData(forcesShader);
        skipAircraftComputeBuffer.SetData(skipAircraftArray);

        aircraftPhysicsShader.SetBuffer(0, "_forcesArray", forcesComputeBuffer);
        aircraftPhysicsShader.SetBuffer(0, "_skipAircraftArray", skipAircraftComputeBuffer);

        int divideBy = 120;
        int xDisplatches = Mathf.CeilToInt(1f * numAircraftPhysics / divideBy);

        aircraftPhysicsShader.Dispatch(0, xDisplatches, 10, 1); // [TODO] find optimal number of threads
    }

    private void CollectDataGPU(bool usePrediction) {
        float lastTime = Time.realtimeSinceStartup;

        forcesComputeBuffer.GetData(forcesShader);

        if (debugMode.IsActive()) {
            sumTimeForGPU += (Time.realtimeSinceStartup - lastTime) * 1000;
            countTimeForGPU++;
            debugMode.SetAverageTimeGPU(sumTimeForGPU / countTimeForGPU);
            debugMode.SetLastTimeGPU((Time.realtimeSinceStartup - lastTime) * 1000);
        }

        for (int j = 0; j < numAircraftPhysics; j++) {
            if (skipAircraftArray[j] != 0)
                continue;

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
            if (aircraftPhysicsArray[currAircraft] == null)
                continue;

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
        int currDim = 0;
        for (int currAircraft = 0; currAircraft < numAircraftPhysics; currAircraft++) {
            if (skipAircraftArray[currAircraft] != 0) {
                currDim += numSurfaces;
                continue;
            }

            List<AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = aircraftPhysicsArray[currAircraft].GetAerodynamicSurfaces();

            for (int currSurface = 0; currSurface < numSurfaces; currSurface++)
                flapAngleShader[currDim + currSurface] = aerodynamicSurfacePhysics[currSurface].GetFlapAngle();
            
            currDim += numSurfaces;
        }

        flapAnglesComputeBuffer.SetData(flapAngleShader);
        aircraftPhysicsShader.SetBuffer(0, "_flapAngleArray", flapAnglesComputeBuffer);
    }

    private void UpdateSurfaceInfoShader() {
        surfaceInfoComputeBuffer.SetData(surfaceInfoShader);
        aircraftPhysicsShader.SetBuffer(0, "_surfaceInfoArray", surfaceInfoComputeBuffer);
    }

    private void CalculateSurfaceInfoShaders(bool usePrediction) {
        int currDim = 0;

        for (int currAircraft = 0; currAircraft < numAircraftPhysics; currAircraft++) {
            if (skipAircraftArray[currAircraft] != 0) {
                currDim += numSurfaces;
                continue;
            }

            List<AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = aircraftPhysicsArray[currAircraft].GetAerodynamicSurfaces();
            Rigidbody rigidbody = aircraftPhysicsArray[currAircraft].gameObject.GetComponent<Rigidbody>();
            Vector3 velocity = rigidbody.velocity;
            Vector3 angularVelocity = rigidbody.angularVelocity;
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
            if (skipAircraftArray[currAircraft] != 0 || aircraftPhysicsArray[currAircraft].GetSkipApplyForces())
                continue;

            //Vector3 forceApplied = (sumForcesShaderPerAircraft[currAircraft].force + sumForcesShaderPredictionPerAircraft[currAircraft].force) / 2f;
            //Vector3 torqueApplied = (sumForcesShaderPerAircraft[currAircraft].torque + sumForcesShaderPredictionPerAircraft[currAircraft].torque) / 2f;

            //Vector3 forceApplied = sumForcesShaderPerAircraft[currAircraft].force;
            //Vector3 torqueApplied = sumForcesShaderPerAircraft[currAircraft].torque;

            aircraftPhysicsArray[currAircraft].ApplyForces(
                ref sumForcesShaderPerAircraft[currAircraft].force, 
                ref sumForcesShaderPerAircraft[currAircraft].torque
            );
        }
    }

    private void ClearComputeBuffers() {
        skipAircraftComputeBuffer.Dispose();
        flapAnglesComputeBuffer.Dispose();
        surfaceInfoComputeBuffer.Dispose();
        aerodynamicSurfaceComputeBuffer.Dispose();
        forcesComputeBuffer.Dispose();
    }

    private void InitializeArrays() {
        // sumForcesShaderPredictionPerAircraft = new ForcesShader[numAircraftPhysics];
        // velocityPredictionPerAircraft = new Vector3[numAircraftPhysics];
        // angularVelocityPredictionPerAircraft = new Vector3[numAircraftPhysics];

        sumForcesShaderPerAircraft = new ForcesShader[numAircraftPhysics];
        flapAngleShader = new float[numSurfaces * numAircraftPhysics];
        surfaceInfoShader = new SurfaceInfoShader[numSurfaces * numAircraftPhysics];
        forcesShader = new ForcesShader[numSurfaces * numAircraftPhysics];
    }

    private void InitializeShader() {
        int numAerodynamicSurfaces = aerodynamicSurfaceConfigs.Count;
        int totalMemory = sizeof(float) * 9;

        numAircraftPhysics = aircraftPhysicsArray.Count;

        skipAircraftArray = new int[numAircraftPhysics];
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

        aerodynamicSurfaceComputeBuffer = new ComputeBuffer(numAerodynamicSurfaces, totalMemory);

        aerodynamicSurfaceComputeBuffer.SetData(aerodynamicSurfaceConfigShader);
        aircraftPhysicsShader.SetBuffer(0, "_surfaceConfigArray", aerodynamicSurfaceComputeBuffer);
    }

    private void InitializeComputeBuffers() {
        skipAircraftComputeBuffer = new ComputeBuffer(numAircraftPhysics, sizeof(int));
        flapAnglesComputeBuffer = new ComputeBuffer(numAircraftPhysics * numSurfaces, sizeof(float));
        forcesComputeBuffer = new ComputeBuffer(numSurfaces * numAircraftPhysics, sizeof(float) * 6);
        surfaceInfoComputeBuffer = new ComputeBuffer(numAircraftPhysics * numSurfaces, sizeof(float) * 10);

#if UNITY_EDITOR
        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ClearComputeBuffers;
#endif
    }

    private void ResetArrays() {
        for (int i = 0; i < numAircraftPhysics; i++) {
            sumForcesShaderPerAircraft[i].force = Vector3.zero;
            sumForcesShaderPerAircraft[i].torque = Vector3.zero;
            forcesShader[i].force = Vector3.zero;
            forcesShader[i].torque = Vector3.zero;
            //sumForcesShaderPredictionPerAircraft[i].force = Vector3.zero;
            //sumForcesShaderPredictionPerAircraft[i].torque = Vector3.zero;
            // velocityPredictionPerAircraft[i] = Vector3.zero;
            // angularVelocityPredictionPerAircraft[i] = Vector3.zero;
        }
    }

    public int AddAircraftPhysics(AircraftPhysics aircraftPhysics) {
        this.aircraftPhysicsArray.Add(aircraftPhysics);

        return this.aircraftPhysicsArray.Count - 1;
    }

    public void MarkAircraftPyhsicsForSkipping(int index, int value) {
        this.skipAircraftArray[index] = value;
    }

    public void ClearAircraftPhysics() {
        this.aircraftPhysicsArray.Clear();
    }

    public void CallAfterTheaterBuilt() {
        InitializeShader();
        InitializeArrays();
        InitializeComputeBuffers();
        theaterPhysicsCalculationThread = new TheaterPhysicsCalculationThread(
            numAircraftPhysics, numSurfaces, skipAircraftArray, flapAngleShader, surfaceInfoShader, aircraftPhysicsArray
        );
        theaterWasBuilt = true;
    }
}

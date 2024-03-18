using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using static TheaterPhysicsCalculation;

public class TheaterPhysicsCalculationThread {

    private bool m_stopWorking;
    private bool m_threadsStarted;
    private int m_numSurfaces;
    private int m_numAircraftPhysics;
    private int m_numThreadsStarted;

    private int[] m_skipAircraftPhysics;
    private float[] m_flapAngleShader;
    private SurfaceInfoShader[] m_surfaceInfoShader;
    private List<AircraftPhysics> m_aircraftPhysicsArray;

    private int m_indexAircraftPhysics;
    private object m_lockIndexAircraftPhysics;
    private object m_lockThreadsStarted;

    private ManualResetEvent m_threadsFinished;

    public TheaterPhysicsCalculationThread(
        int numAircraftPhysics,
        int numSurfaces,
        int[] skipAircraftPhysics,
        float[] flapAngleShader, 
        SurfaceInfoShader[] surfaceInfoShader, 
        List<AircraftPhysics> aircraftPhysicsArray
    ) {
        m_numAircraftPhysics = numAircraftPhysics;
        m_numSurfaces = numSurfaces;
        m_skipAircraftPhysics = skipAircraftPhysics;
        m_flapAngleShader = flapAngleShader;
        m_surfaceInfoShader = surfaceInfoShader;
        m_aircraftPhysicsArray = aircraftPhysicsArray;

        m_lockIndexAircraftPhysics = new object();
        m_lockThreadsStarted = new object();
    }

    public void StartThread(int numThreads) {
        if (m_threadsStarted)
            return;

        m_stopWorking = false;
        m_indexAircraftPhysics = 0;
        m_numThreadsStarted = numThreads;
        m_threadsFinished = new ManualResetEvent(false);

        for (int i = 0; i < numThreads; i++)
            ThreadPool.QueueUserWorkItem(Work);

        m_threadsStarted = true;
    }

    public void WaitForThreads() {
        if (!m_threadsStarted) 
            return;
        
        WaitHandle.WaitAll(new WaitHandle[] { m_threadsFinished });
        m_threadsStarted = false;
    }

    private void Work(object threadContext) {
        int index;

        try {
            while (!m_stopWorking) {
                lock (m_lockIndexAircraftPhysics) {
                    while (m_indexAircraftPhysics < m_numAircraftPhysics && m_skipAircraftPhysics[m_indexAircraftPhysics] != 0)
                        ++m_indexAircraftPhysics;

                    if (m_indexAircraftPhysics < m_numAircraftPhysics) {
                        index = m_indexAircraftPhysics;
                        ++m_indexAircraftPhysics;
                    } else {
                        m_stopWorking = true;
                        break;
                    }
                }

                int currDim = index * m_numSurfaces;
                List<AerodynamicSurfacePhysics> aerodynamicSurfacePhysics = m_aircraftPhysicsArray[index].GetAerodynamicSurfaces();
                ref Vector3 position = ref m_aircraftPhysicsArray[index].GetPosition();
                ref Vector3 localScale = ref m_aircraftPhysicsArray[index].GetLocalScale();
                ref Vector3 velocity = ref m_aircraftPhysicsArray[index].GetVelocity();
                ref Vector3 angularVelocity = ref m_aircraftPhysicsArray[index].GetAngularVelocity();
                ref Vector3 centerOfMass = ref m_aircraftPhysicsArray[index].GetWorldCenterOfMass();
                ref Quaternion rotation = ref m_aircraftPhysicsArray[index].GetRotation();

                for (int currSurface = 0; currSurface < m_numSurfaces; currSurface++) {
                    Vector3 relativePosition = aerodynamicSurfacePhysics[currSurface].GetWorldPosition(ref position, ref localScale, ref rotation) - centerOfMass;
                    //Vector3 relativePosition = aerodynamicSurfacePhysics[currSurface].GetRelativePosition() - centerOfMass;

                    m_surfaceInfoShader[currDim + currSurface].airVelocity = -velocity - Vector3.Cross(angularVelocity, relativePosition);
                    m_surfaceInfoShader[currDim + currSurface].relativePosition = relativePosition;
                    m_surfaceInfoShader[currDim + currSurface].rotation = aerodynamicSurfacePhysics[currSurface].GetWorldRotation(ref rotation);
                    //m_surfaceInfoShader[currDim + currSurface].rotation = aerodynamicSurfacePhysics[currSurface].GetRelativeRotation();

                    m_flapAngleShader[currDim + currSurface] = aerodynamicSurfacePhysics[currSurface].GetFlapAngle();
                }
            }
        } catch (System.Exception e) {
            Debug.LogError("Error in thread: " + e.Message);
        }

        lock (m_lockThreadsStarted) {
            m_numThreadsStarted--;
            if (m_numThreadsStarted == 0) {
                m_threadsFinished.Set();
            }
        }
    }

    public void UpdateValuesForPhysics() {
        for (int i = 0; i < m_numAircraftPhysics; i++) {
            if (m_skipAircraftPhysics[i] == 0) {
                m_aircraftPhysicsArray[i].UpdateValuesForPhysics();
            }
        }
    }

    public void SetDataInFlapAnglesComputeBuffer(ComputeBuffer flapAnglesComputeBuffer) {
        flapAnglesComputeBuffer.SetData(m_flapAngleShader);
    }

    public void SetDataInSurfaceInfoComputeBuffer(ComputeBuffer surfaceInfoComputeBuffer) {
        surfaceInfoComputeBuffer.SetData(m_surfaceInfoShader);
    }

}

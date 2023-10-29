using UnityEngine;

[CreateAssetMenu(fileName = "New Aerodynamic Surface Config", menuName = "Aerodynamic Surface Config")]
public class AerodynamicSurfaceConfig : ScriptableObject {

    // lift-curve slope of a segment of finite aspect ratio surface
    public float liftCurve = 6.28f;

    //  skin friction coefficient
    public float skinFrictionCoef = 0.02f;

    // angle of attack at which surface creates zero lift
    public float zeroLiftAoA = 0;

    // angles of attack at which stall starts
    public float stallAnglePos = 15;
    public float stallAngleNeg = -15;

    // chord of the surface
    public float flapChord = 1;

    // surface that is movable as a control surface
    public float airfoilChordFraction = 0;

    // length of the surface
    public float span = 1;

    public bool autoAspectRatio = true;
    public float aspectRatio = 2;

    private void OnValidate() {
        if (airfoilChordFraction > 0.4f)
            airfoilChordFraction = 0.4f;
        if (airfoilChordFraction < 0)
            airfoilChordFraction = 0;

        if (stallAnglePos < 0)
            stallAnglePos = 0;
        if (stallAngleNeg > 0)
            stallAngleNeg = 0;

        if (flapChord < 1e-3f)
            flapChord = 1e-3f;

        if (autoAspectRatio)
            aspectRatio = span / flapChord;
    }

}

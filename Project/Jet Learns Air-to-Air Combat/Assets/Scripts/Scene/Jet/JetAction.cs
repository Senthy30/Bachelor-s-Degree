using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetAction {

    // Attributes -------------------------------------------------------------

    private RollAction m_rollAction;
    private PitchAction m_pitchAction;
    private YawAction m_yawAction;
    private ThrustAction m_thrustAction;
    private FlapsAction m_flapsAction;
    private LaunchMissileAction m_launchMissileAction;
    private DropDecoyAction m_dropDecoyAction;

    #region Specific Attributes For Player
    
    private bool m_flapsActionWasReleased = true;
    private bool m_launchMissileActionWasReleased = true;
    private bool m_dropDecoyActionWasReleased = true;

    #endregion

    #region Getters

    // Roll Action ------------------------------------------------------------
    public RollAction GetRollAction() {
        return m_rollAction;
    }

    // Pitch Action -----------------------------------------------------------
    public PitchAction GetPitchAction() {
        return m_pitchAction;
    }

    // Yaw Action -------------------------------------------------------------
    public YawAction GetYawAction() {
        return m_yawAction;
    }

    // Thrust Action --------------------------------------------------------
    public ThrustAction GetThrustAction() {
        return m_thrustAction;
    }

    // Flaps Action -----------------------------------------------------------
    public bool GetFlapsActionWasReleased() {
        return m_flapsActionWasReleased;
    }

    public FlapsAction GetFlapsAction() {
        return m_flapsAction;
    }

    // Launch Missile Action -------------------------------------------------
    public bool GetLaunchMissileActionWasReleased() {
        return m_launchMissileActionWasReleased;
    }

    public LaunchMissileAction GetLaunchMissileAction() {
        return m_launchMissileAction;
    }

    // Drop Decoy Action ------------------------------------------------------
    public bool GetDropDecoyActionWasReleased() {
        return m_dropDecoyActionWasReleased;
    }

    public DropDecoyAction GetDropDecoyAction() {
        return m_dropDecoyAction;
    }

    #endregion

    #region Setters

    // Roll Action ------------------------------------------------------------
    public void SetRollAction(int rollAction) {
        m_rollAction = (RollAction)rollAction;
    }

    public void SetRollAction(RollAction rollAction) {
        m_rollAction = rollAction;
    }

    // Pitch Action -----------------------------------------------------------
    public void SetPitchAction(int pitchAction) {
        m_pitchAction = (PitchAction)pitchAction;
    }

    public void SetPitchAction(PitchAction pitchAction) {
        m_pitchAction = pitchAction;
    }

    // Yaw Action -------------------------------------------------------------
    public void SetYawAction(int yawAction) {
        m_yawAction = (YawAction)yawAction;
    }

    public void SetYawAction(YawAction yawAction) {
        m_yawAction = yawAction;
    }

    // Throttle Action --------------------------------------------------------
    public void SetThrustAction(int thrustAction) {
        m_thrustAction = (ThrustAction)thrustAction;
    }

    public void SetThrustAction(ThrustAction thrustAction) {
        m_thrustAction = thrustAction;
    }

    // Flaps Action -----------------------------------------------------------
    public void SetFlapsAction(int flapsAction) {
        m_flapsAction = (FlapsAction)flapsAction;
    }

    public void SetFlapsAction(FlapsAction flapsAction) {
        m_flapsAction = flapsAction;
    }

    public void SetFlapsActionWasReleased(bool flapsActionWasReleased) {
        m_flapsActionWasReleased = flapsActionWasReleased;
    }

    // Launch Missile Action -------------------------------------------------
    public void SetLaunchMissileAction(int launchMissileAction) {
        m_launchMissileAction = (LaunchMissileAction)launchMissileAction;
    }

    public void SetLaunchMissileAction(LaunchMissileAction launchMissileAction) {
        m_launchMissileAction = launchMissileAction;
    }

    public void SetLaunchMissileActionWasReleased(bool launchMissileActionWasReleased) {
        m_launchMissileActionWasReleased = launchMissileActionWasReleased;
    }

    // Drop Decoy Action ------------------------------------------------------
    public void SetDropDecoyAction(int dropDecoyAction) {
        m_dropDecoyAction = (DropDecoyAction)dropDecoyAction;
    }

    public void SetDropDecoyAction(DropDecoyAction dropDecoyAction) {
        m_dropDecoyAction = dropDecoyAction;
    }

    public void SetDropDecoyActionWasReleased(bool dropDecoyActionWasReleased) {
        m_dropDecoyActionWasReleased = dropDecoyActionWasReleased;
    }

    #endregion

    #region Action Class Enumerations
    public enum RollAction {
        None,
        Left,
        Right
    }

    public enum PitchAction {
        None,
        Up,
        Down
    }

    public enum YawAction {
        None,
        Left,
        Right
    }

    public enum ThrustAction {
        None,
        Increase,
        Decrease
    }

    public enum FlapsAction {
        None,
        Trigger
    }

    public enum LaunchMissileAction {
        None,
        Launch
    }

    public enum DropDecoyAction {
        None,
        Drop
    }
    #endregion

    #region Static Methods

    public static void CalculateHeuristicActions(JetAction jetAction) {
        // Roll
        if (Input.GetKey(KeyCode.A)) {
            jetAction.SetRollAction(RollAction.Left);
        } else if (Input.GetKey(KeyCode.D)) {
            jetAction.SetRollAction(RollAction.Right);
        } else {
            jetAction.SetRollAction(RollAction.None);
        }

        // Pitch
        if (Input.GetKey(KeyCode.W)) {
            jetAction.SetPitchAction(PitchAction.Up);
        } else if (Input.GetKey(KeyCode.S)) {
            jetAction.SetPitchAction(PitchAction.Down);
        } else {
            jetAction.SetPitchAction(PitchAction.None);
        }

        // Yaw
        if (Input.GetKey(KeyCode.Q)) {
            jetAction.SetYawAction(YawAction.Left);
        } else if (Input.GetKey(KeyCode.E)) {
            jetAction.SetYawAction(YawAction.Right);
        } else {
            jetAction.SetYawAction(YawAction.None);
        }

        // Thrust
        if (Input.GetKey(KeyCode.Space)) {
            jetAction.SetThrustAction(ThrustAction.Increase);
        } else if (Input.GetKey(KeyCode.LeftControl)) {
            jetAction.SetThrustAction(ThrustAction.Decrease);
        } else {
            jetAction.SetThrustAction(ThrustAction.None);
        }

        // Flaps
        if (Input.GetKey(KeyCode.F)) {
            if (jetAction.GetFlapsActionWasReleased()) {
                jetAction.SetFlapsAction(FlapsAction.Trigger);
                jetAction.SetFlapsActionWasReleased(false);
            } else {
                jetAction.SetFlapsAction(FlapsAction.None);
            }
        } else {
            jetAction.SetFlapsAction(FlapsAction.None);
            jetAction.SetFlapsActionWasReleased(true);
        }

        // Launch Missile
        if (Input.GetKey(KeyCode.Mouse1)) {
            if (jetAction.GetLaunchMissileActionWasReleased()) {
                jetAction.SetLaunchMissileAction(LaunchMissileAction.Launch);
                jetAction.SetLaunchMissileActionWasReleased(false);
            } else {
                jetAction.SetLaunchMissileAction(LaunchMissileAction.None);
            }
        } else {
            jetAction.SetLaunchMissileAction(LaunchMissileAction.None);
            jetAction.SetLaunchMissileActionWasReleased(true);
        }

        // Drop Decoy
        if (Input.GetKey(KeyCode.Mouse2)) {
            if (jetAction.GetDropDecoyActionWasReleased()) {
                jetAction.SetDropDecoyAction(DropDecoyAction.Drop);
                jetAction.SetDropDecoyActionWasReleased(false);
            } else {
                jetAction.SetDropDecoyAction(DropDecoyAction.None);
            }
        } else {
            jetAction.SetDropDecoyAction(DropDecoyAction.None);
            jetAction.SetDropDecoyActionWasReleased(true);
        }
    }

    public static RollAction GetRollEnumFromInputSign(float rollInputSign) {
        if (rollInputSign > 0) {
            return RollAction.Right;
        } else if (rollInputSign < 0) {
            return RollAction.Left;
        } else {
            return RollAction.None;
        }
    }

    public static PitchAction GetPitchEnumFromInputSign(float pitchInputSign) {
        if (pitchInputSign > 0) {
            return PitchAction.Up;
        } else if (pitchInputSign < 0) {
            return PitchAction.Down;
        } else {
            return PitchAction.None;
        }
    }

    public static YawAction GetYawEnumFromInputSign(float yawInputSign) {
        if (yawInputSign > 0) {
            return YawAction.Left;
        } else if (yawInputSign < 0) {
            return YawAction.Right;
        } else {
            return YawAction.None;
        }
    }

    public static ThrustAction GetThrustEnumFromInputSign(float thrustInputSign) {
        if (thrustInputSign > 0) {
            return ThrustAction.Increase;
        } else if (thrustInputSign < 0) {
            return ThrustAction.Decrease;
        } else {
            return ThrustAction.None;
        }
    }

    public static FlapsAction GetFlapsEnumFromInputSign(bool flapInputWasPressed) {
        if (flapInputWasPressed) {
            return FlapsAction.Trigger;
        } else {
            return FlapsAction.None;
        }
    }

    public static LaunchMissileAction GetLaunchMissileEnumFromInputSign(bool launchedMissileInputWasPressed) {
        if (launchedMissileInputWasPressed) {
            return LaunchMissileAction.Launch;
        } else {
            return LaunchMissileAction.None;
        }
    }

    public static DropDecoyAction GetDropDecoyEnumFromInputSign(bool dropDecoyInputWasPressed) {
        if (dropDecoyInputWasPressed) {
            return DropDecoyAction.Drop;
        } else {
            return DropDecoyAction.None;
        }
    }

    #endregion
}

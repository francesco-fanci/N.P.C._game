// N.P.C. (Null Pointer Character) - FSM for Garbage Collector enemy (Patrol -> Alert -> Purge)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCollectorFSM : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Alert,
        Purge
    }

    [Header("Current State")]
    [SerializeField]
    private State _currentState = State.Patrol;

    [Header("Patrol Waypoints")]
    [SerializeField, Tooltip("List of world transforms to patrol between in order.")]
    private List<Transform> _waypoints = new List<Transform>();

    [SerializeField, Tooltip("Patrol movement speed in m/s.")]
    private float _patrolSpeed = 2.5f;

    [SerializeField, Tooltip("Rotation speed when turning toward waypoints.")]
    private float _rotationSpeed = 5f;

    [SerializeField, Tooltip("Distance threshold to consider a waypoint reached.")]
    private float _waypointTolerance = 0.2f;

    [SerializeField, Tooltip("Wait time (seconds) at each waypoint before continuing.")]
    private float _waypointWaitTime = 1f;

    [Header("Spotlight & Detection")]
    [SerializeField, Tooltip("The forward-facing spotlight.")]
    private Light _spotlight;

    [SerializeField, Tooltip("Normal patrol light color.")]
    private Color _patrolLightColor = new Color(1f, 0.2f, 0.2f); // Crimson Red

    [SerializeField, Tooltip("Alert/Purge light color.")]
    private Color _purgeLightColor = Color.red;

    [SerializeField, Tooltip("Time in seconds of the alert sequence before resetting the player.")]
    private float _purgeDelay = 0.6f;

    // Internal patrol state
    private int _currentWaypointIndex = 0;
    private bool _isWaitingAtWaypoint = false;
    private bool _isPurging = false;
    private float _originalLightIntensity = 1f;

    private void Start()
    {
        if (_spotlight != null)
        {
            _spotlight.color = _patrolLightColor;
            _originalLightIntensity = _spotlight.intensity;
        }

        if (_waypoints.Count == 0)
        {
            Debug.LogWarning($"[GarbageCollector] \"{gameObject.name}\" has no waypoints assigned.", this);
        }
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Alert:
            case State.Purge:
                // Don't move while purging
                break;
        }
    }

    // --- Patrol Logic ---

    private void HandlePatrol()
    {
        if (_waypoints == null || _waypoints.Count == 0 || _isWaitingAtWaypoint)
            return;

        Transform target = _waypoints[_currentWaypointIndex];
        if (target == null) return;

        // Move towards the current waypoint, ignoring Y so we stay on the ground
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;

        if (direction.magnitude > _waypointTolerance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(target.position.x, transform.position.y, target.position.z),
                _patrolSpeed * Time.deltaTime
            );

            // Smoothly rotate towards movement direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // We reached the waypoint, wait before moving to next
            StartCoroutine(WaitAtWaypointRoutine());
        }
    }

    private IEnumerator WaitAtWaypointRoutine()
    {
        _isWaitingAtWaypoint = true;
        yield return new WaitForSeconds(_waypointWaitTime);

        // Cycle to the next waypoint (wraps around)
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
        _isWaitingAtWaypoint = false;
    }

    // --- Detection & Purge ---

    // Called by the GarbageCollectorDetector when player enters the spotlight cone
    public void OnPlayerDetected(GameObject player)
    {
        if (_isPurging) return;

        StartCoroutine(PurgeRoutine(player));
    }

    private IEnumerator PurgeRoutine(GameObject player)
    {
        _isPurging = true;
        _currentState = State.Alert;

        Debug.Log("<color=red><b>[Garbage Collector] ANOMALY DETECTED! INITIATING PURGE ROUTINE...</b></color>");

        // Flash spotlight red to warn the player
        if (_spotlight != null)
        {
            _spotlight.color = _purgeLightColor;
            _spotlight.intensity = _originalLightIntensity * 2f;
        }

        // Brief dramatic pause before resetting the player
        yield return new WaitForSeconds(_purgeDelay);

        // Send player back to last checkpoint
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer();
        }
        else
        {
            // Fallback if no checkpoint manager exists
            player.transform.position = new Vector3(0, 1, 0);
        }

        // Short cooldown then go back to patrolling
        yield return new WaitForSeconds(0.4f);

        if (_spotlight != null)
        {
            _spotlight.color = _patrolLightColor;
            _spotlight.intensity = _originalLightIntensity;
        }

        _isPurging = false;
        _currentState = State.Patrol;
    }

    // --- Editor Gizmos ---

    private void OnDrawGizmos()
    {
        if (_waypoints == null || _waypoints.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _waypoints.Count; i++)
        {
            if (_waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(_waypoints[i].position, 0.4f);

            int nextIndex = (i + 1) % _waypoints.Count;
            if (_waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(_waypoints[i].position, _waypoints[nextIndex].position);
            }
        }
    }
}

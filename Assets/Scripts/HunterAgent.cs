using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HunterAgent : Agent
{
    [SerializeField] private Transform animalAgent;
    [SerializeField] private float moveSpeed = 6f;

    public override void OnEpisodeBegin()
    {
        // Move the HunterAgent back to it's starting location
        this.transform.localPosition = new Vector3(2, 1, 30);

        // Reset the HunterAgent's rotation
        this.transform.localRotation = Quaternion.identity;

        // Move the AnimalAgent back to it's starting location
        animalAgent.localPosition = new Vector3(5, 1, 63);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // HunterAgent and AnimalAgent positions
        sensor.AddObservation(animalAgent.localPosition);
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers action)
    {
        float moveX = action.ContinuousActions[0];
        float moveZ = action.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        // HunterAgent fell off the platform
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AnimalAgent>(out AnimalAgent animalAgentComponent))
        {
            SetReward(+1f);
            EndEpisode();
        }
    }
}

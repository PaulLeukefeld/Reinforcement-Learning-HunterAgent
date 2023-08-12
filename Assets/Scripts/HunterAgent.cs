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
        this.transform.localPosition = new Vector3(2, 1, -17);

        // Reset the HunterAgent's rotation
        this.transform.localRotation = Quaternion.identity;

        // Move the AnimalAgent back to a random starting location
        bool validPosition = false;
        Vector3 animalPosition = Vector3.zero;
        int tries = 0;
        while (!validPosition && tries < 10)
        {
            // Generate a random position within a certain range
            animalPosition = new Vector3(Random.Range(-20f, 20f), 1, Random.Range(-20f, 20f));

            // Check if there are any colliders within a certain radius of the position
            Collider[] colliders = Physics.OverlapSphere(animalPosition, 1f);

            // Check if the position is valid
            validPosition = colliders.Length == 0;

            // Increment the number of tries
            tries++;
        }

        // Set the position of the AnimalAgent to the valid position
        animalAgent.localPosition = animalPosition;
    
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
            SetReward(-1f);
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

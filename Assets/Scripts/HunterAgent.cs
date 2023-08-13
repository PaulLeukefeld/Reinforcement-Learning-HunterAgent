using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HunterAgent : Agent
{
    [SerializeField] private Transform animalAgent;
    [SerializeField] private Transform armory;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Material hunterAgentMaterial;
    private bool hasSword = false;

    public override void OnEpisodeBegin()
    {
        // Move the HunterAgent back to it's starting location
        this.transform.localPosition = new Vector3(0, 1, -5);

        // Reset the HunterAgent's rotation
        this.transform.localRotation = Quaternion.identity;

        // Move the AnimalAgent back to it's starting location
        animalAgent.localPosition = new Vector3(0, 1, 5);

        // Reset the HunterAgent's sword status
        hasSword = false;

        // Reset the HunterAgent's material color
        this.GetComponent<MeshRenderer>().material = hunterAgentMaterial;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // HunterAgent and AnimalAgent positions
        sensor.AddObservation(animalAgent.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Armory position
        sensor.AddObservation(armory.localPosition);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AnimalAgent"))
        {
            if (hasSword)
            {
                SetReward(+1f);
                EndEpisode();
            }
            else
            {
                SetReward(-1f);
                EndEpisode();
            }
        }

        else if (collision.gameObject.CompareTag("Armory"))
        {
            hasSword = true;
            // Change the color of the agent to red to indicate that it has a sword
            this.GetComponent<MeshRenderer>().material.color = Color.red;
            SetReward(+1f);
        }
    }
}

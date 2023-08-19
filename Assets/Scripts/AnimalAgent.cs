using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AnimalAgent : Agent
{
    [SerializeField] private Transform hunterAgent;
    [SerializeField] private Transform armory;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float minRange = -15f;
    [SerializeField] private float maxRange = 15;
    [SerializeField] private Transform[] agents = new Transform[2];
    private void Start()
    {
        agents[0] = hunterAgent;
        agents[1] = this.transform;
    }

    public override void OnEpisodeBegin()
    {
        // Move the AnimalAgent back to a random starting location
        Vector3 animalAgentPosition = new Vector3(Random.Range(minRange, maxRange), 1, Random.Range(minRange, maxRange));
        while (Vector3.Distance(animalAgentPosition, armory.localPosition) < 2f)
        {
            animalAgentPosition = new Vector3(Random.Range(minRange, maxRange), 1, Random.Range(minRange, maxRange));
        }
        this.transform.localPosition = animalAgentPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // HunterAgent and AnimalAgent positions
        sensor.AddObservation(hunterAgent.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Armory position
        sensor.AddObservation(armory.localPosition);

        // HunterAgent sword observation
        sensor.AddObservation(hunterAgent.GetComponent<HunterAgent>().GetHunterAgentSwordStatus());
    }

    public override void OnActionReceived(ActionBuffers action)
    {
        float moveX = action.ContinuousActions[0];
        float moveZ = action.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        // AnimalAgent fell off the platform
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1f);
            // End the episode for all agents
            foreach (var agent in agents)
            {
                agent.GetComponent<Agent>().EndEpisode();
            }
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
            if (hunterAgent.GetComponent<HunterAgent>().GetHunterAgentSwordStatus())
            {
                SetReward(-1f);
                // End the episode for all agents
                foreach (var agent in agents)
                {
                    agent.GetComponent<Agent>().EndEpisode();
                }
            }
            else
            {
                SetReward(+1f);
                // End the episode for all agents
                foreach (var agent in agents)
                {
                    agent.GetComponent<Agent>().EndEpisode();
                }
            }
        }
    }
}

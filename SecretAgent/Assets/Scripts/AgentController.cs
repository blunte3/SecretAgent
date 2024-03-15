using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UI;

public class AgentController : Agent
{
    //Pellet variables
    [SerializeField] private Transform target;
    public int pelletCount;
    public GameObject food;
    [SerializeField] public List<GameObject> spawnedPelletsList = new List<GameObject>();

    //Agent variables
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    //Environment variables
    [SerializeField] private Transform environmentLocation;
    Material envMaterial;
    public GameObject env;

    //Time keeping variables
    [SerializeField] private int timeForEpisode;
    private float timeLeft;
    //private float rewardTimer = 0f;
    //private const float rewardInterval = 1f;

    //Secret Agent
    public SecretAgentController classObject;
    [SerializeField] private float agentTouchRadius = 1.5f;

    // Hunger level variables
    public float maxHunger = 100f;
    public float hungerDecreaseRate = 1f;
    public float hungerIncreaseAmount = 20f;
    public float currentHunger = 0f;

    // UI variables
    public Slider hungerSlider;

    public bool IsSecret(){
        return false;
    }
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public float getHunger(){
        return currentHunger;
    }
    public override void OnEpisodeBegin()
    {
        //Agent
        transform.localPosition = new Vector3(Random.Range(-4f,4f), 0.4f, Random.Range(-4f, 4f));

        //Pellet
        // CreatePellet();

        //Timer to determine if agent is taking too long
        EpisodeTimerNew();

        // Initialize hunger level
        currentHunger = maxHunger;
        UpdateHungerUI();
    }

    private void Update()
    {
        CheckRemainingTime();

        // Decrease hunger level over time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        UpdateHungerUI();

        // Check if the agent is too hungry
        if (currentHunger <= 0f)
        {
            envMaterial.color = Color.red;
            AddReward(-20f);
            classObject.AddReward(20f);
            EndEpisode();
            classObject.EndEpisode();
        }
    }

    private void CreatePellet()
    {
        distanceList.Clear();
        badDistanceList.Clear();

        if(spawnedPelletsList.Count != 0)
        {
            RemovePellet(spawnedPelletsList);
        }

        for(int i = 0; i < pelletCount; i++)
        {
            int counter = 0;
            bool distanceGood;
            bool alreadyDecremented = false;

            //Spawning pellet
            GameObject newPellet = Instantiate(food);
            //Make pellet child of the environment
            newPellet.transform.parent = environmentLocation;
            //Give random spawn location
            Vector3 pelletLocation = new Vector3(Random.Range(-4f, 4f), 0.4f, Random.Range(-4f, 4f));

            if(spawnedPelletsList.Count != 0)
            {
                for(int k = 0; k < spawnedPelletsList.Count; k++)
                {
                    if(counter < 10)
                    {
                        distanceGood = CheckOverlap(pelletLocation, spawnedPelletsList[k].transform.localPosition, 5f);
                        if(distanceGood == false)
                        {
                            pelletLocation = new Vector3(Random.Range(-4f, 4f), 0.4f, Random.Range(-4f, 4f));
                            k--;
                            alreadyDecremented = true;
                        }

                        distanceGood = CheckOverlap(pelletLocation, transform.localPosition, 5f);
                        if (distanceGood == false)
                        {
                            pelletLocation = new Vector3(Random.Range(-4f, 4f), 0.4f, Random.Range(-4f, 4f));
                            if(alreadyDecremented == false)
                            {
                                k--;
                            }
                        }

                        counter++;
                    }
                    else
                    {
                        k = spawnedPelletsList.Count;
                    }
                }
            }

            //Spawn in new location
            newPellet.transform.localPosition = pelletLocation;
            //Add to list
            spawnedPelletsList.Add(newPellet);
        }
    }

    public List<float> distanceList = new List<float>();
    public List<float> badDistanceList = new List<float>();

    public bool CheckOverlap(Vector3 objectWeWantToAvoidOverlapping, Vector3 alreadyExistingObject, float minDistanceWanted)
    {
        float DistanceBetweenObjects = Vector2.Distance(objectWeWantToAvoidOverlapping, alreadyExistingObject);
        if(minDistanceWanted <= DistanceBetweenObjects)
        {
            distanceList.Add(DistanceBetweenObjects);        
            return true;
        }
        badDistanceList.Add(DistanceBetweenObjects);
        return false;
    }
    private void RemovePellet(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach(GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
        }
        toBeDeletedGameObjectList.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);

        /*
        Vector3 velocity = new Vector3(moveX, 0f, moveZ);
        velocity = velocity.normalized * Time.deltaTime * moveSpeed;

        transform.localPosition += velocity;
        */
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Pellet")
        {
            //Remove from list
            spawnedPelletsList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(5f);
            classObject.AddReward(-5f);

            // Increase hunger level when eating a pellet
            currentHunger += hungerIncreaseAmount;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger); // Ensure hunger level doesn't exceed max
            UpdateHungerUI();

            // Remove pellet
            // Destroy(other.gameObject);
        }
        if (other.gameObject.tag == "Wall")
        {
            // RemovePellet(spawnedPelletsList);
            if (IsTouchingSecretAgent())
            {
                envMaterial.color = Color.yellow;
                classObject.AddReward(100f);
                AddReward(-25f);
            }
            else
            {
                envMaterial.color = Color.black;
                AddReward(-25f);
            }
            classObject.EndEpisode();
            EndEpisode();
        }
    }

    private bool IsTouchingSecretAgent()
    {
        // Check if any colliders of the SecretAgent are touching the Agent
        Collider[] colliders = Physics.OverlapSphere(transform.position, agentTouchRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("SecretAgent"))
            {
                return true;
            }
        }
        return false;
    }
    private void UpdateHungerUI()
    {
        // Update UI element to reflect hunger level
        hungerSlider.value = currentHunger / maxHunger;
    }

    private void EpisodeTimerNew()
    {
        timeLeft = Time.time + timeForEpisode;
    }

    // Method to remove a pellet from the list
    public void RemovePellet(GameObject pellet)
    {
        spawnedPelletsList.Remove(pellet);
    }

    private void CheckRemainingTime()
    {
        if(Time.time >= timeLeft)
        {
            envMaterial.color = Color.blue;
            AddReward(-15f);
            classObject.AddReward(20f);
            // RemovePellet(spawnedPelletsList);
            classObject.EndEpisode();
            EndEpisode();
        }
    }
}

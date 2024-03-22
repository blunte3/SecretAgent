using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EnvScript : MonoBehaviour
{
    //pellet variables
    [SerializeField] private Transform target;
    public int pelletCountFactor = 10;
    public int pelletCount = 0;
    [SerializeField] public GameObject food;
    //store pellets in this list
     [SerializeField] public List<GameObject> spawnedPelletsList = new List<GameObject>();
    public List<float> distanceList = new List<float>();
    public List<float> badDistanceList = new List<float>();

    //agent tracking
    public GameObject agent;
    [SerializeField] public List<AgentController> spawnedAgentsList = new List<AgentController>();

    //env variables
    [SerializeField] private Transform environmentLocation;
    Material envMaterial;
    public GameObject env;
    void Awake(){
        //initialize 
        AddAgent();//add all the agents to the list first
        CreatePellet();//now that the agents are all in the lists, create pellets based on that
        food = GameObjecy.Find(spawnedAgentsList[0]);
        CheckAgentHunger();
    }

    private void AddAgent(){
        // Find all instances of the AgentController script in the scene
        spawnedAgentsList = FindObjectsOfType<AgentController>().ToList();
        // Add the found agents to the spawnedAgentsList
    }

    //loop through the list of Agents, checking all their hunger
    //if hunger <= 0, delete them from list and delete the object

    private void CheckAgentHunger(){
        for(int i = 0; i < spawnedAgentsList.Count; i++){
            if(spawnedAgentsList[i].getHunger() <= 0){
                Destroy(spawnedAgentsList[i].gameObject);
                spawnedAgentsList.RemoveAt(i);
                i--;
            }
        }
    }

    private void CreatePellet()
    {
        distanceList.Clear();
        badDistanceList.Clear();
        pelletCount = pelletCountFactor*spawnedAgentsList.Count;
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

        // Update is called once per frame
    void Update()
    {
        CheckAgentHunger();
        CreatePellet();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstaclesController : MonoBehaviour
{
    [SerializeField] List<Transform> Obstacles;
    [SerializeField] Transform Cube;
    [SerializeField] Transform RampBlock;
    private GameObject player;
    private ObstacleType previousObsType;
    private bool first = true;
    private int obstaclesCounter;
    [SerializeField] int maxObstacles;
    [SerializeField] float firstObsZPozition;
    private float previousObsZPosition;
    private int previousObsHeight;

    enum ObstacleType { Cube, Ramp }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        PlaceObstacle();
        if (obstaclesCounter == maxObstacles && player.transform.position.z > Obstacles[5].position.z) RemoveObstacle(Obstacles[0]);
    }

    private void PlaceObstacle()
    {
        if (obstaclesCounter >= maxObstacles) return;
        int number = Random.Range(0, Enum.GetNames(typeof(ObstacleType)).Length);
        Transform newObject;

        //Если рампа, позиция нового +6 / +7

        if (first)
        {
            newObject = GenerateObstacle(2, (ObstacleType)number).transform;
            Obstacles.Add(newObject);
            newObject.position = new Vector3(newObject.position.x, newObject.position.y, firstObsZPozition);
            previousObsHeight = 2;
            first = false;
        }
        else
        {
            int taller = Random.Range(0, 2);
            int newHeight = taller > 0 || previousObsHeight < 4 ? Random.Range(previousObsHeight, previousObsHeight + 2) 
                : Random.Range(previousObsHeight - 3, previousObsHeight);
            previousObsHeight = newHeight;
            newObject = GenerateObstacle(newHeight, (ObstacleType)number).transform;
            Obstacles.Add(newObject);
            newObject.position = new Vector3(newObject.position.x, newObject.position.y, 
            previousObsType == ObstacleType.Ramp ? previousObsZPosition + Random.Range(6f, 7f) : previousObsZPosition + Random.Range(2f, 2.6f));
        }
        previousObsZPosition = newObject.position.z;
        previousObsType = (ObstacleType)number;

        obstaclesCounter++;
    }

    private GameObject GenerateObstacle(int height, ObstacleType obstacle)
    {
        GameObject parentObj = new GameObject();
        switch (obstacle)
        {
            case ObstacleType.Cube:
                Transform newObject = Instantiate(Cube);
                newObject.parent = parentObj.transform;
                newObject.localScale = new Vector3(1, 0.5f * height, 1);
                newObject.localPosition = new Vector3(newObject.position.x, 0.25f * height, newObject.position.z);
                break;

            case ObstacleType.Ramp:
                Transform ramp = Instantiate(RampBlock);
                ramp.parent = parentObj.transform;
                ramp.localPosition = new Vector3(ramp.localPosition.x, 0.5f * height + 0.15f, 2.25f);
                Transform newObject1 = Instantiate(Cube);
                newObject1.parent = parentObj.transform;
                newObject1.localScale = new Vector3(1, 0.5f * height, 2);
                newObject1.localPosition = new Vector3(newObject1.position.x, 0.25f * height, newObject1.position.z);
                Transform newObject2 = Instantiate(Cube);
                newObject2.parent = parentObj.transform;
                newObject2.localScale = new Vector3(1, 0.5f * height + 0.5f, 1);
                newObject2.localPosition = new Vector3(newObject1.position.x, (0.5f * height + 0.5f) / 2, newObject1.position.z + 4);
                break;
        }
        return parentObj;
    }

    private void RemoveObstacle(Transform obstacle)
    {
        Destroy(obstacle.gameObject);
        Obstacles.Remove(obstacle);
        obstaclesCounter--;
    }

}

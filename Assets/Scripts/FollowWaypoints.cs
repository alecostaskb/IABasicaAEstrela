using UnityEngine;

public class FollowWaypoints : MonoBehaviour
{
    private Transform goal;
    private float speed = 5.0f;
    private float accuracy = 5.0f;
    private float rotationSpeed = 2.0f;
    private GameObject[] waitPoints;
    private GameObject currentNode;
    private int currentWP = 0;
    private Graph graph;

    public GameObject wpManager;

    private void Start()
    {
        Time.timeScale = 5.0f;
        waitPoints = wpManager.GetComponent<WPManager>().waypoints;
        graph = wpManager.GetComponent<WPManager>().graph;
        currentNode = waitPoints[0];
    }

    private void LateUpdate()
    {
        if (graph.pathList.Count == 0 || currentWP == graph.pathList.Count)
        {
            return;
        }

        currentNode = graph.getPathPoint(currentWP);

        if (Vector3.Distance(graph.pathList[currentWP].getID().transform.position, transform.position) < accuracy)
        {
            currentWP++;
        }

        if (currentWP < graph.pathList.Count)
        {
            goal = graph.pathList[currentWP].getID().transform;
            Vector3 lookAtGoal = new Vector3(
                goal.position.x,
                transform.position.y,
                goal.position.z);

            Vector3 direction = lookAtGoal - transform.position;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * rotationSpeed);

            transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
        }
    }

    public void GotoHeli()
    {
        graph.AStar(currentNode, waitPoints[0]);

        currentWP = 0;
    }

    public void GotoRuin()
    {
        graph.AStar(currentNode, waitPoints[7]);

        currentWP = 0;
    }

    public void GotoRock()
    {
        graph.AStar(currentNode, waitPoints[1]);

        currentWP = 0;
    }

    public void GotoFactory()
    {
        graph.AStar(currentNode, waitPoints[4]);

        currentWP = 0;
    }
}
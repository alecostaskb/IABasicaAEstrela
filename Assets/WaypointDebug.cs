using UnityEngine;

[ExecuteInEditMode]
public class WaypointDebug : MonoBehaviour
{
    private void RenameWPs(GameObject overlook)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("wp");
        int i = 1;

        foreach (GameObject go in gos)
        {
            if (go != overlook)
            {
                go.name = "WP" + string.Format("{0:000}", i);

                i++;
            }
        }
    }

    private void OnDestroy()
    {
        RenameWPs(gameObject);
    }

    // Use this for initialization
    private void Start()
    {
        if (transform.parent.gameObject.name != "WayPoint")
        {
            return;
        }

        RenameWPs(null);
    }

    // Update is called once per frame
    private void Update()
    {
        GetComponent<TextMesh>().text = transform.parent.gameObject.name;
    }
}
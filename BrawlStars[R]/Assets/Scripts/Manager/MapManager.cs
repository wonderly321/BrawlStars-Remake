using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private List<int> mapInfo = new List<int>();

    [Header("Prefabs")]
    public GameObject grass;
    public GameObject wall;
    public GameObject Terrain;
    public GameObject box;

    // Start is called before the first frame update
    void Start()
    {
        TextAsset txt = Resources.Load("map") as TextAsset;
        string[] strs = txt.text.Split('\n');
        foreach (string str in strs)
        {
            var line = str.Trim().Split(' ');
            foreach(string s in line)
            {
                int t = int.Parse(s);
                mapInfo.Add(t);
            }
        }
        // 0: ground 1: grass 2: wall
        SpawnMap();
        GameManager.CallNet("spawn_box", new object[]{GameManager.user_id});
    }

    void SpawnMap()
    {
        int n = (int)Mathf.Sqrt(mapInfo.Count);
        for(int x = 0; x < n; x++) 
        {
            for(int y = 0; y < n; y++) 
            {
                int idx = x * n + y;
                int type = mapInfo[idx];
                if(type == 1)
                {
                    Instantiate(grass, new Vector3(-15.5f+x, 0, 15.5f-y), Quaternion.identity, Terrain.transform);
                }
                else if(type == 2)
                {
                    Instantiate(wall, new Vector3(-15.5f+x, 0, 15.5f-y), Quaternion.identity, Terrain.transform);
                }
            }
        }
    }

    public void AddBox(float x, float y)
    {
        Instantiate(box, new Vector3(x, 0, y), Quaternion.identity, Terrain.transform);
    } 
}

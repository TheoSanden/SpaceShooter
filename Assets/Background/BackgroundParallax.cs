using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BackgroundLayer 
{
    public float ScrollSpeed;
    public GameObject BackgroundObject;
}



public class BackgroundParallax : MonoBehaviour
{
    [SerializeField]
    BackgroundLayer[] Layers;

    Vector2 CameraBounds;
    Dictionary<BackgroundLayer, List<GameObject>> BackgroundObjectsSet = new Dictionary<BackgroundLayer, List<GameObject>>();

    void Start()
    {
        CameraBounds = new Vector2(Camera.main.orthographicSize * (float)(16.0 / 9.0), Camera.main.orthographicSize);
        foreach (BackgroundLayer Layer in Layers)
        {
            List<GameObject> BackgroundList = new List<GameObject>();
            for(float i = -CameraBounds.y *2; i < CameraBounds.y; i += CameraBounds.y * 2) 
            {
                BackgroundList.Add(Instantiate(Layer.BackgroundObject,new Vector3(0,i,0),Quaternion.identity));
            }
            BackgroundObjectsSet.Add(Layer, BackgroundList);
        }
    }

    
    void Update()
    {
        foreach(BackgroundLayer Layer in Layers) 
        {
            foreach(GameObject go in BackgroundObjectsSet[Layer]) 
            {
                go.transform.position += Layer.ScrollSpeed * Time.deltaTime * -Vector3.up;
                if(go.transform.position.y <= -(CameraBounds.y * 2)) 
                {
                    go.transform.position = new Vector3(0,CameraBounds.y * 2,0);
                }
            }
        }
    }
}

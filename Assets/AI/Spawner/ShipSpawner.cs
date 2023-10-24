using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEditor;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    // Start is called before the first frame updat
    [SerializeField]
    GameObject Ship_Prefab;

    [SerializeField]
    ObjectPooler<GameObject> Ship_Basic_Pooler = new ObjectPooler<GameObject>();

    [SerializeField]
    Vector2 VPatternBounds = new Vector2(100,50);
    [SerializeField]
    int ShipAmountPerVPattern = 5;

    Vector2 CameraBounds;


    float ShipSpawnTime = 3;
    float ShipSpawnTimer = 0;
    void Start()
    {
        Ship_Basic_Pooler.Initialize(Ship_Prefab,OnPop, OnQueue);
        CameraBounds = new Vector2(Camera.main.orthographicSize * (float)(16.0/9.0),Camera.main.orthographicSize);
        SpawnBasicShipInVPattern();
    }

    // Update is called once per frame
    void Update()
    {
        ShipSpawnTimer += Time.deltaTime;

        if (ShipSpawnTimer >= ShipSpawnTime) 
        {
            SpawnBasicShipInVPattern();
            ShipSpawnTimer = 0;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector2 SpawnBoundsCenter = (Vector2)Camera.main.transform.position + new Vector2(0, CameraBounds.y + VPatternBounds.y);
        Gizmos.DrawWireSphere(SpawnBoundsCenter, 10);

        Gizmos.color = Color.blue;
        Vector2 AnchorPoint = SpawnBoundsCenter + new Vector2(-VPatternBounds.x, VPatternBounds.y);
        Gizmos.DrawWireSphere(AnchorPoint, 10);

        
        Handles.matrix = transform.localToWorldMatrix;
        Vector3[] points = new Vector3[5]{SpawnBoundsCenter + new Vector2(-VPatternBounds.x,VPatternBounds.y),
                                          SpawnBoundsCenter + new Vector2(VPatternBounds.x,VPatternBounds.y),
                                          SpawnBoundsCenter + new Vector2(VPatternBounds.x,-VPatternBounds.y),
                                          SpawnBoundsCenter + new Vector2(-VPatternBounds.x,-VPatternBounds.y),
                                          SpawnBoundsCenter + new Vector2(-VPatternBounds.x,VPatternBounds.y)};
        Handles.DrawAAPolyLine(10,points);
    }

    void SpawnBasicShipInVPattern() 
    {
        float CameraBoundsDeltaX = CameraBounds.x - VPatternBounds.x;
        Vector2 SpawnBoundsCenter = (Vector2)Camera.main.transform.position + new Vector2(Random.Range(-CameraBoundsDeltaX,CameraBoundsDeltaX),CameraBounds.y + VPatternBounds.y);
        Vector2 AnchorPoint = SpawnBoundsCenter + new Vector2(-VPatternBounds.x,VPatternBounds.y);

        float XPosition = 0;
        float YPosition = 0;


        for (int i = 0; i < ShipAmountPerVPattern; i++) 
        {
            XPosition = AnchorPoint.x + ((float)i /(float)ShipAmountPerVPattern) * (VPatternBounds.x * 2);

            int IndexPivotPoint = 0;
            if (ShipAmountPerVPattern % 2 == 0) 
            {
                //if ship amount per v pattern is uneven
                IndexPivotPoint =  (ShipAmountPerVPattern / 2);

                float YIndex = i % IndexPivotPoint;

                if (i > IndexPivotPoint -1) 
                {
                    YIndex = IndexPivotPoint - YIndex-1;
                }
                YPosition = AnchorPoint.y - ((VPatternBounds.y * 2) / (ShipAmountPerVPattern/2)) * YIndex;
            }
            else 
            {
                //if ship amount per v pattern is uneven
                IndexPivotPoint = ((ShipAmountPerVPattern + 1) / 2);
                float YIndex = i % IndexPivotPoint;
                if (i > IndexPivotPoint-1) 
                {
                    YIndex = IndexPivotPoint - YIndex -2;
                }

                YPosition = AnchorPoint.y - ((VPatternBounds.y * 2) / (((ShipAmountPerVPattern - 1) / 2) + 1)) * YIndex;
            }

            Vector2 SpawnPosition = new Vector2(XPosition, YPosition);

            Ship_Basic Ship = Ship_Basic_Pooler.Pop().GetComponent<Ship_Basic>();
            Ship.transform.position = SpawnPosition;
            Ship.transform.parent = this.transform;
            Ship.transform.rotation = Ship_Prefab.transform.rotation;
            Vector2 YLength = (Vector2.up * ((VPatternBounds.y * 2) + (CameraBounds.y * 2)));
            Vector2 MoveToPosition = SpawnPosition - YLength;
            Ship.SetInitialPosition(MoveToPosition);
        }
    }
    public void HandleDestroyedShip(GameObject gameObject) 
    {
        Ship_Basic_Pooler.Enqueu(gameObject);
    }
    void OnPop(GameObject ship) 
    {
        ship.GetComponent<Ship>().OnPop();
    }
    void OnQueue(GameObject ship) 
    {
        ship.GetComponent<Ship>().OnQueue();
    }
}

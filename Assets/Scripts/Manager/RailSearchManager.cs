using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RailSearchManager : MonoBehaviour
{
    public static RailSearchManager instance;
    public Shelf[] allRails;
    public List<Shelf> railsInRange;
    public List<ZRowList> railList = new List<ZRowList>();
    public float areaSize = 20;

    int numberOfXRows;
    int numberOfZRows;
    Transform levelDimensions
    {
        get
        {
            return this.gameObject.transform;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    void Start()
    {
        SearchForAllRails();
    }

    public void SearchForAllRails()
    {
        numberOfXRows = (int)Mathf.Ceil(levelDimensions.localScale.x / areaSize);
        numberOfZRows = (int)Mathf.Ceil(levelDimensions.localScale.z / areaSize);
        allRails = GameObject.FindObjectsOfType<Shelf>();
        Vector2 startPos = levelDimensions.position;
        railList.Clear();

        for (int i = 0; i < numberOfZRows; i++)
        {
            var newList = new ZRowList();
            railList.Add(newList);
            for (int e = 0; e < numberOfXRows; e++)
            {
                railList[i].xRows.Add(new XRowList());
            }
        }

        foreach (Shelf rail in allRails)
        {
            int distanceZ = (int)Mathf.Ceil((levelDimensions.position.z - rail.transform.position.z) / areaSize);
            int rowIndexZ = (numberOfZRows / 2) + distanceZ;

            int distanceX = (int)Mathf.Ceil((levelDimensions.position.x - rail.transform.position.x) / areaSize);
            int rowIndexX = (numberOfXRows / 2) + distanceX;
            railList[rowIndexZ].xRows[rowIndexX].rails.Add(rail);
        }
    }

    public void CheackForRailsInRange(Transform playerPosition)
    {
        int distanceZplayer = (int)Mathf.Ceil((levelDimensions.position.z - playerPosition.position.z) / areaSize);
        int rowIndexZplayer = numberOfZRows / 2 + distanceZplayer;

        int distanceXplayer = (int)Mathf.Ceil((levelDimensions.position.x - playerPosition.position.x) / areaSize);
        int rowIndexXplayer = numberOfXRows / 2 + distanceXplayer;

        railsInRange.Clear();
        foreach (Shelf rail in railList[rowIndexZplayer].xRows[rowIndexXplayer].rails)
        {
            railsInRange.Add(rail);
        }

        if ((float)distanceXplayer - (levelDimensions.position.x - playerPosition.position.x) / areaSize >= 0.5f)
        {
            if (rowIndexXplayer + 1 > railList[rowIndexZplayer].xRows.Count)
            {
                return;
            }

            foreach (Shelf rail in railList[rowIndexZplayer].xRows[rowIndexXplayer + 1].rails)
            {
                railsInRange.Add(rail);
            }
        }
        else
        {
            if (0 > rowIndexXplayer - 1)
            {
                return;
            }

            foreach (Shelf rail in railList[rowIndexZplayer].xRows[rowIndexXplayer - 1].rails)
            {
                railsInRange.Add(rail);
            }
        }

        if ((float)distanceZplayer - (levelDimensions.position.z - playerPosition.position.z) / areaSize >= 0.5f)
        {
            if (rowIndexZplayer + 1 > railList.Count)
            {
                return;
            }

            foreach (Shelf rail in railList[rowIndexZplayer + 1].xRows[rowIndexXplayer].rails)
            {
                railsInRange.Add(rail);
            }
        }
        else
        {
            if (0 > rowIndexXplayer - 1)
            {
                return;
            }

            foreach (Shelf rail in railList[rowIndexZplayer - 1].xRows[rowIndexXplayer].rails)
            {
                railsInRange.Add(rail);
            }
        }
    }
}

[System.Serializable]
public class XRowList
{
    public List<Shelf> rails = new List<Shelf>();
}

[System.Serializable]
public class ZRowList
{
    public List<XRowList> xRows = new List<XRowList>();
}

[CustomEditor(typeof(RailSearchManager))]
public class RailSearchManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RailSearchManager railSearchManager = (RailSearchManager)target;
        if (GUILayout.Button("Search for all rails"))
        {
            railSearchManager.SearchForAllRails();
        }
    }
}

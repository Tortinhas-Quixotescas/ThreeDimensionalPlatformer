using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    public enum CheckPointType
    {
        Common,
        Final
    }

    private CheckPointType type;

    private void Start()
    {
        this.type = CheckPointType.Common;
        Transform tenthChild = this.transform.GetChild(7);
        tenthChild.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool hasBeenVisited = MainManager.Instance.currentLevel.SetCheckPointAsVisited(this.gameObject);
            if (!hasBeenVisited)
            {
                this.SetAsShiny();
            }

            if (this.type == CheckPointType.Final)
            {
                MainManager.Instance.LoadNextLevel();
            }
        }
    }

    public void SetAsShiny()
    {
        Transform light = transform.GetChild(7);
        light.gameObject.SetActive(true);
    }

    public void SetAsFinal()
    {
        this.type = CheckPointType.Final;
        for (int i = 0; i < 7; i++)
        {
            Transform child = this.transform.GetChild(i);
            child.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.75f, 0.1f, 0.1f, 0.5f);
        }
        Transform light = this.transform.GetChild(7);
        light.GetComponent<Light>().color = new Color(1f, 0.1f, 0.2f, 1f);
    }
}

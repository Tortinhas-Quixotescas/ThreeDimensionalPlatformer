using UnityEngine;

public class BossActivator : MonoBehaviour
{
    public GameObject boss;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            MainManager.Instance.currentLevelData.SetFinalCheckPointActive(false);
            this.boss.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }

}

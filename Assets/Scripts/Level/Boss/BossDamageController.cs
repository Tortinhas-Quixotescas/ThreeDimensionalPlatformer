using UnityEngine;

public class BossDamageController : MonoBehaviour
{
    public BossFightController boss;

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            this.boss.DamageBoss();
            MainManager.Instance.playerController.Bounce();
        }
    }

}

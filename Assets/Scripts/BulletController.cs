using UnityEngine;

// 子弹控制器
public class BulletController : MonoBehaviour
{
    void Update()
    {
        DistoryWhileOutOfScreen();
    }

    private void DistoryWhileOutOfScreen()
    {
        if (this.transform.position.y < -6f)
        {
            Destroy(gameObject, 0f);
        }
    }
}

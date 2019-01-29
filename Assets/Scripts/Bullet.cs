using UnityEngine;

public class Bullet : MonoBehaviour
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

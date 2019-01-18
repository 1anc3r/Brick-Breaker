using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DistoryWhileOutOfScreen();
    }

    private void OnCollisionEnter(Collision collision)
    {

    }

    private void DistoryWhileOutOfScreen()
    {
        if (this.transform.position.y < -6f)
        {
            GameObject.Destroy(gameObject, 0f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private int score = 10;
    private int layer = 0;
    private enum BlockType
    {
        Normal = 0,
        Expansion = 1,
        Explode = 2,
        Frozen = 3,
    }
    private BlockType type;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (type == BlockType.Normal)
        {
            gameObject.GetComponentInChildren<TextMesh>().text = score.ToString();
            gameObject.GetComponent<MeshRenderer>().material.color = new Color32(33, 150, 243, 255);
        }
        else if (type == BlockType.Expansion)
        {
            gameObject.GetComponentInChildren<TextMesh>().text = "+";
            gameObject.GetComponentInChildren<TextMesh>().color = new Color32(42, 40, 40, 255);
            gameObject.GetComponent<MeshRenderer>().material.color = new Color32(255, 235, 59, 255);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == "Bullet Ball")
        {
            Camera.main.GetComponent<GameController>().AddScore(1);
            DistoryWhileOutOfScore();
        }
    }

    public void Init(byte type, int score)
    {
        this.type = (BlockType)type;
        this.score = score;
    }

    public void Press()
    {
        layer++;
        if(layer == 13)
        {
            Camera.main.GetComponent<GameController>().GameOver();
        }
    }

    public int GetScore()
    {
        return score;
    }

    private void DistoryWhileOutOfScore()
    {
        if (--score == 0)
        {
            if(type == BlockType.Expansion)
            {
                Camera.main.GetComponent<GameController>().AddCapacity();
            }
            GameObject.Destroy(gameObject, 0f);
        }
    }
}

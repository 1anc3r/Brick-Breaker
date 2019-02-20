using UnityEngine;

// 砖块控制器
public class BlockController : MonoBehaviour
{
    private int score = 10;
    private int layer = 0;
    private BlockType type;

    private TextMesh textMesh;
    private MeshRenderer rendErer;
    private GameController gameController;

    void Start()
    {
        textMesh = gameObject.GetComponentInChildren<TextMesh>();
        rendErer = gameObject.GetComponent<MeshRenderer>();
        gameController = Camera.main.GetComponent<GameController>() as GameController;
    }

    void Update()
    {
        if (type == BlockType.Normal)
        {
            textMesh.text = score.ToString();
            rendErer.material.color = new Color32(33, 150, 243, 255);
        }
        else if (type == BlockType.Expansion)
        {
            textMesh.text = "+";
            textMesh.color = new Color32(42, 40, 40, 255);
            rendErer.material.color = new Color32(255, 235, 59, 255);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == "Bullet Ball")
        {
            gameController.AddScore(1);
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
        if (layer == 13)
        {
            gameController.GameOver();
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
            if (type == BlockType.Expansion)
            {
                gameController.AddCapacity();
            }
            Destroy(gameObject, 0f);
        }
    }
}

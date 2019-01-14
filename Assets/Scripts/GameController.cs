using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    // private Transform transform;
    public Font font;
    public GameObject BulletBall; // BulletBall预制体
    public GameObject BlockBall; // BlockBall预制体
    public GameObject BlockCube; // BlockCube预制体
    private Image background; // Background预制体
    private int frame = 0;
    private int score = 0; // 游戏分数
    private int layer = 0; // 层数
    private int capacity = 5; // 弹容量
    private int quantity = 5; // 弹余量
    private enum GameStatus
    {
        Dead = 0,
        Alive = 1
    }
    private GameStatus status = GameStatus.Dead; // 游戏状态
    private bool isLaunching = false; // 正在发射
    private bool isReloading = false; // 正在装填
    private LayerMask layerMask; // 层遮罩，用于瞄准
    private Vector3 launcher = new Vector3(0, 4.1f, 0); // 发射架
    private Vector3 direction = Vector3.zero; // 发射方向
    private List<GameObject> bulletBalls;
    private List<GameObject> blocks;

    // Start is called before the first frame update
    void Start()
    {
        // transform = Camera.main.transform;
        layerMask = 1 << (LayerMask.NameToLayer("Plane"));
        bulletBalls = new List<GameObject>();
        blocks = new List<GameObject>();
		setBackground(Path.Combine (Application.streamingAssetsPath, "Background.jpg"));
        gamePlay();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameCheck())
        {
            if (!isReloading)
            {
                if (!isLaunching)
                {
                    aim();
                }
                else
                {
                    if (frame % 10 == 0)
                    {
                        launchOneBall();
                    }
                }
            }
            else
            {
                pressAllBlocks();
                placeAllBlocks();
            }
            frame = (frame == 1200) ? 0 : ++frame;
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.font = font;
        style.fontSize = 40;
        style.alignment = (TextAnchor)TextAlignment.Center;
        style.fontStyle = FontStyle.BoldAndItalic;
        style.normal.textColor = Color.white;
        Vector3 launcherCopy = Camera.main.WorldToScreenPoint(launcher);
        GUI.Label(new Rect(launcherCopy.x - 50, Screen.height - launcherCopy.y - 50, 100, 50), "" + quantity, style);
        if (GUI.Button(new Rect(launcherCopy.x - 50, 10, 100, 50), "Score : " + score, style))
        {
            gameOver();
            gamePlay();
        }
        if (status == GameStatus.Dead)
        {
            if (GUI.Button(new Rect(0, Screen.height / 2 - 25, Screen.width, 50), "Replay", style))
            {
                gamePlay();
            }
        }
    }

    // 游戏开始
    public void gamePlay()
    {
        score = 0;
        frame = 0;
        isLaunching = false;
        isReloading = false;
        status = GameStatus.Alive;
        placeAllBlocks();
    }

    // 游戏结束
    public void gameOver()
    {
        status = GameStatus.Dead;
        clearAllBalls();
        clearAllBlocks();
    }

    private void clearAllBalls()
    {
        foreach (GameObject bulletBall in bulletBalls)
        {
            if (bulletBall != null)
            {
                GameObject.Destroy(bulletBall);
            }
        }
        bulletBalls.Clear();
    }

    private int clearAllBlocks()
    {
        int score = 0;
        foreach (GameObject block in blocks)
        {
            if (block != null)
            {
                score += block.GetComponent<Block>().getScore();
                GameObject.Destroy(block);
            }
        }
        blocks.Clear();
        return score;
    }

    // 检测游戏状态
    public bool gameCheck()
    {
        if (status == GameStatus.Alive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 增加分数
    public void addScore(int score)
    {
        this.score += score;
    }

    // 增加弹容量
    public void addCapacity()
    {
        capacity++;
    }

    // 设置背景
    public void setBackground (string path)
    {
        StartCoroutine(setBackgroundAsync (path));
    }

    // 终端需要使用WWW来加载图片
    private IEnumerator setBackgroundAsync (string url)
    {
        WWW www = new WWW (url);
        yield return www;
        Texture2D texture = www.texture;
        background = GameObject.Find("Background Image").GetComponent<Image>();
        background.rectTransform.sizeDelta = new Vector2 (Screen.width, Screen.height);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        background.sprite = sprite;
    }

    // 瞄准
    private void aim()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
#elif UNITY_IPHONE || UNITY_ANDROID
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
        {
            isLaunching = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask.value))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);
                direction = (hitInfo.point - launcher).normalized;
                direction.z = 0;
            }
        }
    }

    // 重新装填
    private void reload()
    {
        frame = 1;
        quantity = capacity;
    }

    // 发射一个球
    private void launchOneBall()
    {
        if (quantity > 0)
        {
            GameObject bulletBall = GameObject.Instantiate(BulletBall, launcher, Quaternion.identity);
            bulletBall.GetComponent<Rigidbody>().AddForce(direction * 512f);
            bulletBall.transform.name = "Bullet Ball";
            bulletBalls.Add(bulletBall);
            quantity--;
        }
        else
        {
            checkAllBalls();
        }
    }

    // 检测所有球是否在屏幕上
    private void checkAllBalls()
    {
        foreach (GameObject bulletBall in bulletBalls)
        {
            if (bulletBall != null)
            {
                return;
            }
        }
        bulletBalls.Clear();
        isLaunching = false;
        isReloading = true;
        reload();
    }

    // 放置一堆砖
    private void placeAllBlocks()
    {
        float range = Mathf.Abs(Random.Range(-3f, 3f));
        if (range > 0 && range <= 1.25)
        {
            placeOneBlock(1.2f, 1.85f);
        }
        if (range > 0.75 && range <= 2.25)
        {
            placeOneBlock(-1.85f, -1.2f);
        }
        if (range > 2.25)
        {
            placeOneBlock(-0.35f, 0.35f);
        }
    }

    // 放置一个砖
    private void placeOneBlock(float left, float right)
    {
        Vector3 position = new Vector3(Random.Range(left == 0f ? -2f : left, right == 0f ? 2f : right), -5f, 0f);
        GameObject block;
        int number = (score + 50) / 10;
        if (layer % 5 == 0)
        {
            block = GameObject.Instantiate(BlockBall, position, Quaternion.identity);
            if (layer % 5 == 0)
            {
                block.GetComponent<Block>().init(1, 1);
            }
        }
        else
        {
			block = GameObject.Instantiate(BlockCube, position, Quaternion.identity);
			block.GetComponent<Block>().init(0, number);
            block.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        }
        block.transform.name = "Block";
        blocks.Add(block);
    }

    // 推进所有砖
    private void pressAllBlocks()
    {
        foreach (GameObject block in blocks)
        {
            if (block != null)
            {
                Vector3 target = block.transform.position;
                target.y += 1;
                block.transform.position = Vector3.MoveTowards(block.transform.position, target, 0.7f);
                block.GetComponent<Block>().press();
            }
        }
        layer++;
        isReloading = false;
    }
}

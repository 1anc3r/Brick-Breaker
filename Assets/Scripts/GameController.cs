using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    // private Transform transform;
    public Font font;
    public GameObject BulletBall; // BulletBall预制体
    public GameObject BlockBall; // BlockBall预制体
    public GameObject BlockCube; // BlockCube预制体
    private Image background;
    private int frame = 0; // 游戏帧，用于控制射速
    private int score = 0; // 游戏分数
    private int layer = 0; // 层数
    private int capacity = 5; // 弹容量
    private int quantity = 5; // 弹余量
    private enum GameStatus // 游戏状态
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
    private List<GameObject> bullets;
    private List<GameObject> blocks;

    // Start is called before the first frame update
    void Start()
    {
        // transform = Camera.main.transform;
        layerMask = 1 << (LayerMask.NameToLayer("Plane"));
        bullets = new List<GameObject>();
        blocks = new List<GameObject>();
        background = GameObject.Find("Background Image").GetComponent<Image>();
        setBackgroundAsync(Path.Combine (Application.streamingAssetsPath, "Background.jpg"));
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
                        launchOneBullet();
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
            clearAllBullets();
        }
        if (status == GameStatus.Dead)
        {
            if (GUI.Button(new Rect(0, Screen.height * 0.26f, Screen.width, Screen.height * 0.11f), "", style))
            {
                gamePlay();
            }
            if (GUI.Button(new Rect(0, Screen.height * 0.50f, Screen.width, Screen.height * 0.11f), "", style))
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
        clearAllBullets();
        clearAllBlocks();
    }

    // 清空屏幕上的Bullet
    private void clearAllBullets()
    {
        foreach (GameObject bulletBall in bullets)
        {
            if (bulletBall != null)
            {
                GameObject.Destroy(bulletBall);
            }
        }
        bullets.Clear();
    }

    // 清空屏幕上的Block
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

    // 设置背景图片，同步方法
    public void setBackground(string path)
    {
        try
        {
            Texture2D texture = new Texture2D(Screen.width, Screen.height);
            texture.LoadImage(System.IO.File.ReadAllBytes(path));
            if (texture.width / texture.height != Screen.width / Screen.height)
            {
                texture = scaleTexture(texture, Screen.width, Screen.height);
            }
            background.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            background.sprite = sprite;
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    // 设置背景图片，异步方法
    public void setBackgroundAsync(string path)
    {
        StartCoroutine(setBackgroundByUrl(path));
    }

    // 设置背景图片，支持本地和网络
    private IEnumerator setBackgroundByUrl(string path)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
        {
            yield return request.SendWebRequest();
            Debug.Log("path===" + path);
            Texture2D texture = new Texture2D(Screen.width, Screen.height);
            if (request.responseCode == 200)
            {
                texture.LoadImage(request.downloadHandler.data);
                if (texture.width / texture.height != Screen.width / Screen.height)
                {
                    texture = scaleTexture(texture, Screen.width, Screen.height);
                }
                background.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                background.sprite = sprite;
            }
        }
    }

    // 缩放Texture
    private Texture2D scaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
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
    private void launchOneBullet()
    {
        if (quantity > 0)
        {
            GameObject bulletBall = GameObject.Instantiate(BulletBall, launcher, Quaternion.identity);
            bulletBall.GetComponent<Rigidbody>().AddForce(direction * 512f);
            bulletBall.transform.name = "Bullet Ball";
            bullets.Add(bulletBall);
            quantity--;
        }
        else
        {
            checkAllBullets();
        }
    }

    // 检测所有球是否在屏幕上
    private void checkAllBullets()
    {
        foreach (GameObject bulletBall in bullets)
        {
            if (bulletBall != null)
            {
                return;
            }
        }
        bullets.Clear();
        isLaunching = false;
        isReloading = true;
        reload();
    }

    // 放置一堆Block
    private void placeAllBlocks()
    {
        float range = Mathf.Abs(Random.Range(-3f, 3f));
        if (range > 0 && range <= 1.25)
        {
            placeOneBlock(1.2f, 1.85f);
        }
        if (range > 0.75 && range < 2.25)
		{
			placeOneBlock(-0.35f, 0.35f);
        }
		if (range >= 2.25)
		{
			placeOneBlock(-1.85f, -1.2f);
        }
    }

    // 放置一个Block
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

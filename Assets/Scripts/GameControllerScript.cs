using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameControllerScript : MonoBehaviour
{
    public Color32[] TileColors = new Color32[4];
    public Material TileColorMaterial;
    public GameObject EndPanel;
    public Text scoreText;
    public Text HighScoreText;

    private const float boundsSize = 5f;
    private const float boundsSizeMargin = 1.5f;
    private const float tilesMovingSpeed = 5f;
    private const float errorMargin = 0.1f;
    private const float tileBoundsGain = 0.25f;
    private const int comboStartGain = 3;

    private GameObject theTiles;
    private GameObject[] tiles;
    private Vector2 tileBounds;
    private int tileIndex = 0;
    private int tileCount = 0;

    private int combo = 0;

    private float tileTransition = 0f;
    private float tileSpeed = 1.5f;
    private float secondaryPosition;

    private bool isMovingOnX = true;
    private bool isGameOver = false;

    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;
    private Vector3 lastTileScale;

    void Start()
    {
        theTiles = GameObject.FindGameObjectWithTag("TheTiles");
        tiles = new GameObject[theTiles.transform.childCount];
        tileCount = -tiles.Length;
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = theTiles.transform.GetChild(i).gameObject;
            ColorMesh(tiles[i].GetComponent<MeshFilter>().mesh);

            tileCount++;
        }
        tileIndex = tiles.Length - 1;
        tileBounds = new Vector2(boundsSize, boundsSize);
        scoreText.text = "0";
        EndPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (PlaceTile())
            {
                RespawnTile();
            }
            else
            {
                EndGame();
            }
        }

        MoveTile();

        // Move theTiles
        theTiles.transform.position = Vector3.Lerp(theTiles.transform.position, desiredPosition, tilesMovingSpeed * Time.deltaTime);
    }

    private void RespawnTile()
    {
        lastTilePosition = tiles[tileIndex].transform.localPosition;
        lastTileScale = tiles[tileIndex].transform.localScale;
        tileIndex--;
        if (tileIndex < 0)
        {
            tileIndex = tiles.Length - 1;
        }

        desiredPosition = Vector3.down * tileCount;
        tiles[tileIndex].transform.localPosition = new Vector3(0, tileCount, 0);
        tiles[tileIndex].transform.localScale = new Vector3(tileBounds.x, 1, tileBounds.y);

        ColorMesh(tiles[tileIndex].GetComponent<MeshFilter>().mesh);

        // game score
        tileCount++;
        scoreText.text = tileCount.ToString();
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];

        float f = Mathf.Sin(tileCount * 0.25f);
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Lerp4(
                TileColors[0],
                TileColors[1],
                TileColors[2],
                TileColors[3],
                f);
        }
        mesh.colors32 = colors; 
    }

    private void MoveTile()
    {
        if (isGameOver)
        {
            return;
        }
        tileTransition += Time.deltaTime * tileSpeed;
        if (isMovingOnX)
        {
            // Moving on X axis
            tiles[tileIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * boundsSize * boundsSizeMargin, tileCount, secondaryPosition);
        }
        else
        {
            // Moving on Z axis
            tiles[tileIndex].transform.localPosition = new Vector3(secondaryPosition, tileCount, Mathf.Sin(tileTransition) * boundsSize * boundsSizeMargin);
        }
    }

    private bool PlaceTile()
    {
        Transform t = tiles[tileIndex].transform;

        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.position.x;

            if (Mathf.Abs(deltaX) > errorMargin)
            {
                // Cut the tile
                combo = 0;
                tileBounds.x -= Mathf.Abs(deltaX);
                if (tileBounds.x <= 0)
                {
                    return false;
                }

                float middle = (lastTilePosition.x + t.localPosition.x) / 2;
                t.localScale = new Vector3(tileBounds.x, 1, tileBounds.y);

                // Create rubble
                float xPosition = (t.position.x > 0)
                    ? t.position.x + (t.localScale.x / 2)
                    : t.position.x - (t.localScale.x / 2);
                CreateRubble(
                    new Vector3(xPosition, t.position.y, t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
                );
                t.localPosition = new Vector3(
                    middle - (lastTilePosition.x / 2)
                    , tileCount, lastTilePosition.z);
            }
            else
            {
                if (combo > comboStartGain)
                {
                    tileBounds.x += tileBoundsGain;

                    if (tileBounds.x > boundsSize)
                    {
                        tileBounds.x = boundsSize;
                    }

                    float middle = (lastTilePosition.x + t.localPosition.x) / 2;
                    t.localScale = new Vector3(tileBounds.x, 1, tileBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), tileCount, lastTilePosition.z);
                }
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, tileCount, lastTilePosition.z);
            }
        }
        else
        {
            float deltaZ = lastTilePosition.z - t.position.z;

            if (Mathf.Abs(deltaZ) > errorMargin)
            {
                // Cut the tile
                combo = 0;
                tileBounds.y -= Mathf.Abs(deltaZ);
                if (tileBounds.y <= 0)
                {
                    return false;
                }

                float middle = (lastTilePosition.z + t.localPosition.z) / 2;
                t.localScale = new Vector3(tileBounds.x, 1, tileBounds.y);
                // Create rublle
                float zPosition = (t.position.z > 0)
                    ? t.position.z + (t.localScale.z / 2)
                    : t.position.z - (t.localScale.z / 2);
                CreateRubble(
                    new Vector3(t.position.x, t.position.y, zPosition),
                    new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
                );
                t.localPosition = new Vector3(lastTilePosition.x, tileCount, middle - (lastTilePosition.z / 2));

            }
            else
            {
                if (combo > comboStartGain)
                {
                    tileBounds.x += tileBoundsGain;

                    if (tileBounds.y > boundsSize)
                    {
                        tileBounds.y = boundsSize;
                    }

                    float middle = (lastTilePosition.z + t.localPosition.z) / 2;
                    t.localScale = new Vector3(tileBounds.x, 1, tileBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, tileCount, middle - (lastTilePosition.z / 2));
                }
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, tileCount, lastTilePosition.z);
            }
        }

        secondaryPosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;

        isMovingOnX = !isMovingOnX;

        return true;
    }

    private void CreateRubble(Vector3 position, Vector3 scale)
    {
        GameObject go = Instantiate(tiles[tileIndex])as GameObject;
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>().mass = 1000;
    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);
        else if (t < 0.66f)
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

    private void EndGame()
    {
        isGameOver = true;
        tiles[tileIndex].AddComponent<Rigidbody>();
        if (PlayerPrefs.GetInt("HighScore") < tileCount)
        {
            PlayerPrefs.SetInt("HighScore", tileCount);
        }
        HighScoreText.text = string.Format("High score: {0}", PlayerPrefs.GetInt("HighScore"));
        EndPanel.SetActive(true);
    }

    public void OnButtonClick(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

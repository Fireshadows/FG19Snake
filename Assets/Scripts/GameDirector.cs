using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField]
    private PlayerSnake m_playerPrefab;
    private PlayerSnake m_player;

    [SerializeField]
    private Vector2 m_startingPosition;

    [SerializeField]
    private int m_rows;
    [SerializeField]
    private int m_columns;

    public int GridArea { get => m_rows * m_columns; }

    public float m_updateRate;
    private float m_updateTime;

    [SerializeField]
    private GameObject m_apple;

    [SerializeField]
    private Text m_appleCountDisplay;
    [SerializeField]
    private Animator m_appleAnimator;
    [SerializeField]
    private GameObject m_autoOnDisplay;
    [SerializeField]
    private float m_autoBlinkRate;
    private float m_autoBlinkTime;

    private int m_appleCount = 0;

    [HideInInspector]
    public Tile[,] m_grid;
    [HideInInspector]
    public Tile m_appleTile;
    public AudioSource m_fxApple;

    public AudioSource m_fxStep;

    private bool m_playing;

    [SerializeField]
    private CanvasGroup m_controlScheme;

    private void Start()
    {
        m_controlScheme.alpha = 1;

        CreateLevel();

        Initialize();

        NoFreeTiles();
    }

    private void Initialize()
    {
        m_playing = false;

        m_player = Instantiate(m_playerPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<PlayerSnake>();
        m_player.Initialize(m_grid[(int)m_startingPosition.x, (int)m_startingPosition.y], this);

        MoveApple(new Vector2(16, 10));

        m_autoOnDisplay.SetActive(false);
        m_updateTime = m_updateRate;
        m_autoBlinkTime = m_autoBlinkRate;

        m_appleCount = 0;
    }

    private void CreateLevel()
    {
        m_grid = new Tile[m_rows, m_columns];

        for (int x = 0; x < m_grid.GetLength(0); x++)
        {
            for (int y = 0; y < m_grid.GetLength(1); y++)
            {
                Tile m_newTile = new Tile(new Vector2(x, y));

                m_grid[x, y] = m_newTile;
            }
        }

        for (int x = 0; x < m_grid.GetLength(0); x++)
        {
            for (int y = 0; y < m_grid.GetLength(1); y++)
            {
                if (x == 0) m_grid[x, y].m_neighbours[(int)Direction.Left] = null;
                else m_grid[x, y].m_neighbours[(int)Direction.Left] = m_grid[x - 1, y];

                if (x == m_grid.GetLength(0) - 1) m_grid[x, y].m_neighbours[(int)Direction.Right] = null;
                else m_grid[x, y].m_neighbours[(int)Direction.Right] = m_grid[x + 1, y];

                if (y == m_grid.GetLength(1) - 1) m_grid[x, y].m_neighbours[(int)Direction.Up] = null;
                else m_grid[x, y].m_neighbours[(int)Direction.Up] = m_grid[x, y + 1];

                if (y == 0) m_grid[x, y].m_neighbours[(int)Direction.Down] = null;
                else m_grid[x, y].m_neighbours[(int)Direction.Down] = m_grid[x, y - 1];
            }

        }

    }

    private void Update()
    {
        m_appleCountDisplay.text = m_appleCount.ToString("000");
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
        if (!m_playing)
        {
            KeyCode[] m_startButtons = {
                KeyCode.LeftArrow,
                KeyCode.UpArrow,
                KeyCode.RightArrow,
                KeyCode.DownArrow,
                KeyCode.Space
            };
            foreach (KeyCode m_keyCode in m_startButtons)
            {
                if (Input.GetKeyDown(m_keyCode))
                {
                    m_playing = true;
                    break;
                }
            }

            if (!m_playing)
                return;
        }
        if (m_controlScheme.alpha > 0)
            m_controlScheme.alpha -= 3 * Time.deltaTime;
        if (m_updateTime <= 0)
        {
            m_updateTime = m_updateRate * (m_player.m_autoPilot ? 1 : 20);
            m_player.OnUpdate();
            m_fxStep.Play();
        }
        else
            m_updateTime -= Time.deltaTime;
        if (m_player.m_autoPilot)
        {
            if (m_autoBlinkTime <= 0)
            {
                m_autoBlinkTime = m_autoBlinkRate;
                m_autoOnDisplay.SetActive(!m_autoOnDisplay.activeSelf);
            }
            else
                m_autoBlinkTime -= Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_autoBlinkTime = 0;
            m_autoOnDisplay.SetActive(false);
        }
    }
    
    public void OnAppleGrappled()
    {
        m_appleCount++;
        m_appleAnimator.SetTrigger("Shake");
        if (m_fxApple.pitch <= 1)
            m_fxApple.pitch = 1.05f;
        else
            m_fxApple.pitch = .95f;
        m_fxApple.Play();
        MoveApple(GetAnyFreeTile().m_coordinates);
    }

    private void MoveApple(Vector2 p_coordinates)
    {
        if (m_appleTile != null)
            m_appleTile.m_hasApple = false;

        m_apple.transform.position = p_coordinates;

        m_appleTile = m_grid[(int)p_coordinates.x, (int)p_coordinates.y];
        m_appleTile.m_hasApple = true;
    }
    
    private bool NoFreeTiles()
    {
        var m_freeTiles = GetFreeTiles();

        if (m_freeTiles.Count() == 0)
            return true;
        return false;
    }

    private Tile GetAnyFreeTile()
    {
        var m_freeTiles = GetFreeTiles();

        if (m_freeTiles.Count() == 0)
            return null;

        return m_freeTiles.ElementAt(UnityEngine.Random.Range(0, m_freeTiles.Count()));
    }

    private IEnumerable<Tile> GetFreeTiles()
    {
        return from Tile m_tile in m_grid
               where !m_tile.m_impassable
               select m_tile;
    }

    public void GameOver()
    {
        m_player.OnGameOver();
        Initialize();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit ();
#endif
    }
}
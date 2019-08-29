using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

public class GameDirector : MonoBehaviour
{
    //LinkedList m_linkedList;

    [SerializeField]
    private PlayerSnake m_player;

    [SerializeField]
    private Vector2 m_startingPosition;

    [SerializeField]
    private int m_rows;
    [SerializeField]
    private int m_columns;
    public float m_updateRate;
    private float m_updateTime;

    [SerializeField]
    private GameObject m_apple;

    [HideInInspector]
    public Tile[,] m_grid;
    [HideInInspector]
    public Tile m_appleTile;
    
    private bool m_playing;
    private void Start()
    {
        m_playing = false;
        CreateLevel();

        m_player = Instantiate(m_player.gameObject, Vector3.zero, Quaternion.identity).GetComponent<PlayerSnake>();
        m_player.Initialize(m_grid[(int)m_startingPosition.x, (int)m_startingPosition.y], this);

        MoveApple(new Vector2(16, 10));

        m_updateTime = m_updateRate;
        NoFreeTiles();

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
        if (m_updateTime <= 0)
        {
            m_updateTime = m_updateRate;
            m_player.OnUpdate();
        }
        else
            m_updateTime -= Time.deltaTime;
    }
    
    public void OnAppleGrappled()
    {
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
        SceneManager.LoadScene(0);
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
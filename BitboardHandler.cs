/*
 *This code is not to show a game, but to demonstrate the use of bit board.
 *The game consists of a set of 8 by 8 tiles, that are procedurally created. They are of
  dirt,grain,water or rocky type.
 *The objective of the game is to spawn houses on dirt patches. Against the player the
  computer spawns trees at the locations of dirt. Once a tree has been planted that player
  can't spawn their house.
 *Bitboards can be used efficiently to store the locations rather than using an array, and 
  further the advantage is that bitwise operations are performed to check for location,
  hence this process is very fast.
 *Bitboards are common in case of board games.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitboardHandler : MonoBehaviour
{
    //storing the name assigned to mouse click action in Input Manager
    [SerializeField] private string MouseClickActionName;

    //storing the prefabs.
    [SerializeField] private GameObject[] tilePrefabs;

    //tree prefab
    [SerializeField] private GameObject treePrefab;

    //house prefab
    [SerializeField] private GameObject housePrefab;

    //score text
    [SerializeField] private UnityEngine.UI.Text score;

    //sum of scores
    [SerializeField] private int sumScore = 0;

    //initial delay for spawning the tree
    [SerializeField] private float initialDelay = 1;

    //interval for spawning the tree
    [SerializeField] private float repeatInterval = 1;


    //storing the bits of dirt;
    long dirtBB = 0;

    //storing the locations of houses
    long playerBB = 0;

    //storing the locations of trees
    long treeBB = 0;
    
    //storing the tiles info;
    List<GameObject> tiles = new List<GameObject>();




    private void Start()
    {
        //instantiating the tiles.
        for (int i = 0; i < 8; ++i)
            for (int j = 0; j < 8; ++j)
            {
                int temp_ind = Random.Range(0, tilePrefabs.Length);
                GameObject tileObject = Instantiate(tilePrefabs[temp_ind], new Vector3(i, 0, j), Quaternion.identity);
                tileObject.name = tilePrefabs[temp_ind].name + "_" + i + "_" + j;
                if(tilePrefabs[temp_ind].tag.ToLower()=="dirt")  dirtBB |= ReturnBB(i,j);

                tiles.Add(tileObject);
            }
        //Debug.Log(System.Convert.ToString(dirtBB,2));
        //Debug.Log("Number of dirt tiles: "+ReturnNumOfOne(dirtBB));

        InvokeRepeating("PlantTree",initialDelay,repeatInterval);
    }

    private void Update()
    {
        if (Input.GetButtonDown(MouseClickActionName))
        {
            HandleMouseClick();
        }
    }

    //function to handle a simple mouse click
    void HandleMouseClick()
    {
        //Debug.Log("Mouse Clicked.");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit))
        {
            //Debug.Log("Hitting "+hit.collider.gameObject.name);
            GameObject parent = hit.collider.transform.gameObject;                       

            string nm = parent.name;
            int ln = nm.Length;
            int r = nm[ln-3] - 48;
            int c = nm[ln-1] - 48;
            Debug.Log(nm);
            //Debug.Log("r: "+r+" c: "+c);


            //Debug.Log("Dirt bb: ");
            //Debug.Log(System.Convert.ToString(dirtBB,2).PadLeft(64,'0'));
            //Debug.Log("Checking r: "+r+" c: "+c);

            /*
             * only allow player to spawn a  house if a tree or a house is not 
               at that position
            */
            if (parent.tag.ToLower()=="dirt" && !CheckBit(playerBB, r, c) && !CheckBit(treeBB, r, c))
            {
                GameObject house = Instantiate(housePrefab);
                house.transform.parent = hit.collider.transform;
                house.transform.localPosition = Vector3.zero;
                playerBB |= ReturnBB(r, c);
                //Debug.Log(System.Convert.ToString(playerBB, 2).PadLeft(64, '0'));
                ShowScore();
            }
            else
            {
                Debug.Log("Can't spawn a house, no dirt here or being blocked.");
            }

        }
    }

    //returns 1 left shifted by r * 8 + c.
    long ReturnBB(int r,int c)
    {
        return 1L << (r * 8) + c;
    }

    //returns the number of one in a bit
    int ReturnNumOfOne(long bb)
    {
        int cnt = 0;
        long temp_bb = bb;

        while (temp_bb!=0)
        {
            temp_bb &= temp_bb - 1;
            cnt++;
        }

        return cnt;
    }

    //plants tree at positions of dirt
    void PlantTree()
    {
        GameObject tree = Instantiate(treePrefab);
        int r = Random.Range(0,8);
        int c = Random.Range(0,8);
        
        /*wan't to spawn a tree at dirt's position if and only if the player does not 
        have a house there already.
        */
        if (CheckBit(dirtBB , r, c) && !CheckBit(playerBB, r, c))
        {
            tree.transform.parent = tiles[r * 8 + c].transform;
            tree.transform.localPosition = Vector3.zero;
            treeBB |= ReturnBB(r,c);
        }
    }

    //returns true if a bit at a particular position is set
    bool CheckBit(long bb,int r,int c)
    {
        long mask = ReturnBB(r, c);
        /*
        Debug.Log("r: "+r+" c: "+c);
        Debug.Log("Bitboard: "+System.Convert.ToString(bb,2));
        Debug.Log("Mask: "+System.Convert.ToString(mask,2));
        Debug.Log("Result of CheckBit: ");
        Debug.Log(System.Convert.ToString(bb,2));
        */
        return ((bb & mask)!=0);
    }

    //method to show the score
    void ShowScore()
    {
        sumScore =  ReturnNumOfOne(playerBB);
        score.text = System.Convert.ToString(sumScore,10);
    }

}

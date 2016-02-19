﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SteeringController))]
public class UnitContoller : MonoBehaviour
{   
    public bool seeker = false;  
    public bool frozen = false;
    public bool lastFrozen = false;
    bool hasHalted = false;
    bool newDirective = false;
    public bool stopGo;
    Vector3 acceleration;

    Transform target;
    List<Transform> seekers;
    Vector3 centerOfMass = Vector3.zero;
    Vector3 targetCoord;
    SteeringController control;
    Vector3 lastCalculatedTarget = Vector3.zero;

    Vector2[] path;
    int pathIndex = 0;
    Vector2 currentWaypoint = Vector2.zero;
    TagGameLogic game;

    // Use this for initialization
    void Start()
    {
        game = FindObjectOfType<TagGameLogic>();
        acceleration = Vector3.zero;
        targetCoord = Vector3.zero;
        seekers = new List<Transform>();
        control = GetComponent<SteeringController>();
        path = new Vector2[0];
    }

    void FindTarget()
    {
        
        if (!target && gameObject.tag == "Seeker")
        {
            seeker = true;
            GetComponent<MeshRenderer>().material.color = Color.red;
            target = GameObject.FindGameObjectWithTag("Unit").transform;
        }
        else if (seekers.Count == 0 && gameObject.tag == "Unit")
        {
            seeker = false;
            GetComponent<MeshRenderer>().material.color = Color.green;
            GameObject[] units = GameObject.FindGameObjectsWithTag("Seeker");
            foreach(var u in units)
            {
                if(u.tag == "Seeker")
                {
                    seekers.Add(u.transform);
                }                    
            }
        }
                
    }
    Vector2 FindCenterOfMass()
    {
        var center = Vector3.zero;
        foreach (var s in seekers)
        {
            center += s.position;
        }        
        return center / seekers.Count;
    }


    Vector2 FindDestination()
    {
        Vector2 destination = Vector2.zero;
        if (seeker)
        {
            destination = target.position + new Vector3(Random.Range(0f, 0.4f), Random.Range(0f, 0.4f),0);
            control.maxVelocity = 2;
        }
        else
        {                  

            Vector3 avoid = FindCenterOfMass();
            destination = avoid;
            float maxDist = 0;
            Vector3 fleeDirection = (transform.position - avoid).normalized * 0.5f;

            //CHeck if position exists in graph -> is walkable
            foreach (var l in GameObject.FindGameObjectsWithTag("Landmark"))
            {
                if (Vector3.Distance((l.transform.position + fleeDirection), avoid) > maxDist)
                {
                    maxDist = Vector3.Distance((l.transform.position + fleeDirection), avoid);
                    destination = (l.transform.position + fleeDirection);
                }                
            }            
        }
        return destination;
    }
 
    // Update is called once per frame
    void Update()
    {
        if(game.started)
        {
            
            Vector2 accleration = Vector2.zero;
            /*
            if (GetComponent<Rigidbody2D>().velocity.magnitude < 0.001f)
            {
                acceleration = control.seek((Vector2)control.wanderPosition());
            }
            else
            {
                FindTarget();      
                acceleration = control.arrive(FindNextPosition(FindDestination()));
            }
            */

            FindTarget();
            acceleration = control.seek(FindNextPosition(FindDestination()));
            control.steer(acceleration);
            control.lookWhereYoureGoing();
        }       
    }    

    Vector2 FindNextPosition(Vector2 targetPos)
    {       
        
        if(path.Length <= 0 || (Vector3.Distance(lastCalculatedTarget, (Vector3)targetPos) > 5))
        {
            path = Pathfinding.RequestPath(transform.position, targetPos);
            pathIndex = 0;
            if (path.Length > 0)
            {
                currentWaypoint = path[pathIndex];
            }
            else
            {
                currentWaypoint = transform.position;
            }
        }

        if (Vector3.Distance((Vector3)currentWaypoint, transform.position) < 0.01f)
        {
            pathIndex++;
            if (pathIndex >= path.Length)
            {
                path = Pathfinding.RequestPath(transform.position, targetPos);
                pathIndex = 0;
            }

            if (path.Length > 0)
            {
                currentWaypoint = path[pathIndex];
            }
            else
            {
                currentWaypoint = transform.position;
            }          
        }

        return currentWaypoint;
    }



   

    void OnCollisionEnter(Collision collision)
    {   
        if(collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            GetComponent<Rigidbody2D>().AddForce((transform.position - collision.gameObject.transform.position).normalized * GetComponent<Rigidbody2D>().mass, ForceMode2D.Impulse);
        }


        if(collision.gameObject.tag == "Unit" || collision.gameObject.tag == "Seeker")
        {
            if (collision.gameObject.GetComponent<UnitContoller>().seeker && !seeker && !frozen)
            {
                GameObject.FindObjectOfType<TagGameLogic>().frozenPlayers++;
                frozen = true;

                if (GameObject.FindObjectOfType<TagGameLogic>().frozenPlayers == GameObject.FindObjectOfType<TagGameLogic>().numberOfPlayers - 1)
                {
                    lastFrozen = true;
                }              
                
            }

            if (collision.gameObject.GetComponent<UnitContoller>().frozen && !seeker)
            {
                GameObject.FindObjectOfType<TagGameLogic>().frozenPlayers--;
                collision.gameObject.GetComponent<UnitContoller>().lastFrozen = false;
                collision.gameObject.GetComponent<UnitContoller>().frozen = false;
            }
        }   
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = pathIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                //Gizmos.DrawCube((Vector3)path[i], Vector3.one *.5f);

                if (i == pathIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}

﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteeringController))]
public class UnitContoller : MonoBehaviour
{   
    public bool seeker = false;
    public bool frozen = false;
    public bool lastFrozen = false;
    bool hasHalted = false;
    bool newDirective = false;    

    Vector3 acceleration;    

    // Use this for initialization
    void Start()
    {
        acceleration = Vector3.zero;

    }
 
    // Update is called once per frame
    void Update()
    {       


        if (frozen)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<MeshRenderer>().material.color = new Color(0.0f, 0.0f, 1.0f);
        }
        else
        {
            if (seeker)
            {
                GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f);
                //Seek non-frozen characters
                acceleration = GetComponent<SteeringController>().seek(FindNearestUnit(false)) + (0.5f)*GetComponent<SteeringController>().wanderPosition();

            }
            else
            {
                GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 0.0f);

                
                //Flee and Seek frozen characters
                Vector3 seekerPosition = GameObject.FindGameObjectWithTag("Seeker").transform.position;


                //Check within flee range
                if (Vector3.Distance(transform.position, seekerPosition) < 3)
                {
                    acceleration = (-1)*GetComponent<SteeringController>().seek(seekerPosition);
                }
                else
                {
                    acceleration = GetComponent<SteeringController>().seek(FindNearestUnit(true));
                }

                /*
                if (!hasHalted)
                {
                    acceleration = Vector3.zero;

                    if ((GetComponent<Rigidbody>().velocity.magnitude > 0.1f))
                    {
                        if(GetComponent<SteeringController>().notKinetic == 0)
                        {
                            GetComponent<Rigidbody>().velocity = Vector3.zero;
                        }
                        else
                        {
                            GetComponent<Rigidbody>().velocity /= 4;
                        }                        
                    }
                    else
                    {
                        hasHalted = true;
                    }
                }
                else
                {      
                   
                } 
                */               
            }
                        
            GetComponent<SteeringController>().steer(acceleration);
            GetComponent<SteeringController>().lookAtDirection(acceleration);
        }
    }

    //If !Seeking then wander + flee if close
    //else seek nearest opponent

    //Find closest unit
    Vector3 FindNearestUnit(bool findFrozen)
    {
        UnitContoller[] units = FindObjectsOfType<UnitContoller>();

        Vector3 nearestPosition = new Vector3(999, 999, 999);
        foreach (var unit in units)
        {
            if ((unit.transform != this.transform) 
                && ((!findFrozen && !unit.frozen) || (findFrozen && unit.frozen))
                && (Vector3.Distance(unit.gameObject.transform.position, transform.position) < Vector3.Distance(nearestPosition, transform.position)))
            {
                if(findFrozen && Vector3.Distance(transform.position, unit.transform.position) < 4 || !findFrozen)
                {
                    nearestPosition = unit.gameObject.transform.position;
                }                
            }
        }

        //If no target found, wander
        if (nearestPosition.x == 999)
        {
            nearestPosition = GetComponent<SteeringController>().wanderPosition();
        }

        return nearestPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
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
}

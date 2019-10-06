﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main control and logic for all ships. Input driven by another class
public class ShipController : MonoBehaviour
{
    // Set in editor


    private List<ShipPart> parts;
    private List<Thrusters> forwardThrusters = new List<Thrusters>();
    private List<Thrusters> backThrusters = new List<Thrusters>();
    private List<Thrusters> positiveTorqueThrusters = new List<Thrusters>(); // Thrusters used in right turn - can be on both sides
    private List<Thrusters> negativeTorqueThrusters = new List<Thrusters>(); // Thrusters used in left turn - can be on both sides
    private List<Thrusters> thrusters = new List<Thrusters>();
    private List<ProjectileWeapon> weapons = new List<ProjectileWeapon>();
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void RegisterParts(ShipPart[,] parts) {
        List<ShipPart> asList = new List<ShipPart>();
        foreach (ShipPart part in parts) {
            asList.Add(part);
        }
        RegisterParts(asList);
    }

    public void RegisterParts(List<ShipPart> parts) {
        this.parts = parts;
        rb = GetComponent<Rigidbody2D>(); // Might get called before Start

        foreach (ShipPart part in parts) {
            if (part == null) {
                continue;
            }

            part.transform.parent = transform;

            // Organize thrusters into categories
            Thrusters partThrusters = part.GetComponent<Thrusters>();
            if (partThrusters != null) {
                thrusters.Add(partThrusters);
                float partZ = part.transform.rotation.eulerAngles.z;
                int dir = Mathf.RoundToInt(partZ / 90);
                if (dir == 0) {
                    forwardThrusters.Add(partThrusters);
                } else if (dir == 2 || dir == -2) {
                    backThrusters.Add(partThrusters);
                } else { 
                    if (Vector3.Cross(part.transform.up, (Vector2)part.transform.position - rb.worldCenterOfMass).z > 0) {
                        negativeTorqueThrusters.Add(partThrusters);
                    } else {
                        positiveTorqueThrusters.Add(partThrusters);
                    }
                }
            } else if (part.GetComponent<ProjectileWeapon>() != null) {
                weapons.Add(part.GetComponent<ProjectileWeapon>());
            }

            // Set density. Every part has the same volume, so just use mass directly
            part.GetComponent<Collider2D>().density = part.mass;

            // Tag to the same layer as the parent
            part.gameObject.layer = gameObject.layer;
        }
    }

    // Each direction can be on simultaneously because they activate different thrusters
    public void Move(bool up, bool down, bool right, bool left) {
        List<Thrusters> activeThrusters = new List<Thrusters>();

        if (up) {
            activeThrusters.AddRange(forwardThrusters);
        }

        if (down) {
            activeThrusters.AddRange(backThrusters);
        }

        if (right) {
            activeThrusters.AddRange(negativeTorqueThrusters);
        }

        if (left) {
            activeThrusters.AddRange(positiveTorqueThrusters);
        }

        foreach (Thrusters thruster in thrusters) {
            if (activeThrusters.Contains(thruster)) {
                thruster.ActivateThruster();
                rb.AddForceAtPosition(thruster.transform.up * thruster.thrustForce, thruster.transform.position);
            } else {
                thruster.DisableThruster();
            }
        }
    }

    public void Attack() {
        foreach (ProjectileWeapon weapon in weapons) {
            weapon.Attack();
        }
    }

    public void NotifyPartDestroyed(ShipPart part) {
        parts.Remove(part);

        Thrusters thruster = part.GetComponent<Thrusters>();
        if (thruster != null) {
            forwardThrusters.Remove(thruster);
            backThrusters.Remove(thruster);
            positiveTorqueThrusters.Remove(thruster);
            negativeTorqueThrusters.Remove(thruster);
            thrusters.Remove(thruster);
        }

        ProjectileWeapon weapon = part.GetComponent<ProjectileWeapon>();
        if (weapon != null) {
            weapons.Remove(weapon);
        }

        if (part.partName == "Cockpit") {
            // Inverse for loop because each death will modify parts with the callback
            Debug.LogFormat("Killing remaining {0} parts", parts.Count);
            for (int i = parts.Count - 1; i >=0; i--) {
                // TODO: It would be really cool if we could show each part blowing up in succession
                parts[i].TakeDamage(parts[i].maxHealth);
            }
            Destroy(gameObject);
        }
    }
}

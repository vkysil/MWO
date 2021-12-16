using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script for managing the jump pad logic
// extends the GroundEffector script
public class JumpPad : GroundEffector
{
    public float jumpPadAmount = 15f; // amount of force behind the jump
    public float jumpPadUpperLimit = 30f; // maximum amount of jumping force
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stealth_Behaviour : BasePlayerController
{
    private void Update()
    {
        Player_Inputs();
        PlayerMovement();
        Check_Interactables();
    }
}

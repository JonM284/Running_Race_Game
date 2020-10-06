using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Sway : MonoBehaviour
{
    //this is made using the help of a youtube video on fps games

    public float amount, max_Amount, smooth_Amount;

    private Vector3 starting_Pos;
    


    // Start is called before the first frame update
    void Start()
    {
        starting_Pos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float movementX = -Input.GetAxis("Mouse X") * amount;
        float movementY = -Input.GetAxis("Mouse Y") * amount;

        movementX = Mathf.Clamp(movementX, -max_Amount, max_Amount);
        movementY = Mathf.Clamp(movementY, -max_Amount, max_Amount);

        Vector3 finalPos = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + starting_Pos, Time.deltaTime * smooth_Amount);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    

    void FixedUpdate()
    {
        Move();
    }

    public float speed = 1f;

    public void Move()
    {

        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        transform.position += movement * speed * Time.deltaTime;

    }
}

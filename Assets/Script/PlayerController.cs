using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 _velocity;
    private Rigidbody rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 velocity)
    {
        _velocity = velocity;
    }
    private void FixedUpdate()
    {
        rigidBody.MovePosition(rigidBody.position + _velocity * Time.deltaTime);
        //Debug.Log(_velocity);
    }
    public void LookAt(Vector3 point)
    {
        Vector3 heightCorrectedPoint = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(heightCorrectedPoint);
    }
}

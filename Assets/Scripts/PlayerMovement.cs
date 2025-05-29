using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] Transform followTarget;
    [SerializeField] float followDistance = 2f;
    [SerializeField] Vector2 movementRange = new Vector2(2f, 2f);
    // [SerializeField] float movementSpeed = 10f; // not needed
    [SerializeField] float smoothTime = 0.2f;
    [SerializeField] float maxRoll = 20f;
    [SerializeField] float rollSpeed = 2f;


    Vector3 velocity;
    float roll;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the target position based on the follow distance and the target's position

        Vector3 tragetPos = followTarget.position + (followTarget.forward * -followDistance);

        // Apply smooth damp to the player's position
        Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, tragetPos, ref velocity, smoothTime);

        //Calculate the new local position
        Vector3 localPos = transform.InverseTransformPoint(smoothedPos);

        // Clamp the local position
        localPos.x = Mathf.Clamp(localPos.x, -movementRange.x, movementRange.x);
        localPos.y = Mathf.Clamp(localPos.y, -movementRange.y, movementRange.y);

        // Update the player's position
        transform.position = transform.TransformPoint(localPos);

        // Match rotation with the target rotation
        transform.rotation = followTarget.rotation;

        // Math the roll based on angular velocity
        // Vector3 angularVelocity = followTarget.GetComponent<Rigidbody>().angularVelocity;

        // rotate around the forward axis of the player
        //roll = Mathf.SmoothDamp(roll, angularVelocity.y * rollSpeed, ref velocity.y, smoothTime);
        //roll = Mathf.Clamp(roll, -maxRoll, maxRoll);

        // Apply the roll to the player's rotation
        /*
        Quaternion rollRotation = Quaternion.Euler(0, 0, roll);
        transform.rotation = Quaternion.Slerp(transform.rotation, rollRotation * followTarget.rotation, Time.deltaTime * smoothTime);
        */

    }
}

using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    [SerializeField]
    private float minXPos = -118.5f;

    [SerializeField]
    private float maxXPos = -15f;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float cameraSpeed = 5f;

    // Start is called once before the first execution of Update
    // after the MonoBehaviour is created
    void Start()
    {
        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError(
                    "No target assigned and no GameObject tagged as Player. " +
                    "Please ensure a reference for the target variable."
                );

                return;
            }

            target = player.transform;
        }

        // Make sure the minimum value is not larger
        // than the maximum value.
        if (minXPos > maxXPos)
        {
            Debug.LogWarning(
                "Min X Pos was greater than Max X Pos. " +
                "The values have been swapped."
            );

            float oldMin = minXPos;
            minXPos = maxXPos;
            maxXPos = oldMin;
        }

        // Original system moves the camera slowly toward the player.
        // Added starting snap so the camera does not travel across
        // the whole level when Play mode begins.
        Vector3 startingPosition = transform.position;

        startingPosition.x = Mathf.Clamp(
            target.position.x,
            minXPos,
            maxXPos
        );

        transform.position = startingPosition;
    }

    // Inputs being polled in Update -
    // Physics generally are applied in FixedUpdate -
    // and camera movement is done in LateUpdate

    // Update is with the computer tick rate
    // FixedUpdate is a fixed rate at which your game updates
    // LateUpdate happens as the last possible update for that frame

    // Update is called once per frame
    void LateUpdate()
    {
        // Early return - if we don't have a target,
        // we can't follow anything so we shouldn't do anything
        if (target == null)
        {
            return;
        }

        // Store our current position
        Vector3 currentPos = transform.position;

        // Update our X position to be the same as our target's X position,
        // but clamp it between our minimum and maximum values
        float targetX = Mathf.Clamp(
            target.position.x,
            minXPos,
            maxXPos
        );

        currentPos.x = targetX;

        // Apply the position back to the camera
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentPos,
            cameraSpeed * Time.deltaTime
        );
    }
}
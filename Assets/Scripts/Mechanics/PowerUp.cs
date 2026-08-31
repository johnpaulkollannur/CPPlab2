using UnityEngine;

/// <summary>
/// Lab #4 Power-Up.
///
/// The Power-Up destroys itself only when
/// the Player touches it.
///
/// Projectiles and other objects do not
/// collect or destroy the Power-Up.
/// </summary>
public class PowerUp : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("PowerUp ready: " + gameObject.name);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Show us what entered the PowerUp.
        Debug.Log(
            "PowerUp touched by: " +
            other.gameObject.name +
            " | Tag: " +
            other.gameObject.tag
        );


        // Only the Player can collect the PowerUp.
        if (other.CompareTag("Player"))
        {
            Debug.Log("POWER-UP COLLECTED!");

            Destroy(gameObject);
        }
    }
}
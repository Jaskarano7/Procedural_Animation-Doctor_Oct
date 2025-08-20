using UnityEngine;

public class HandScript : MonoBehaviour
{

    [SerializeField] private PlayerMovement playerMovement;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Walk"))
        {
            if(gameObject.name == "Empty")
            {
                playerMovement.LeftArmOnGround = true;
            }
            else
            {
                playerMovement.RightArmOnGround = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Walk"))
        {
            if (gameObject.name == "Empty")
            {
                playerMovement.LeftArmOnGround = false;
            }
            else
            {
                playerMovement.RightArmOnGround = false;
            }
        }
    }
}

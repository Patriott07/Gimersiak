using System.Xml.Serialization;
using UnityEngine;

public class DetectorBall : MonoBehaviour
{
    
    public Rigidbody2D rbBall;
    public static DetectorBall Instance;
    void Start()
    {
        Instance = this;
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("ball"))
        {
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (collider.gameObject.GetComponent<BallSc>().IsFragment) return;
            rbBall = rb;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("ball"))
        {
            rbBall = null;
        }
    }
}

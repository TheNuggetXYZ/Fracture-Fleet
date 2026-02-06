using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Vector2 randomSpeed = new(30, 150);
    private float speed;
    
    private void Start()
    {
        SpaceGravitySimulator.I.AddAsteroid(this);
        
        speed = Random.Range(randomSpeed.x, randomSpeed.y);

        if (TryGetComponent(out AstroData astroData))
            Destroy(astroData);
    }

    public void UpdatePosition()
    {
        Vector3 directionToBody = (transform.position - SpaceGravitySimulator.I.mainBody.transform.position).normalized;
        
        Vector3 tangent = Vector3.Cross(Vector3.up, directionToBody).normalized;
        
        rb.AddForce(tangent * (speed * Time.fixedDeltaTime), ForceMode.Acceleration);
    }
}

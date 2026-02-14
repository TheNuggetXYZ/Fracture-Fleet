using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CargoshipHatch : MonoBehaviour
{
    [SerializeField] private float openCloseHatchPlayerDistance;
    [SerializeField] private float closedAngle = 0;
    [SerializeField] private float openedAngle = 60;
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;

    private bool opening = true;
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    private void OnEnable()
    {
        game.input.Player.CargoshipHatch.performed += ToggleHatch;
    }

    private void OnDisable()
    {
        game.input.Player.CargoshipHatch.performed -= ToggleHatch;
    }

    private void Update()
    {
        if (Vector3.Distance(game.player.transform.position, transform.position) <= openCloseHatchPlayerDistance)
        {
            game.worldMenu.ShowObject(game.worldMenu.doorKey, true);
        }

        /*if (opening && transform.rotation.eulerAngles.z < openedAngle)
        {
            transform.Rotate(0, 0, Mathf.Min(speed * Time.deltaTime, openedAngle - transform.rotation.eulerAngles.z));
        }
        else if (transform.rotation.eulerAngles.z > closedAngle)
        {
            transform.Rotate(0, 0, Mathf.Max(-speed * Time.deltaTime, closedAngle - transform.rotation.eulerAngles.z));
        }*/
    }
    
    void FixedUpdate()
    {
        float currentZ = rb.rotation.eulerAngles.z;
        float targetZ = opening ? openedAngle : closedAngle;

        float newZ = Mathf.MoveTowardsAngle(
            currentZ,
            targetZ,
            speed * Time.fixedDeltaTime
        );

        rb.MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.parent.rotation.eulerAngles.z + newZ));
    }

    private void ToggleHatch(InputAction.CallbackContext cc)
    {
        opening = !opening;
    }
}

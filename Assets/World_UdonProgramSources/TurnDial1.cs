using UdonSharp;
using UnityEngine;

public class TurnDial1 : UdonSharpBehaviour
{
    [SerializeField] private int positions;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private NumberSequencePuzzle numberpuzzle;

    private int currentNumber = 0;
    private float targetAngle;

    void Start()
    {
        targetAngle = transform.localEulerAngles.z;
    }

    public override void Interact()
    {
        currentNumber++;

        if (currentNumber >= positions)
        {
            currentNumber = 0;
        }

        targetAngle += 360f / positions;
        numberpuzzle.CheckCode();
    }

    void Update()
    {
        float currentAngle = transform.localEulerAngles.z;

        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, newAngle);
    }

    public int GetCurrentNumber()
    {
        return currentNumber;
    }
}
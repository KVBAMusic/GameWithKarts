using System.Collections;
using UnityEngine;

public class CarCamera : CarComponent
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float smoothingAmount;

    [SerializeField] private Camera frontFacingCamera;
    [SerializeField] private Camera backFacingCamera;
    [Header("Post-race settigns")]
    [Min(0)]
    [SerializeField] private float cameraAnimationDelay;
    [SerializeField] private Vector3 cameraAnimationEulerAngles;
    public Camera FrontFacingCamera => frontFacingCamera;
    public Camera BackFacingCamera => backFacingCamera;

    private Quaternion offset = Quaternion.identity;
    public bool IsFollowingPlayer { get; set; }

    private void Update() {
        frontFacingCamera.gameObject.SetActive(car.Input.BackCamera == 0 || car.IsBot);
        backFacingCamera.gameObject.SetActive(car.Input.BackCamera == 1 && !car.IsBot);
        Vector3 targetEuler = transform.eulerAngles;
        if (!car.Movement.IsAntigrav) targetEuler.z = 0;
        Quaternion targetRotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(targetEuler) * offset, Time.deltaTime * smoothingAmount );
        if (IsFollowingPlayer) cameraTransform.SetPositionAndRotation(cameraTarget.position, targetRotation);
    }

    public override void Init() {
        cameraTransform.gameObject.SetActive(false);
        cameraTransform.parent = null;
        car.Path.OnRaceEnd += RaceEnd;
        offset = Quaternion.identity;
        IsFollowingPlayer = true;
    }

    public void ActivateCamera() => cameraTransform.gameObject.SetActive(!car.IsBot);

    private void RaceEnd(BaseCar _) {
        StartCoroutine(nameof(RaceEndAnimation));
    }

    private IEnumerator RaceEndAnimation() {
        yield return new WaitForSeconds(cameraAnimationDelay);
        offset = Quaternion.Euler(cameraAnimationEulerAngles);
    }
}

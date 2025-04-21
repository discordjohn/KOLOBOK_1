using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class VRBallMoverAdvanced : MonoBehaviour
{
    [Header("Настройки движения")]
    public float pushForce = 10f; // Сила толчка шара
    public float playerMoveSpeed = 2f; // Скорость движения игрока
    public float dampingFactor = 0.98f; // Коэффициент затухания скорости шара

    [Header("Настройки вращения шара")]
    public float rotationMultiplier = 50f; // Множитель для скорости вращения шара

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private Transform playerTransform; // Трансформ игрока (XR Origin)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
            enabled = false;
            return;
        }

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component not found on this GameObject.");
            enabled = false;
            return;
        }

        // Поиск Transform игрока по тегу (убедитесь, что у вашего XR Rig есть тег "Player")
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
        else
        {
            Debug.LogWarning("GameObject with tag 'Player' not found.");
        }

        // Подписываемся на события выбора (начала взаимодействия)
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        // Подписываемся на события отмены выбора (окончания взаимодействия)
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void FixedUpdate()
    {
        // Применяем затухание для естественного замедления шара
        rb.velocity *= dampingFactor;
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Получаем Interactor, который начал взаимодействие
        IXRInteractor interactor = args.interactorObject;

        XRController controller = null;

        if (interactor is XRDirectInteractor directInteractor)
        {
            controller = directInteractor.xrController as XRController; // Явное приведение с помощью as
        }
        else if (interactor is XRRayInteractor rayInteractor)
        {
            controller = rayInteractor.xrController as XRController; // Явное приведение с помощью as
        }

        if (controller != null)
        {
            if (controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 controllerVelocity))
            {
                // Применяем силу к шару
                rb.AddForce(controllerVelocity * pushForce, ForceMode.VelocityChange);

                // Перемещаем игрока в направлении движения шара
                if (playerTransform != null)
                {
                    Vector3 playerMoveDirection = Vector3.ProjectOnPlane(rb.velocity.normalized, Vector3.up); // Движение только по горизонтальной плоскости
                    playerTransform.Translate(playerMoveDirection * playerMoveSpeed * Time.fixedDeltaTime, Space.World);
                }

                // Добавляем вращение шару на основе скорости контроллера
                Vector3 angularVelocity = Vector3.Cross(transform.position - controller.transform.position, controllerVelocity) * rotationMultiplier;
                rb.angularVelocity = angularVelocity;
            }
            else
            {
                Debug.LogWarning("Could not get device velocity from controller: " + controller.name);
            }
        }
        else
        {
            Debug.LogWarning("Interactor is not an XRDirectInteractor/XRRayInteractor or XRController is null.");
        }
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        // Сбрасываем угловую скорость, когда контроллер отпускает шар (опционально)
        rb.angularVelocity = Vector3.zero;
    }
}
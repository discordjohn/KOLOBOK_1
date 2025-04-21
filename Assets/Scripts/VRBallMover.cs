using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class VRBallMover : MonoBehaviour
{
    public float pushForce = 10f; // Сила толчка шара
    public float playerMoveForceMultiplier = 5f; // Множитель силы для движения игрока
    public float dampingFactor = 0.98f; // Коэффициент затухания скорости шара

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private Rigidbody playerRb; // Rigidbody игрока

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

        // Поиск Rigidbody игрока по тегу (убедитесь, что у вашего XR Rig есть тег "Player")
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerRb = playerGO.GetComponent<Rigidbody>();
            if (playerRb == null && !playerGO.GetComponent<CharacterController>())
            {
                Debug.LogWarning("Rigidbody or CharacterController component not found on the Player GameObject.");
            }
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

        Vector3 controllerVelocity = Vector3.zero;

        // Попытка получить скорость из XRBaseControllerInteractor (для реальных контроллеров)
        if (interactor is XRBaseControllerInteractor controllerInteractor && controllerInteractor.xrController != null)
        {
            if (controllerInteractor.xrController is XRController xrController) // Явное приведение к XRController
            {
                if (xrController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out controllerVelocity))
                {
                    // Применяем силу к шару
                    rb.AddForce(controllerVelocity * pushForce, ForceMode.VelocityChange);

                    // Двигаем игрока в том же направлении
                    if (playerRb != null)
                    {
                        playerRb.AddForce(controllerVelocity * playerMoveForceMultiplier, ForceMode.VelocityChange);
                    }
                    else if (args.interactorObject is XRBaseControllerInteractor baseControllerInteractor && baseControllerInteractor.transform.parent != null)
                    {
                        // Попытка переместить Transform игрока, если нет Rigidbody (предполагаем, что XR Rig управляется Transform)
                        Transform playerTransform = baseControllerInteractor.transform.parent.parent; // Зависит от структуры вашего XR Rig
                        playerTransform.Translate(controllerVelocity * playerMoveForceMultiplier * Time.fixedDeltaTime, Space.World);
                    }
                    return; // Выходим, если скорость получена успешно
                }
                else
                {
                    Debug.LogWarning("Could not get device velocity from real controller: " + controllerInteractor.xrController.name);
                }
            }
            else
            {
                Debug.LogWarning("controllerInteractor.xrController is not of type XRController.");
            }
        }
        // Добавьте здесь логику для XRDeviceSimulator, если вы его используете для получения скорости

        Debug.LogWarning("Could not determine interactor type or get its velocity.");
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        // Действия при отпускании шара (можно оставить пустым, если не требуется)
    }
}
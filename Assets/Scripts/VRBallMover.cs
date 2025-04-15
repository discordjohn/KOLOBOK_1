using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRBallMover : MonoBehaviour
{
    public float pushForce = 10f; // Сила толчка
    public float dampingFactor = 0.98f; // Коэффициент затухания скорости

    private Rigidbody rb;
    private XRController leftController;
    private XRController rightController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
            enabled = false;
            return;
        }

        // Поиск контроллеров по тегу (убедитесь, что у ваших контроллеров есть тег "LeftController" и "RightController")
        GameObject leftControllerGO = GameObject.FindGameObjectWithTag("LeftController");
        GameObject rightControllerGO = GameObject.FindGameObjectWithTag("RightController");

        if (leftControllerGO != null)
        {
            leftController = leftControllerGO.GetComponent<XRController>();
            if (leftController == null)
            {
                Debug.LogWarning("LeftController component not found on the LeftController GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject with tag 'LeftController' not found.");
        }

        if (rightControllerGO != null)
        {
            rightController = rightControllerGO.GetComponent<XRController>();
            if (rightController == null)
            {
                Debug.LogWarning("RightController component not found on the RightController GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject with tag 'RightController' not found.");
        }
    }

    void FixedUpdate()
    {
        if (leftController != null)
        {
            CheckForPush(leftController);
        }
        if (rightController != null)
        {
            CheckForPush(rightController);
        }

        // Применяем затухание для естественного замедления шара
        rb.velocity *= dampingFactor;
    }

    void CheckForPush(XRController controller)
    {
        // Проверяем, находится ли контроллер в контакте с шаром
        if (IsControllerTouchingBall(controller))
        {
            // Получаем текущую скорость контроллера
            Vector3 controllerVelocity = GetControllerVelocity(controller);

            // Применяем силу к шару в направлении движения контроллера
            rb.AddForce(controllerVelocity * pushForce, ForceMode.VelocityChange);
        }
    }

    bool IsControllerTouchingBall(XRController controller)
    {
        // Проверяем столкновение между коллайдером контроллера и коллайдером шара
        // Вам может потребоваться добавить коллайдеры на модели контроллеров.
        // Для Action-based XR Rig используйте Interaction Manager и Interactor/Interactable.
        // Для Device-based/Legacy XR Rig добавьте Rigidbody и Collider на модели контроллеров с Is Trigger = true.

        Collider[] controllerColliders;
        if (controller.TryGetComponent(out CharacterController characterController))
        {
            controllerColliders = new Collider[] { characterController };
        }
        else
        {
            controllerColliders = controller.GetComponentsInChildren<Collider>();
        }

        Collider ballCollider = GetComponent<Collider>();

        foreach (var controllerCollider in controllerColliders)
        {
            if (controllerCollider.bounds.Intersects(ballCollider.bounds))
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetControllerVelocity(XRController controller)
    {
        // В зависимости от используемого SDK и настроек, получение скорости контроллера может отличаться.
        // Для OpenXR (Action-based):
        if (controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 velocity))
        {
            return velocity;
        }

        // Для OpenXR (Device-based) или других SDK может потребоваться другой способ получения скорости.
        // Например, отслеживание положения в предыдущем кадре и вычисление разницы.
        // Или использование специфичных API SDK.
        Debug.LogWarning("Could not get controller velocity for " + controller.name);
        return Vector3.zero;
    }
}
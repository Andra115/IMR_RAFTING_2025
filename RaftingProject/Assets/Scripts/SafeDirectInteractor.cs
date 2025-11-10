using UnityEngine;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor))]
public class SafeDirectInteractor : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor interactor;

    void Awake()
    {
        interactor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
    }

    public void SafeDisable()
    {
        if (interactor != null && interactor.isActiveAndEnabled)
            interactor.enabled = false;
    }

    public void SafeEnable()
    {
        if (interactor != null && !interactor.isActiveAndEnabled)
            interactor.enabled = true;
    }
}
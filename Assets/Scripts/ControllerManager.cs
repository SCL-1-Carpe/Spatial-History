using UnityEngine;
using MagicLeap;
using UnityEngine.XR.MagicLeap;
using ML_C_Intensity = UnityEngine.XR.MagicLeap.MLInputControllerFeedbackIntensity;
using ML_C_Vibe = UnityEngine.XR.MagicLeap.MLInputControllerFeedbackPatternVibe;

public class ControllerManager : ControllerFeedbackExample
{
    private MainManager mainManager;
    [SerializeField] ControllerConnectionHandler controllerConnectionHandler;


    void Start()
    {
        mainManager = GetComponent<MainManager>();
        MLInput.OnControllerButtonUp += HandleOnButtonUp;
        MLInput.OnControllerButtonDown += HandleOnButtonDown;
        MLInput.OnTriggerDown += HandleOnTriggerDown;
    }

    void Update()
    {
    }

    public override void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        MLInputController controller = controllerConnectionHandler.ConnectedController;
        if (controller != null && controller.Id == controllerId && button == MLInputControllerButton.Bumper)
            mainManager.ML_OnBumperButton();
        else if (controller != null && controller.Id == controllerId && button == MLInputControllerButton.HomeTap)
            mainManager.AllInit();
    }

    public override void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
    {
        MLInputController controller = controllerConnectionHandler.ConnectedController;
        if (controller != null && controller.Id == controllerId &&
            button == MLInputControllerButton.Bumper)
        {
            controller.StartFeedbackPatternVibe(ML_C_Vibe.ForceUp, ML_C_Intensity.Medium);
        }
    }

    public override void HandleOnTriggerDown(byte controllerId, float value)
    {
        MLInputController controller = controllerConnectionHandler.ConnectedController;
        if (controller != null && controller.Id == controllerId)
        {
            ML_C_Intensity intensity = (ML_C_Intensity)((int)(value * 1.0f));
            controller.StartFeedbackPatternVibe(ML_C_Vibe.Buzz, intensity);
            mainManager.ML_OnTriggerDown();
        }
    }
}

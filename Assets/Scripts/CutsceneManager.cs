using Unity.Cinemachine;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private CinemachineSplineDolly startCutsceneDolly;

    private bool isStartCutsceneEnding;

    private void Start()
    {
        isStartCutsceneEnding = false;
        startCutsceneDolly.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isStartCutsceneEnding)
        {
            if (startCutsceneDolly.CameraPosition > 0.9f)
            {
                isStartCutsceneEnding = true;
                GameManager.I.worldMenu.BlackFadeIn(3, EndAction);

                void EndAction()
                {
                    startCutsceneDolly.gameObject.SetActive(false);
                    GameManager.I.StartGame();
                }
            }
        }
    }
}

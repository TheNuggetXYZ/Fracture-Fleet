using Unity.Cinemachine;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private bool skipStartCutscene = false;

    [SerializeField] private CinemachineSplineDolly startCutsceneDolly;
    [SerializeField] private AudioSource startCutsceneSFX;

    private bool isStartCutsceneEnding;

    private void Start()
    {
        if (!skipStartCutscene)
        {
            isStartCutsceneEnding = false;
            startCutsceneDolly.gameObject.SetActive(true);
            startCutsceneSFX.Play();
        }
    }

    private void Update()
    {
        if (!isStartCutsceneEnding && !skipStartCutscene)
            StartCutsceneUpdate();
    }

    private void StartCutsceneUpdate()
    {
        if (startCutsceneDolly.CameraPosition > 0.9f)
        {
            isStartCutsceneEnding = true;
            GameManager.I.worldMenu.BlackFadeIn(3, StartCutsceneEndAction);
        }
    }
    
    private void StartCutsceneEndAction()
    {
        startCutsceneDolly.gameObject.SetActive(false);
        GameManager.I.StartGame();
    }
}

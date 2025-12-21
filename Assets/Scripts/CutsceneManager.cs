using Unity.Cinemachine;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private bool skipStartCutscene = false;

    [SerializeField] private CinemachineSplineDolly startCutsceneDolly;
    [SerializeField] private AudioSource startCutsceneSFX;

    private bool isStartCutsceneEnding;
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }
    
    private void Start()
    {
        if (!skipStartCutscene)
        {
            isStartCutsceneEnding = false;
            startCutsceneDolly.gameObject.SetActive(true);
            startCutsceneSFX.Play();
        }
        else
            game.StartGame();
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
            game.worldMenu.BlackFadeIn(3, StartCutsceneEndAction);
        }
    }
    
    private void StartCutsceneEndAction()
    {
        Destroy(startCutsceneDolly.gameObject);
        game.StartGame();
    }
}

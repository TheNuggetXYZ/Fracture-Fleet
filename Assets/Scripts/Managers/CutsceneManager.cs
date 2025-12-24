using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class CutsceneManager : MonoBehaviour
{
    // TODO: cutscene speed based on fps, low fps = fast
    
    
    [Header("Start Cutscene")]
    [SerializeField] private bool skipStartCutscene = false;

    [SerializeField] private CinemachineSplineDolly startCutsceneDolly;
    [SerializeField] private float startCutsceneDollySpeed;
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
        StartCutsceneUpdate();
    }

    private void StartCutsceneUpdate()
    {
        if (!isStartCutsceneEnding && !skipStartCutscene && startCutsceneDolly.CameraPosition > 0.6f)
        {
            isStartCutsceneEnding = true;
            game.worldMenu.BlackFadeIn(2.5f, StartCutsceneEndAction);
        }
        
        startCutsceneDolly.CameraPosition += startCutsceneDollySpeed * Time.deltaTime;
    }
    
    private void StartCutsceneEndAction()
    {
        Destroy(startCutsceneDolly.gameObject);
        game.StartGame();
    }
}

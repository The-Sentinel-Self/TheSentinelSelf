using UnityEngine;

/*
    RestartScene - hard reboot of the current scene.

    CleanupSimulation - not yet implemented
        the idea is to provide a softer option over a hard reboot
        clean up any possible memory leaks and reduces the current complexity
        of the simulation
*/
public enum SafetyBehavior {RestartScene, CleanupSimulation};

public class SafetyWatchdog : MonoBehaviour
{
    [Header("Watchdog Settings")]

    [Tooltip("FPS threshold below which watchdog kicks in")]
    [Range(5.0f, 40.0f)]
    public float fpsThreshold = 15.0f;

    [Tooltip("Minutes below fps threshold when watchdog activates")]
    [Range(0.1f, 30.0f)]
    public float minutesUnderThreshold = 10.0f;

    public SafetyBehavior behavior = SafetyBehavior.RestartScene;

    [Space(10)]
    [Header("Debug")]
    public float currentFps = 1000.0f;
    private float lastFps = 1000.0f;
    
    private float timeThreshold; // in seconds, calculated on start


    [SerializeField]
    private bool timerRunning = false;

    [SerializeField]
    private float timerTime = 0.0f;

    private float calculateEvery = 1.0f; // in seconds, used to get an avg fps
    private int framesCount = 0;
    private float framesTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        timeThreshold = minutesUnderThreshold * 60.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timerTime += Time.unscaledDeltaTime;
        framesTime += Time.unscaledDeltaTime;
        framesCount++;

        if(timerRunning && ( timerTime >= timeThreshold )) {
            Debug.Log("REBOOT THE SCENE!");
            timerRunning = false;
            timerTime = 0.0f;
            lastFps = 100.0f;
        }

        if(framesTime >= 1.0f) {
            // Calculate avg fps
            lastFps = currentFps;
            currentFps = framesCount / framesTime;
            framesCount = 0;
            framesTime -= 1.0f;
            
            if(currentFps <= fpsThreshold) {
                if(lastFps > fpsThreshold) {
                    // Start timer...
                    timerRunning = true;
                    timerTime = 0.0f;
                }
            } else {
                if(lastFps <= fpsThreshold) {
                    timerRunning = false;
                    timerTime = 0.0f;
                }
            }
        }     
    }
}

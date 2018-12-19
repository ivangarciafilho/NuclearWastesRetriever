using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EnhancedBehaviours;
using FishFlock;

public class SubmarineGameManager : MyMonoBehaviour.MyGameManager
{
    public static AsyncOperation loadingNuclearWastes       { get; private set; }
    public static AsyncOperation loadingScenario            { get; private set; }
    public static AsyncOperation loadingSecondarySystems    { get; private set; }
    public static AsyncOperation loadingRenderingSystems    { get; private set; }

    public GameObject fishFlockObject;
    public FishFlockControllerGPU fishFlockController;

    public static SubmarineGameManager instance;

    public static bool paused = false;

    public enum AuraMode { NO_AURA, AURA };
    public AuraMode auraMode = AuraMode.AURA;

    private IEnumerator Start()
    {
        instance = this;

        Time.timeScale = 0;
        Application.backgroundLoadingPriority = ThreadPriority.High;
        loadingNuclearWastes = SceneManager.LoadSceneAsync("NuclearWastes", LoadSceneMode.Additive);
        loadingScenario = SceneManager.LoadSceneAsync("ProceduralSea", LoadSceneMode.Additive);
        loadingSecondarySystems = SceneManager.LoadSceneAsync("SecondarySystems", LoadSceneMode.Additive);
        loadingScenario.priority = 4;
        while (!loadingNuclearWastes.isDone)    yield return null;
        while (!loadingScenario.isDone)         yield return null;
        while (ProceduralReefsGenerator.proceduralReefsGeneratorInstance == null) yield return null;
        while (ProceduralReefsGenerator.isBusy) yield return null;

        loadingRenderingSystems = SceneManager.LoadSceneAsync(auraMode == AuraMode.AURA ? "RenderingSystems" : "RenderingSystemsNoAura", LoadSceneMode.Additive);
        while (!loadingRenderingSystems.isDone) yield return null;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(("MainSystems")));

        Time.timeScale = 1;
        StartCoroutine(Post_Start_Fishes());
    }

    IEnumerator Post_Start_Fishes()
    {
        yield return new WaitForSeconds(3f);

        WasteDetectionSystem.NuclearWaste closestNuclearWaste = WasteDetectionSystem.closestNuclearWaste;
        fishFlockController.target = closestNuclearWaste.itsTransform;
        fishFlockObject.SetActive(true);

        StartCoroutine(Custom_Fish_Behaviours());
        StartCoroutine(Nuclear_Waste_Cast_For_Colliders());
    }

    IEnumerator Custom_Fish_Behaviours()
    {
        yield return new WaitForSeconds(0.1f);

        float fish_dist = fishFlockController.neighbourDistance;
        while(Application.isPlaying && fishFlockController != null)
        {
            float rng = Random.Range(fish_dist - 0.4f, fish_dist + 0.4f);
            fishFlockController.neighbourDistance = rng;

            yield return new WaitForSeconds(Random.Range(3, 5));
        }
    }

    IEnumerator Nuclear_Waste_Cast_For_Colliders()
    {
        yield return new WaitForSeconds(0.1f);

        bool checked_colls = false;
        bool sleeping = false;
        while (Application.isPlaying && fishFlockController != null)
        {
            WasteDetectionSystem.NuclearWaste closestNuclearWaste = WasteDetectionSystem.closestNuclearWaste;
            Rigidbody nuclearWasteRigidbody = closestNuclearWaste.itsRigidbody;
            sleeping = nuclearWasteRigidbody.IsSleeping();

            if (sleeping && !checked_colls)
            {
                var colls = Physics.OverlapSphere(closestNuclearWaste.itsTransform.position, 10.0f);
                for (int i = 0; i < colls.Length; i++)
                {
                    // add colliders to the fish controller
                }

                checked_colls = true;
            }
            else if(!sleeping && checked_colls)
            {
                checked_colls = false;
            }

            yield return null;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!fishFlockObject.activeInHierarchy) return;

        WasteDetectionSystem.NuclearWaste closestNuclearWaste = WasteDetectionSystem.closestNuclearWaste;

        fishFlockController.target = closestNuclearWaste.itsTransform;

        if (Input.GetKeyDown(KeyCode.Escape) && Tutorial.completedTutorial)
        {
            paused = !paused;

            if (paused)
            {
                SubmarineGUI.instance.guiObject.SetActive(true);
            }
            else
            {
                SubmarineGUI.instance.guiObject.SetActive(false);
            }
        }
    }
}

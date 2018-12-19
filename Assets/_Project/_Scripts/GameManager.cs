using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EnhancedBehaviours;

public class GameManager : MyMonoBehaviour.MyGameManager
{
    public static AsyncOperation loadingNuclearWastes       { get; private set; }
    public static AsyncOperation loadingScenario            { get; private set; }
    public static AsyncOperation loadingSecondarySystems    { get; private set; }
    public static AsyncOperation loadingRenderingSystems    { get; private set; }

    private IEnumerator Start()
    {
        Time.timeScale = 0;
        loadingNuclearWastes = SceneManager.LoadSceneAsync("NuclearWastes", LoadSceneMode.Additive);
        loadingScenario = SceneManager.LoadSceneAsync("ProceduralSea", LoadSceneMode.Additive);
        loadingSecondarySystems = SceneManager.LoadSceneAsync("SecondarySystems", LoadSceneMode.Additive);
        while (!loadingNuclearWastes.isDone)    yield return null;
        while (!loadingScenario.isDone)         yield return null;
        while (ProceduralReefsGenerator.proceduralReefsGeneratorInstance == null) yield return null;
        while (ProceduralReefsGenerator.isBusy) yield return null;
        loadingRenderingSystems = SceneManager.LoadSceneAsync("RenderingSystems", LoadSceneMode.Additive);
        while (!loadingRenderingSystems.isDone) yield return null;
        Time.timeScale = 1;
    }
}

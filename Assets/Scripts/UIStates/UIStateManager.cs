using UnityEngine;
using System.Collections.Generic;

public class UIStateManager : MonoBehaviour
{
    private UIStateBase currentState;
    private Dictionary<string, GameObject> uiCache = new Dictionary<string, GameObject>();

    [SerializeField] private Transform uiRoot; // Donde se instancian los prefabs (Canvas/Panel principal)
    void Start()
    {
        ChangeState("Main"); // o "Main" mientras pruebas
        Debug.Log("Inicializando estado en el Main");
    }

    public void ChangeState(string stateName)
    {
        if (currentState != null)
        {
            currentState.OnExit();
            Destroy(currentState.gameObject);
        }

        GameObject prefab = LoadUIPrefab(stateName);
        if (prefab == null)
        {
            Debug.LogError($"UI prefab not found for state: {stateName}");
            return;
        }

        GameObject instance = Instantiate(prefab, uiRoot);
        currentState = instance.GetComponent<UIStateBase>();
        currentState.Init(this);
        currentState.OnEnter();
    }

    private GameObject LoadUIPrefab(string stateName)
    {
        if (uiCache.ContainsKey(stateName))
            return uiCache[stateName];

        GameObject prefab = Resources.Load<GameObject>($"UI/{stateName}UI");
        if (prefab != null)
            uiCache[stateName] = prefab;

        return prefab;
    }
}

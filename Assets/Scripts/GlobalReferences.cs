using Unity.VisualScripting;
using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
public static GlobalReferences Instance {get; set; }
public GameObject bulletImpactEffectPrefab;
public GameObject grenadeExplosionEffect;
public GameObject flashBlindEffect;
public GameObject smokePlumeEffect;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

}

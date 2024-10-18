using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject flashHolder;
    public Sprite[] flashSprites;
    public SpriteRenderer[] SpriteRenderers;

    public float flashTime;

    private void Start()
    {
        DeActivate();
    }
    public void Activate()
    {
        flashHolder.SetActive(true);
        
        for (int i = 0; i < SpriteRenderers.Length; i++)
        {
            int flashSpriteIndex = Random.Range(0, flashSprites.Length);
            SpriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
        }
        Invoke("DeActivate", flashTime);
    }
    void DeActivate()
    {
        flashHolder.SetActive(false);
    }
}

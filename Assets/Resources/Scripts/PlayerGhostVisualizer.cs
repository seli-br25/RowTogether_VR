using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhostVisualizer : MonoBehaviour
{

    public bool local = false;
    public Material playerHeadMaterial;
    public Material ghostHeadMaterial;
    public Renderer headRender;
    public Material playerHandMaterial;
    public Material ghostHandMaterial;
    public Renderer leftHandRenderer;
    public Renderer rightHandRenderer;


    System.Collections.IEnumerator Start()
    {

        if (local)
        {
            yield return new WaitForEndOfFrame();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.gameObject.name == "hands:Lhand")
                {
                    leftHandRenderer = r;

                }
                if (r.gameObject.name == "hands:Rhand")
                {
                    rightHandRenderer = r;
                }
            }
        }
    }

    //public void Start()
    //{
    //    if (local)
    //    {
    //        Transform leftHandChild = this.transform.Find("Left Controller");
    //        Debug.Log(leftHandChild);
    //        if (leftHandChild != null)
    //        {
    //            leftHandRenderer = leftHandChild.GetComponentInChildren<Renderer>();
    //        }

    //        Transform rightHandChild = transform.Find("hands:Rhand");
    //        if (rightHandChild != null)
    //        {
    //            rightHandRenderer = rightHandChild.GetComponentInChildren<Renderer>();
    //        }
    //    }
    //}


    public void SetGhost()
    {
        if (headRender != null) 
        {
            headRender.material = ghostHeadMaterial;
        }

        if (leftHandRenderer != null)
        {
            leftHandRenderer.material = ghostHandMaterial;
        }

        if (rightHandRenderer != null)
        {
            rightHandRenderer.material = ghostHandMaterial;
        }
    }

    public void SetPlayer()
    {
        if (headRender != null)
        {
            headRender.material = playerHeadMaterial;
        }
        if (leftHandRenderer != null)
        {
            leftHandRenderer.material = playerHandMaterial;
        }
        if (rightHandRenderer != null)
        {
            rightHandRenderer.material = playerHandMaterial;
        }
    }
}

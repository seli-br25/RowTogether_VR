using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableNetworked : XRGrabInteractable
{



    public Transform originalSceneParent;
    private PhotonView photonView;

    public void Start()
    {
        photonView = GetComponent<PhotonView>();
    }


    [SerializeField]
    private bool m_ChangeTransformParent = true;
    /// <summary>
    /// Whether to set the parent of this object when this object is grabbed.
    /// </summary>
    public bool ChangeTransformParent
    {
        get => m_ChangeTransformParent;
        set => m_ChangeTransformParent = value;
    }

    // Override Grab method
    protected override void Grab()
    {
        base.Grab();

        if (!m_ChangeTransformParent) 
        {
            transform.SetParent(originalSceneParent);
        }

    }

    // Override Drop method
    protected override void Drop()
    {
        base.Drop();
        if (!m_ChangeTransformParent) { 
        }
    }
}

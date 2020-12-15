using UnityEngine;
using UnityEngine.UI;

public class HintObject : MonoBehaviour
{
    public enum HintType { proximity, interaction, permanent };

    public string hintID;

    public HintType type;
    public string title;
    public string hintText;

    public GameObject hintPrefab;
    GameObject hintObject;
    Vector3 targetPos;
    public Vector3 offset = Vector3.zero;
    bool canShow = false;
    bool showing = false;

    private void Awake()
    {
        if (HintManager.IsComplete(hintID))
        {
            Destroy(gameObject);
            return;
        }

        hintObject = GameObject.Instantiate(hintPrefab);
        hintObject.transform.position = transform.position;
        Text[] texts = hintObject.GetComponentsInChildren<Text>();
        texts[0].text = title;
        texts[1].text = hintText;

        hintObject.SetActive(type == HintType.permanent);
    }

    private void OnEnable()
    {
        HintManager.OnHintChanged += OnHintChanged;

    }

    private void OnDisable()
    {
        HintManager.OnHintChanged -= OnHintChanged;
    }

    public void OnHintChanged(string id, bool complete)
    {
        Debug.Log("onHintChanged" + id);
        if (string.Compare(id, hintID) == 0)
        {
            HintManager.SetShowing(id, false);
            Destroy(hintObject);
            Destroy(gameObject);
        }
    }


    public void InteractionComplete()
    {
        HintManager.SetComplete(hintID, true);
    }


    private void Update()
    {
        if (canShow && !HintManager.GetShowing(hintID))
        {
            hintObject.SetActive(true);
            HintManager.SetShowing(hintID, true);
            showing = true;
            // hint object should be front-right of camera
            Quaternion cameraYRot = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
            targetPos = transform.position + cameraYRot * offset;
            canShow = false;
        }
        hintObject.transform.position = Vector3.Lerp(hintObject.transform.position, targetPos, 0.4f);
        Vector3 fromHint = Camera.main.transform.position - hintObject.transform.position;
        hintObject.transform.rotation = Quaternion.Lerp(hintObject.transform.rotation, Quaternion.LookRotation(-fromHint, Vector3.up), 0.4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hands"))
        {
            canShow = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hands"))
        {
            hintObject.SetActive(false);
            canShow = false;
            if (showing)
            {
                HintManager.SetShowing(hintID, false);
                showing = false;
            }
        }
    }
}
using UnityEngine;

public class UIDamageText : MonoBehaviour
{
    void Update()
    {
        transform.localScale += new Vector3(3, 3, 3) * Time.deltaTime;
        transform.position += new Vector3(0, 1, 0) * Time.deltaTime;
    }
}

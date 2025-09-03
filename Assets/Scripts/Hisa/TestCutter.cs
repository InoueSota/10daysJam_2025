using UnityEngine;

public class TestCutter : MonoBehaviour
{

    [SerializeField] TrailCutMover trailPrefab;

    [SerializeField] ScreenAxisSlicer_Snapshot snapshot;
    [SerializeField] GameObject obj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {


            //trailCutMover.Play(snapshot.tmpP0, snapshot.tmpP1,0.5f);

            TrailCutMover go = Instantiate(trailPrefab);
            go.gameObject.SetActive(true);   // Ç±ÇÍïKê{
            go.Play(snapshot.tmpP0, snapshot.tmpP1, 0.5f);

        }

        if (Input.GetMouseButton(0)|| Input.GetMouseButton(1)) {
            obj.SetActive(false);
        }
    }
}

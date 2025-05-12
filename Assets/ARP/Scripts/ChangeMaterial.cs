using UnityEngine;


public class ChangeMaterial : MonoBehaviour
{
    [SerializeField] SequenceInfoOfInteractObj SequenceInfo;
    public Material outline_material;
    public Material material;
    public Renderer renderer;
    public int alpha;
    private SequenceInfoOfInteractObj _sequenceInfo;
    
    private bool hasRun = false;
    void Start() {
        renderer = GetComponent<Renderer>();
    }
    void Update()
    {
        if (!hasRun)
        {
            _sequenceInfo = SequenceInfoOfInteractObj.Instance;
            hasRun = true;
        }
        if (_sequenceInfo.currentStep == alpha)
        {
            renderer.material =outline_material;
        }
        else
        {
            renderer.material = material;
        }
        
        

    }
    
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControllScript : MonoBehaviour
{
    private List<GhostController> ghosts;
    private RobotController robot;
    private bool robotReady = false;
    public Transform Ghostspawner;

    private readonly struct WeightedGhost {
        public WeightedGhost(float weight, GhostController ghost) {
            this.weight = weight;
            position = ghost.GetPosition();
            rotation = ghost.GetQuaternion();
        }
        private readonly float weight;
        private readonly Vector3 position;
        private readonly Quaternion rotation;

        public float GetWeight(){
            return weight;
        }

        public Vector3 GetPosition(){
            return position;
        }

        public Quaternion GetRotation(){
            return rotation;
        }
    }
    
    private void ChooseGhosts(List<WeightedGhost> weightedGhosts){
        float sumOfWeights = weightedGhosts.Sum(ghost => ghost.GetWeight());

        for(int i = 0; i < weightedGhosts.Count; i++){
            float chooser = Random.Range(0f, sumOfWeights);
            
            for (int j = 0; j < weightedGhosts.Count; j++){
                chooser -= weightedGhosts[j].GetWeight();
                if (chooser <= 0){
                    ghosts[i].SetPosition(weightedGhosts[j].GetPosition(), weightedGhosts[j].GetRotation());
                    break;
                }
            }
        }
    }

    private float CalculateWeight(float robotDistance, float ghostDistance, float variance = 0.4f){
        
        float alpha = 1 / variance * Mathf.Sqrt(Mathf.PI * 2);
        float exponent = -1 * Mathf.Pow(robotDistance - ghostDistance, 2) / 2 * Mathf.Pow(variance, 2);
        float weight = alpha * Mathf.Exp(exponent);
        return weight;
        
        /*
        float diff = Mathf.Abs(robotDistance - ghostDistance);
        if (diff < 0.2f)
            return 14;
        if (diff < 0.6f)
            return 13;
        if (diff < 1)
            return 12;
        if (diff < 2)
            return 11;
        return 1;
        */
    }

    private float Scan(int numberOfScans){
        float sum = 0;

        for (int i = 0; i < numberOfScans; i++){
            float scan;
            do{
                scan = robot.Scan();
            } while (scan == 40.0f);
            sum += scan;
        }

        return sum / numberOfScans;
    }

    void Start() {
        for (int i = 0; i < 5000; i++){
            CreateRandomGhost(Ghostspawner);
        }
        
    }

    void Update(){
        
        if (robotReady){

            float robotDistance = Scan(3);
            List<WeightedGhost> weightedGhosts = new List<WeightedGhost>();
            
            // Gewichtung der Ghosts und Entfernung aus der Liste
            foreach (GhostController ghost in ghosts){
                weightedGhosts.Add(new WeightedGhost(
                        CalculateWeight(robotDistance, ghost.GetDistance()),
                        ghost
                    )
                );
            }
            // Ziehung der Ghosts
            ChooseGhosts(weightedGhosts);
            
            // Bewegung des Roboters und Geister
            GenerateCommand(robotDistance);
            
            //robot.CompareLocations(ghosts[0].GetPosition(), ghosts[0].GetRotation());
            Debug.Log("Distance according to sensor: " + robotDistance);
            
            robotReady = false;
        }
    }

    private void GenerateCommand(float robotDistance){

        if (robotDistance > 4f){
            //Move
            robot.Move(2f, 50f);
            ghosts.ForEach(ghost => ghost.Move(Random.Range(1f,3f)));
        }
        else{
            //Rotate
            robot.Rotate(7);
            ghosts.ForEach(ghost => ghost.Rotate(Random.Range(4,28)));
        }
            
    }

    private void CreateRandomGhost(Transform trans){
        float randomX = Random.Range(-10, 10);//-50,50
        float randomZ = Random.Range(-10, 40);//-100,100
        //trans.Rotate(0, 96.123f, 0);
        var position = new Vector3(randomX, 0.5f, randomZ);
        Instantiate(Ghost, position, Ghost.transform.rotation);
    }

    private void CreateGhost(Vector3 position, Quaternion rotation){
        Instantiate(Ghost, position, rotation);
    }

    #region Stuff You Shouldn't Touch
    private static ControllScript self;
    public GameObject Ghost;
    void Awake()
    {
        if (self)
            Destroy(this);
        else
            self = this;
        ghosts = new List<GhostController>();
    }

    void OnDestroy()
    {
        self = null;
    }

    public static ControllScript GetInstance()
    {
        return self;
    }

    public void RegisterRobot(RobotController robot)
    {
        this.robot = robot;
    }
    public void RegisterGhost(GhostController ghost)
    {
        ghosts.Add(ghost);
    }
    public void DeRegisterGhost(GhostController ghost)
    {
        ghosts.Remove(ghost);
    }
    public void notifyRobotReady()
    {
        robotReady = true;
    }
    #endregion

}

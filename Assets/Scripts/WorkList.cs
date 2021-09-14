using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WorkList : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        foreach(WeighterObject weighterObject in WeighterObject.weighterObjects){
            Character character =  weighterObject.gameObject.GetComponent<Character>();
            if (character != null && character.workplace != null){
            
                for(int id = 0;;id++){
                    if (workplaces[id]==character.workplace){
                        characterToHisId.Add(character,id);
                        break;
                    }
                }
            }
        }
    }
    void Awake()
    {
        singleton = this;
        achieved = new float[phrases.Length];
    }
    public Workplace[] workplaces;
    Dictionary<Character, int> characterToHisId = new Dictionary<Character, int>();
    public string[] phrases;
    public int[] goals;
    float[] achieved;
    public static WorkList singleton;
    public Text text;

    // Update is called once per frame
    void Update()
    {
        bool missionComplete = true;
        string newText = "";
        foreach(Character character in characterToHisId.Keys){
            int id=0;
            characterToHisId.TryGetValue(character, out id);
            if (character.previouslyWorked){
                achieved[id]+=Time.deltaTime;
            }
            int realAchieved = (int)(achieved[id]/House.inGameHour);
            if (realAchieved < goals[id])
                missionComplete=false;
            newText += phrases[id] + "(" + realAchieved.ToString() + " / " + goals[id].ToString() + ")\n";
        }
        text.text = newText;
        if (missionComplete){
            // TODO something
            Debug.Log("YAY!");
        }
    }
}
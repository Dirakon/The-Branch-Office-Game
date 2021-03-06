using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class House : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        singleton = this;
    }
    public string nextLevel = "";
    void Start()
    {
    }
    public bool gameOver = false;
    public float gameOverDistance = 30f;
    public float gameOverSpeed = 5f;
    public float horizontalRadius;
    public void LoadNextScene(){
        WeighterObject.weighterObjects = null;
        MovingObject.movingObjects = null;
        SceneManager.LoadScene(nextLevel,LoadSceneMode.Single);
    }
    public static float inGameHour = 10f;
    public IEnumerator GameOver(Vector3 direction, bool skipScene = false)
    {
        gameOver = true;
        if (!skipScene)
        {
            float t = 0;
            Vector3 start = transform.position;
            int sign = direction == transform.right ? -1 : 1;
            while (t < 1)
            {
                t += (gameOverSpeed * Time.deltaTime);
                if (t > 1)
                    t = 1;
                Vector3 angle = transform.rotation.eulerAngles;
                angle.z += sign * 200f * massMultiplier * Time.deltaTime;
                transform.rotation = Quaternion.Euler(angle);

                transform.localPosition = Vector3.Lerp(start, transform.position + gameOverDistance * sign * (-transform.right), t);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(2f);
        }
        WeighterObject.weighterObjects = null;
        MovingObject.movingObjects = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
    public static House singleton;
    public float massMultiplier;
    public float minutesOnLevel;
    public void FixedUpdate()
    {

    }
    public float capProjection(float projection,float leeway =0f)
    {
        return Mathf.Abs(projection) < House.singleton.horizontalRadius + leeway ? projection : (House.singleton.horizontalRadius+ leeway ) * (projection / Mathf.Abs(projection));
    }
    public float GetProjection(Vector3 vectorToProject)
    {
        return GetProjection(vectorToProject, false);
    }
    public float GetProjection(Vector3 vectorToProject, bool debug)
    {
        float ans = Vector3.Dot(transform.right, vectorToProject);
        if (debug)
            Debug.Log(ans);
        return ans;
    }
    // Update is called once per frame
    void Update()
    {
        if (WeighterObject.weighterObjects == null)
            return;
        if (WeighterObject.weighterObjects[0] == null){
            // some bug, for some reason weighters didn't update
            
        WeighterObject.weighterObjects = null;
        MovingObject.movingObjects = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
   
        }
        if (gameOver)
            return;
        minutesOnLevel -= Time.deltaTime;
        Clock.singleton.setTime(minutesOnLevel);
        float angularVelocity = 0f;
        foreach (WeighterObject weighterObject in WeighterObject.weighterObjects)
        {
            Vector3 difference = weighterObject.transform.position - transform.position;
            angularVelocity += GetProjection(difference) * weighterObject.relativeMass;
        }
        Vector3 angle = transform.rotation.eulerAngles;
        angle.z -= angularVelocity * massMultiplier * Time.deltaTime;
        while (angle.z >= 90)
            angle.z -= 180;
        while (angle.z < -90)
            angle.z += 180;
        foreach (MovingObject movingObject in MovingObject.movingObjects)
        {
            if (!movingObject.movingAllowed)
                continue;
            float projection = GetProjection(movingObject.gameObject.transform.position - House.singleton.transform.position);
            float leeway = projection > 0? movingObject.rightSideBaseSize : movingObject.leftSideBaseSize;
            float newprojection = capProjection(projection - angle.z * movingObject.speedModifier * Time.deltaTime,leeway);
            if (Mathf.Abs(newprojection) < horizontalRadius+leeway)
                movingObject.gameObject.transform.position += transform.right * (newprojection - projection);
        }
        transform.rotation = Quaternion.Euler(angle);
        if (Mathf.Abs(angle.z) >= 23 || minutesOnLevel <= 0 || Input.GetKeyDown(KeyCode.R)){
            SoundManager.singleton.fallingSound.Play();
            SoundManager.singleton.clockSound.Stop();
            Debug.Log("Death apparently");
            gameOver=true;
            StartCoroutine(GameOver(angle.z < 0 ? transform.right : -transform.right,minutesOnLevel<=0|| Input.GetKeyDown(KeyCode.R)));
        }
    }
}

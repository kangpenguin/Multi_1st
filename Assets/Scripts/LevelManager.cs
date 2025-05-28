using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviourPun
{
    public Animator sceneLoaderanimator; //꼭 개선하고 싶은 내용이다.
    //UI매니저를 통해 했어야했다. 씬 로더는 이 씬에만 쓰는 게 아니기 때문이다.

    float changeTIme = 2.3f;

    int index;

    public void ChangeLevel()
    {
        index = SceneManager.GetActiveScene().buildIndex + 1;

        if (SceneManager.GetActiveScene().buildIndex == 2)
            index = 1;

        sceneLoaderanimator.SetTrigger("fade");
    }

    IEnumerator photonLoad()
    {
        yield return new WaitForSeconds(changeTIme);

        PhotonNetwork.LoadLevel(index);
    }

    IEnumerator sceneLoad()
    {
        yield return new WaitForSeconds(changeTIme);

        SceneManager.LoadScene(index);
    }

    public void LoadLevel()
    {
        StartCoroutine("ChangeLevel");
    }

    public void StartphotonLoad()
    {
        StartCoroutine("photonLoad");
    }

    public void StartsceneLoad()
    {
        StartCoroutine("sceneLoad");
    }
}

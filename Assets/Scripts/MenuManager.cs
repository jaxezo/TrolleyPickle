using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public TMP_Dropdown drop_FrameworkSelect;
    public Animation anim_GameStart;
    private EthicalFrameworks.Frameworks targettedFramework;
    // Start is called before the first frame update
    void Start()
    {
        int loadedFramework = PlayerPrefs.GetInt ("TargetFramework", 0);
        targettedFramework = (EthicalFrameworks.Frameworks)loadedFramework;

        drop_FrameworkSelect.value = loadedFramework;

        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame ()
    {
        PlayerPrefs.SetInt ("TargetFramework", (int)targettedFramework);
        
        StartCoroutine (LoadGame());
    }
    public void ChangeTargettedFramework (int value)
    {
        targettedFramework = (EthicalFrameworks.Frameworks)value;
    }
    private IEnumerator LoadGame ()
    {
        anim_GameStart.Play("MenuStart");

        yield return new WaitForSeconds (anim_GameStart.GetClip("MenuStart").length);

        SceneManager.LoadScene (1);
    }
}

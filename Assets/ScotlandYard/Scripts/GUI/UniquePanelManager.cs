using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UniquePanelManager : MonoBehaviour
{
    public static UniquePanelManager Instance { get; private set; }

    public UniquePanelEnsurer MainMenuPanel;

    List<UniquePanelEnsurer> panels = new List<UniquePanelEnsurer>();
    UniquePanelEnsurer lastPanel;

    void Awake()
    {
        Instance = this;

    }

    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            StartCoroutine(coOnEnable());
        }
    }

    public void OnEnablePanel(UniquePanelEnsurer panel)
    {
        if (!panels.Contains(panel))
            panels.Add(panel);

        if (panel == lastPanel)
            return;

        StopCoroutine(coOnEnable());
        StartCoroutine(coOnEnable());

        lastPanel = panel;
    }

    IEnumerator coOnEnable()
    {
        while (panels.FirstOrDefault(o => o.enabled && o.Animation != null && o.Animation.enabled) != null)
            yield return null;

        yield return null;
        yield return null;
        yield return null;

        panels.Sort();

        int notDisabledPanel = 0;

        for (int i = panels.Count - 1; i >= 0; i--)
        {
            var p = panels[i];
            if(i == notDisabledPanel)
                continue;

            if (!p.gameObject.activeSelf)
                continue;

            this.LogDebug(p.name + " - Disable");
            //p.Deactivate();

        }

        yield return null;

        if (panels.FirstOrDefault(o => o.gameObject.activeSelf) == null)
        {
            this.LogWarn("No panel visible! activating last one...");
            if(lastPanel != null)
                lastPanel.Activate();
        }
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if(MainMenuPanel.gameObject.activeSelf
                && (MainMenuPanel.Animation == null || !MainMenuPanel.Animation.enabled))
            {
                this.LogInfo("APPLICATION QUIT");
                Application.Quit();
            }
        }
    }
}
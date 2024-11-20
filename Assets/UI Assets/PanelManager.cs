using Unity.Burst;
using UnityEngine;

public class PanelManager
{
    private GameObject cameraPanel;
    private GameObject basketPanel;
    private GameObject journalPanel;

    public PanelManager(GameObject cameraPanel, GameObject basketPanel, GameObject journalPanel)
    {
        this.cameraPanel = cameraPanel;
        this.basketPanel = basketPanel;
        this.journalPanel = journalPanel;
    }

    public void ToggleBasketPanel()
    {
        basketPanel.SetActive(!basketPanel.activeSelf);
    }

    public void ToggleJournalPanel()
    {
        bool isJournalActive = journalPanel.activeSelf;
        journalPanel.SetActive(!isJournalActive);
        cameraPanel.SetActive(isJournalActive);
        Debug.Log("Journal Button Pressed!");
    }

    public void ToggleCameraIcon()
    {
        Transform icon1 = cameraPanel.transform.GetChild(0);
        Transform icon2 = cameraPanel.transform.GetChild(1);

        bool isIcon1Active = icon1.gameObject.activeSelf;
        icon1.gameObject.SetActive(!isIcon1Active);
        icon2.gameObject.SetActive(isIcon1Active);
    }
        
}

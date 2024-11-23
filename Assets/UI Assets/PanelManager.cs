using Unity.Burst;
using UnityEngine;

public class PanelManager
{
    private GameObject cameraPanel, cameraLabel;
    private GameObject basketPanel;
    private GameObject journalPanel, journalLabel;

    public PanelManager(GameObject basketPanel, GameObject journalPanel, GameObject cameraLabel, GameObject journalLabel)
    {
        this.basketPanel = basketPanel;
        this.journalPanel = journalPanel;
        this.cameraLabel = cameraLabel;
        this.journalLabel = journalLabel;
    }

    public void ToggleBasketPanel()
    {   
        bool isBasketActive = basketPanel.activeSelf; // Correct this
        basketPanel.SetActive(!isBasketActive);
        journalPanel.SetActive(false);
        journalLabel.SetActive(false);
        cameraLabel.SetActive(true);
    }

    public void ToggleJournalPanel()
    {
        bool isJournalActive = journalPanel.activeSelf;
        journalPanel.SetActive(!isJournalActive);
        journalLabel.SetActive(!isJournalActive);
        basketPanel.SetActive(isJournalActive);
        cameraLabel.SetActive(isJournalActive);
    }

    public void ShowCameraPanel()
    {
        journalPanel.SetActive(false);
        journalLabel.SetActive(false);
        basketPanel.SetActive(false);
        cameraLabel.SetActive(true);
    }
        
}

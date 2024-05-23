using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueScreen : MonoBehaviour
{
    [SerializeField] private GameObject cataloguePanel;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject catalogueItemPrefab;

    public void CloseScreen()
    {
        cataloguePanel.SetActive(false);
    }

    public void UpdateCatalogueUI()
    {
        ClearCurrentUI();
        List<Scannable> list = Catalogue.ScannedItems;
        AddItemsFromCatalogue(list);
    }

    private void ClearCurrentUI()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void AddItemsFromCatalogue(List<Scannable> catalogue)
    {
        foreach(Scannable item in catalogue)
        {
            AddItem(item);
        }
    }

    private void AddItem(Scannable item)
    {
        // Spawn in one of our item panels
        GameObject panel = Instantiate(catalogueItemPrefab, content);

        // Get the text component from the panel
        Text[] panelText = panel.GetComponentsInChildren<Text>();

        // Update each component to show name/description
        panelText[0].text = item.ScanName;
        panelText[1].text = item.ScanDescription;
    }
}

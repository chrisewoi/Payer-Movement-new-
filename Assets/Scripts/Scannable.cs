using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scannable : MonoBehaviour
{
    
    public enum Category
    {
        Enviroment,
        Enemy,
        Item
    }

    [SerializeField] private Category category;
    [SerializeField] private string scanName;
    [SerializeField] private string scanDescription;

    private static Scanpopup scanPopup;

    private void Start()
    {
        if (scanPopup == null)
        {
            scanPopup = FindObjectOfType<Scanpopup>();
        }
    }

    public void Scan()
    {
        FindObjectOfType<Scanpopup>().DisplayScan(scanName, scanDescription);
    }
}

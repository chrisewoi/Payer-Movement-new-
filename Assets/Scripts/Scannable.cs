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

    [SerializeField] private Category scanCategory;
    [SerializeField] private string scanName;
    [SerializeField] private string scanDescription;

    public string ScanName
    {
        get
        {
            return scanName;
        }
    }

    public string ScanDescription
    {
        get
        {
            return scanDescription;
        }
    }

    public Category ScanCategory
    {
        get
        {
            return scanCategory;
        }
    }

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
        scanPopup.DisplayScan(scanName, scanDescription);
        Catalogue.CheckNewScan(this);
    }
}

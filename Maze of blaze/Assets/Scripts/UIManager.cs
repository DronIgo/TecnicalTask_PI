using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// A class which manages the game's UI
/// </summary>
public class UIManager : MonoBehaviour
{

    [Header("Page Management")]
    [Tooltip("The pages managed by the UI Manager")]
    public List<UIPage> pages;
    [Tooltip("The index of the active page in the UI")]
    public int currentPage = 0;
    [Tooltip("The page (by index) switched to when the UI Manager starts up")]
    public int defaultPage = 0;
    [Tooltip("The index of the game over page in the pages list")]
    public int gameOverPage = 0;
    [Tooltip("The index of the pause page in the pages list")]
    public int pausePage = 1;

    // A list of all UI element classes
    private List<UIelement> UIelements;

    // The event system handling UI navigation
    [HideInInspector]
    public EventSystem eventSystem;


    /// <summary>
    /// Finds and stores all UIElements in the UIElements list
    /// </summary>
    private void SetUpUIElements()
    {
        UIelements = FindObjectsOfType<UIelement>().ToList();
    }

    /// <summary>
    /// Gets the event system from the scene if one exists
    /// If one does not exist a warning will be displayed
    /// </summary>
    private void SetUpEventSystem()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("There is no event system in the scene but you are trying to use the UIManager. /n" +
                "All UI in Unity requires an Event System to run. /n" + 
                "You can add one by right clicking in hierarchy then selecting UI->EventSystem.");
        }
    }

    /// <summary>
    /// Goes through all UI elements and calls their UpdateUI function
    /// </summary>
    public void UpdateUI()
    {
        foreach(UIelement uiElement in UIelements)
        {
            uiElement.UpdateUI();
        }
    }

    private void Start()
    {
        SetUpEventSystem();
        SetUpUIElements();
        InitilizeFirstPage();
        UpdateUI();
    }

    /// <summary>
    /// Sets up the first page
    /// </summary>
    private void InitilizeFirstPage()
    {
        GoToPage(defaultPage);
    }

    /// <summary>
    /// Goes to a page by that page's index
    /// </summary>
    /// <param name="pageIndex">The index in the page list to go to</param>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex < pages.Count && pages[pageIndex] != null)
        {
            SetActiveAllPages(false);
            pages[pageIndex].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Goes to a page by that page's name
    /// </summary>
    /// <param name="pageName">The name of the page in the game you want to go to, if their are duplicates this picks the first found</param>
    public void GoToPageByName(string pageName)
    {
        UIPage page = pages.Find(item => item.name == pageName);
        int pageIndex = pages.IndexOf(page);
        GoToPage(pageIndex);
    }

    /// <summary>
    /// Turns all stored pages on or off depending on parameters
    /// </summary>
    /// <param name="activated">The true or false value to set all page game objects activeness to</param>
    public void SetActiveAllPages(bool activated)
    {
        if (pages != null)
        {
            foreach (UIPage page in pages)
            {
                if (page != null)
                    page.gameObject.SetActive(activated);
            }
        }
    }
}

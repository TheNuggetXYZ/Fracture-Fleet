using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [field: SerializeField] public WorldMenu worldMenu {get; private set;}
    [SerializeField] private GameObject parentItem;

    public void Show(bool show)
    {
        gameObject.SetActive(show);
        
        if (parentItem)
            parentItem.SetActive(!show);
        
        if (parentItem && parentItem.activeInHierarchy)
            worldMenu.SetCurrentMenuPanel(parentItem);
        else if (gameObject.activeInHierarchy)
            worldMenu.SetCurrentMenuPanel(gameObject);
        else
            worldMenu.SetCurrentMenuPanel(null);
    }

    public void ExitGame()
    {
        worldMenu.ExitGame();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIHandler : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas canvas;
    [Header("Generic Objects")]
    public GameObject background;
    public Material backgroundGradient;

    [Header("Select Level Menu")]
    public GameObject levelsViewport;
    public GameObject levelsGameObject;
    public GameObject beforeLevelsGameObject;
    public GameObject AfterLevelsGameObject;
    public GameObject worldNameObject;
    public int amtOfLevels = 25;
    public int comptdLevels = 12;
    public int levelsPerRow = 5;
    public int actualWorld = 1;
    [Header("Prefabs Level Frames")]
    public GameObject completedLevel;
    public GameObject availableLevel;
    public GameObject UnavailableLevel;

    [Header("World Arrows")]
    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject goBackArrow;

    [Header("Worlds Information")]
    public string[] worldname = {
        "Océano", //Azul claro
        "Bosque", //Verde
        "Desierto", //Amarillo
        "Sabana", //Naranja
        "Volcán", //Rojo
        "Cosmos", // Azul Oscuro
        "Agujero Negro" // Violeta
    };

    public Color[] upWorldColor = {
        new Color(0.149f, 0.45f, 0.623f, 1f), //Azul claro
        new Color(0.176f, 0.584f, 0.215f, 1f), //Verde
        new Color(0.631f, 0.482f, 0.164f, 1f), //Amarillo
        new Color(0.631f, 0.329f, 0.164f, 1f), //Naranja
        new Color(0.537f, 0.172f, 0.223f, 1f), //Rojo
        new Color(0.203f, 0.09f, 0.313f, 1f), //Azul oscuro
        new Color(0.215f, 0.098f, 0.333f, 1f) //Violeta
    };

    public Color[] downWorldColor = {
        new Color(0.168f, 0.674f, 0.819f, 1f), //Azul claro
        new Color(0.313f, 0.909f, 0.427f, 1f), //Verde
        new Color(0.859f, 0.909f, 0.313f, 1f), //Amarillo
        new Color(0.949f, 0.729f, 0.254f, 1f), //Naranja
        new Color(0.96f, 0.356f, 0.219f, 1f), //Rojo
        new Color(0.309f, 0.403f, 0.745f, 1f), //Azul oscuro
        new Color(0.780f, 0.313f, 0.674f, 1f) //Violeta
    };

    public Vector2[] arrowWorldPositions = {
        new Vector2(0.28f, 0.72f), //Océano
        new Vector2(0.28f, 0.72f), //Bosque
        new Vector2(0.26f, 0.74f), //Desierto
        new Vector2(0.28f, 0.72f), //Sabana
        new Vector2(0.28f, 0.72f), //Volcán
        new Vector2(0.28f, 0.72f), //Cosmos
        new Vector2(0.14f, 0.86f) //Agujero Negro
    };



    private Vector2 baseScreenSize = new Vector2(1080f, 1920f);
    private int totalWorlds = 7;

    private bool isActuallyMoving = true;
    
    public enum Directions{
        LEFT = -1,
        RIGHT = 1
    };

    void Start()
    {
        startWorldsMatrix();
        isActuallyMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator instantiateWorldLevelMenu(GameObject goToModify, int worldToLoad , int amountOfLevels, int completedLevels){

        int rowAmount = 0;
        int addedBoxes = 0;
        float boxSize, boxVertical, marginBetweenFrames = 0.02f;
        Color avgColor;

        if(worldToLoad >= 1 && worldToLoad < totalWorlds){
            avgColor = averageColorGradient(downWorldColor[worldToLoad - 1], upWorldColor[worldToLoad - 1]);
            Debug.Log("Average de " + worldToLoad + " (if)");
        } else if (worldToLoad < 1){
            avgColor = averageColorGradient(downWorldColor[0], upWorldColor[0]);
            Debug.Log("Average de " + 0 + " en worldToLoad " + worldToLoad + " (else if)");
        } else {
            avgColor = averageColorGradient(downWorldColor[totalWorlds - 1], upWorldColor[totalWorlds - 1]);
            Debug.Log("Average de " + (totalWorlds-1).ToString() + " en worldToLoad " + worldToLoad + " (else)");
        }
        
        yield return new WaitForEndOfFrame();

        float inheritedWidth = levelsViewport.transform.GetComponent<RectTransform>().rect.width;
        float inheritedHeight = levelsViewport.transform.GetComponent<RectTransform>().rect.height;



        //First, remove all the elements of the levels game object
        removeAllChildren(goToModify);

        //Then, add the needed ones
        //First calculate the amount of rows and sizes of the box
        rowAmount = Mathf.FloorToInt(amountOfLevels/levelsPerRow);
        if(amountOfLevels%levelsPerRow > 0){
            rowAmount++;
        }

        boxSize = (1f - ((float)levelsPerRow-1f)*(float)marginBetweenFrames)/(float)levelsPerRow;
        boxVertical = (1f - ((float)5-1f)*(float)marginBetweenFrames)/5f;
        goToModify.GetComponent<RectTransform>().sizeDelta = new Vector2(inheritedWidth, rowAmount*(((inheritedWidth - (inheritedWidth*((float)levelsPerRow-1f)*marginBetweenFrames))/levelsPerRow)+(marginBetweenFrames*inheritedWidth)) + 5);

        float cellWidth = inheritedWidth/levelsPerRow;

        goToModify.GetComponent<GridLayoutGroup>().cellSize = new Vector2((inheritedWidth - (inheritedWidth*((float)levelsPerRow-1f)*marginBetweenFrames))/levelsPerRow, (inheritedWidth - (inheritedWidth*((float)levelsPerRow-1f)*marginBetweenFrames))/levelsPerRow);
        goToModify.GetComponent<GridLayoutGroup>().spacing = new Vector2(inheritedWidth*marginBetweenFrames, inheritedWidth*marginBetweenFrames);

        //Now, create a base Prefab

        //Finally add them to levels GO
        for(int y = 0; y < rowAmount; y++){
            for(int x = 0; x < levelsPerRow; x++){
                if(addedBoxes < completedLevels){
                    GameObject completedPrefab = completedLevel;
                    completedPrefab.name = (addedBoxes+1).ToString();
                    completedPrefab.GetComponent<RectTransform>().anchorMin = new Vector2(boxSize*x + marginBetweenFrames*x , (1f-(inheritedWidth/inheritedHeight)) + (inheritedWidth/inheritedHeight)*(1 - boxVertical - (boxVertical+marginBetweenFrames)*y));
                    completedPrefab.GetComponent<RectTransform>().anchorMax = new Vector2(boxSize + (boxSize+marginBetweenFrames)*x , (1f-(inheritedWidth/inheritedHeight)) + (inheritedWidth/inheritedHeight)*(1 - boxVertical*y - marginBetweenFrames*y));
                    completedPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                    completedPrefab.transform.Find("Value").GetComponent<TextMeshProUGUI>().color = avgColor;
                    completedPrefab.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = (addedBoxes+1).ToString();
                    if(addedBoxes < amountOfLevels){
                        Instantiate(completedPrefab, goToModify.transform);
                    }
                } else if(addedBoxes == completedLevels){            
                    GameObject availablePrefab = availableLevel;
                    availablePrefab.name = (addedBoxes+1).ToString();
                    availablePrefab.GetComponent<RectTransform>().anchorMin = new Vector2(boxSize*x + marginBetweenFrames*x , (1f-(inheritedWidth/inheritedHeight)) + (inheritedWidth/inheritedHeight)*(1 - boxVertical - (boxVertical+marginBetweenFrames)*y));
                    availablePrefab.GetComponent<RectTransform>().anchorMax = new Vector2(boxSize + (boxSize+marginBetweenFrames)*x , (1f-(inheritedWidth/inheritedHeight)) + (inheritedWidth/inheritedHeight)*(1 - boxVertical*y - marginBetweenFrames*y));
                    availablePrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                    availablePrefab.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = (addedBoxes+1).ToString();
                    if(addedBoxes < amountOfLevels){   
                        Instantiate(availablePrefab, goToModify.transform);
                    }
                } else {
                    GameObject unavailablePrefab = UnavailableLevel;
                    unavailablePrefab.name = (addedBoxes+1).ToString();
                    unavailablePrefab.GetComponent<RectTransform>().anchorMin = new Vector2(boxSize*x + marginBetweenFrames*x , (1f-(inheritedWidth/inheritedHeight)) + (inheritedWidth/inheritedHeight)*(1 - boxVertical - (boxVertical+marginBetweenFrames)*y));
                    unavailablePrefab.GetComponent<RectTransform>().anchorMax = new Vector2(boxSize + (boxSize+marginBetweenFrames)*x , (1f-(inheritedWidth/inheritedHeight)) + (inheritedWidth/inheritedHeight)*(1 - boxVertical*y - marginBetweenFrames*y));
                    unavailablePrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                    unavailablePrefab.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = (addedBoxes+1).ToString();
                    if(addedBoxes < amountOfLevels){
                        Instantiate(unavailablePrefab, goToModify.transform);
                    }
                }
                addedBoxes++;
            }
        }

    }

    public void moveWorldRight(){
        if(!isActuallyMoving){
            isActuallyMoving = true;
            StartCoroutine(changeWorld(Directions.RIGHT));
        }
    }

    public void moveWorldLeft(){
        if(!isActuallyMoving){
            isActuallyMoving = true;
            StartCoroutine(changeWorld(Directions.LEFT));
        }
    }

    void StartAnimations(){
        StartLeftArrowAnimation();
        StartRightArrowAnimation();
    }

    void StartLeftArrowAnimation(){
        RectTransform leftRect = leftButton.GetComponent<RectTransform>();
        leftButton.GetComponent<RectTransform>().DOMoveX(leftRect.position.x - 0.015f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetId("leftArrowAnimation");
    }

    void StartRightArrowAnimation(){
        RectTransform rightRect = rightButton.GetComponent<RectTransform>();
        rightButton.GetComponent<RectTransform>().DOMoveX(rightRect.position.x + 0.015f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetId("rightArrowAnimation");
    }

    void StartGoBackAnimation(){
        RectTransform backRect = goBackArrow.GetComponent<RectTransform>();
        goBackArrow.GetComponent<RectTransform>().DOMoveX(backRect.position.x - 0.015f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
    }

    public IEnumerator changeWorld(Directions direction){
        if(direction == Directions.LEFT){
            if(actualWorld > 1){
                actualWorld--;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.5f);
                yield return new WaitForSeconds(0.51f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld < 1){
                actualWorld = 1;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.5f);
                yield return new WaitForSeconds(0.51f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld > totalWorlds){
                actualWorld = totalWorlds;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(Directions.RIGHT, 0.5f);
                yield return new WaitForSeconds(0.51f);
                StartCoroutine(resetWorldsMatrix());
            }
        } else if(direction == Directions.RIGHT){
            if(actualWorld < totalWorlds){
                actualWorld++;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.5f);
                yield return new WaitForSeconds(0.51f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld < 1){
                actualWorld = 1;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.5f);
                yield return new WaitForSeconds(0.51f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld > totalWorlds){
                actualWorld = totalWorlds;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(Directions.LEFT, 0.5f);
                yield return new WaitForSeconds(0.51f);
                StartCoroutine(resetWorldsMatrix());
            }
        }

        yield return new WaitForSeconds(0.05f);

        isActuallyMoving = false;
    }

    void activateArrows(Directions arrowSide){
        if(arrowSide == Directions.LEFT){
            Color leftArrowImg = leftArrow.GetComponent<Image>().color;
            leftArrow.GetComponent<Image>().color = new Color(leftArrowImg.r, leftArrowImg.g, leftArrowImg.b, 1);
            leftButton.GetComponent<Button>().interactable = true;
        } else if(arrowSide == Directions.RIGHT){
            Color rightArrowImg = rightArrow.GetComponent<Image>().color;
            rightButton.GetComponent<Button>().interactable = true;
            rightArrow.GetComponent<Image>().color = new Color(rightArrowImg.r, rightArrowImg.g, rightArrowImg.b, 1);
        }
    }

    void deactivateArrows(Directions arrowSide){
        if(arrowSide == Directions.LEFT){
            Color leftArrowImg = leftArrow.GetComponent<Image>().color;
            leftArrow.GetComponent<Image>().color = new Color(leftArrowImg.r, leftArrowImg.g, leftArrowImg.b, 0);
            leftButton.GetComponent<Button>().interactable = false;
        } else if(arrowSide == Directions.RIGHT){
            Color rightArrowImg = rightArrow.GetComponent<Image>().color;
            rightArrow.GetComponent<Image>().color = new Color(rightArrowImg.r, rightArrowImg.g, rightArrowImg.b, 0);
            rightButton.GetComponent<Button>().interactable = false;
        }
    }

    void removeAllChildren(GameObject go){
        foreach (Transform child in go.transform)
         {
             Destroy(child.gameObject);
         }
    }

    void setArrowsAvailability(){
        if(actualWorld <= 1){
            deactivateArrows(Directions.LEFT);
            activateArrows(Directions.RIGHT);
        } else if(actualWorld >= totalWorlds){
            deactivateArrows(Directions.RIGHT);
            activateArrows(Directions.LEFT);
        } else{
            activateArrows(Directions.LEFT);
            activateArrows(Directions.RIGHT);
        }
    }

    void displaceWorldMatrix(Directions dir, float duration){
        if(dir == Directions.LEFT){
            DOTween.To(()=> levelsGameObject.GetComponent<RectTransform>().anchorMin, x=>levelsGameObject.GetComponent<RectTransform>().anchorMin = x, new Vector2(1.7f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> beforeLevelsGameObject.GetComponent<RectTransform>().anchorMin, x=>beforeLevelsGameObject.GetComponent<RectTransform>().anchorMin = x, new Vector2(0.5f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> AfterLevelsGameObject.GetComponent<RectTransform>().anchorMin, x=>AfterLevelsGameObject.GetComponent<RectTransform>().anchorMin = x, new Vector2(2.9f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> levelsGameObject.GetComponent<RectTransform>().anchorMax, x=>levelsGameObject.GetComponent<RectTransform>().anchorMax = x, new Vector2(1.7f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> beforeLevelsGameObject.GetComponent<RectTransform>().anchorMax, x=>beforeLevelsGameObject.GetComponent<RectTransform>().anchorMax = x, new Vector2(0.5f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> AfterLevelsGameObject.GetComponent<RectTransform>().anchorMax, x=>AfterLevelsGameObject.GetComponent<RectTransform>().anchorMax = x, new Vector2(2.9f, 1f), duration).SetEase(Ease.InOutQuad);
        } else if(dir == Directions.RIGHT){
            DOTween.To(()=> levelsGameObject.GetComponent<RectTransform>().anchorMin, x=>levelsGameObject.GetComponent<RectTransform>().anchorMin = x, new Vector2(-0.7f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> beforeLevelsGameObject.GetComponent<RectTransform>().anchorMin, x=>beforeLevelsGameObject.GetComponent<RectTransform>().anchorMin = x, new Vector2(-1.9f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> AfterLevelsGameObject.GetComponent<RectTransform>().anchorMin, x=>AfterLevelsGameObject.GetComponent<RectTransform>().anchorMin = x, new Vector2(0.5f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> levelsGameObject.GetComponent<RectTransform>().anchorMax, x=>levelsGameObject.GetComponent<RectTransform>().anchorMax = x, new Vector2(-0.7f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> beforeLevelsGameObject.GetComponent<RectTransform>().anchorMax, x=>beforeLevelsGameObject.GetComponent<RectTransform>().anchorMax = x, new Vector2(-1.9f, 1f), duration).SetEase(Ease.InOutQuad);
            DOTween.To(()=> AfterLevelsGameObject.GetComponent<RectTransform>().anchorMax, x=>AfterLevelsGameObject.GetComponent<RectTransform>().anchorMax = x, new Vector2(0.5f, 1f), duration).SetEase(Ease.InOutQuad);
        }
    }

    IEnumerator resetWorldsMatrix(){
        //removeAllChildren(levelsGameObject);
        StartCoroutine(instantiateWorldLevelMenu(levelsGameObject, actualWorld, amtOfLevels, comptdLevels));
        yield return new WaitForSeconds(0.01f);
        levelsGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
        levelsGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
        yield return new WaitForSeconds(0.01f);
        beforeLevelsGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(-0.7f, 1f);
        beforeLevelsGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(-0.7f, 1f);
        AfterLevelsGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1.7f, 1f);
        AfterLevelsGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1.7f, 1f);
        StartCoroutine(instantiateWorldLevelMenu(beforeLevelsGameObject, actualWorld - 1, amtOfLevels, comptdLevels));
        StartCoroutine(instantiateWorldLevelMenu(AfterLevelsGameObject, actualWorld + 1, amtOfLevels, comptdLevels));
    }

    IEnumerator fadeInformation(){

        //Fade bg color
        DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), downWorldColor[actualWorld - 1], 0.3f).SetEase(Ease.OutSine);
        DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), upWorldColor[actualWorld - 1], 0.3f).SetEase(Ease.OutSine);
        
        //Set Arrows Positions  
        DOTween.Kill("leftArrowAnimation");
        DOTween.Kill("rightArrowAnimation");
        leftButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].x, leftButton.GetComponent<RectTransform>().anchorMin.y);
        leftButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].x, leftButton.GetComponent<RectTransform>().anchorMax.y);
        rightButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].y, rightButton.GetComponent<RectTransform>().anchorMin.y);
        rightButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].y, rightButton.GetComponent<RectTransform>().anchorMax.y);
        leftButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0); 
        rightButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);
        StartAnimations();

        //Set World Name
        Color worldNameColor = worldNameObject.GetComponent<TextMeshProUGUI>().color;
        DOTween.To(()=> worldNameObject.GetComponent<TextMeshProUGUI>().color, x=>worldNameObject.GetComponent<TextMeshProUGUI>().color = x, new Color(worldNameColor.r, worldNameColor.g, worldNameColor.b, 0f), 0.08f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.08f);
        worldNameObject.GetComponent<TextMeshProUGUI>().text = worldname[actualWorld - 1];
        DOTween.To(()=> worldNameObject.GetComponent<TextMeshProUGUI>().color, x=>worldNameObject.GetComponent<TextMeshProUGUI>().color = x, new Color(worldNameColor.r, worldNameColor.g, worldNameColor.b, 1f), 0.08f).SetEase(Ease.OutQuad);


    }

    void startWorldsMatrix(){
        backgroundGradient.SetColor("_Color", downWorldColor[actualWorld - 1]);
        backgroundGradient.SetColor("_Color2", upWorldColor[actualWorld - 1]);
        //background.GetComponent<Image>().color = worldcolor[actualWorld - 1];
        worldNameObject.GetComponent<TextMeshProUGUI>().text = worldname[actualWorld - 1];
        leftButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].x, leftButton.GetComponent<RectTransform>().anchorMin.y);
        leftButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].x, leftButton.GetComponent<RectTransform>().anchorMax.y);
        rightButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].y, rightButton.GetComponent<RectTransform>().anchorMin.y);
        rightButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].y, rightButton.GetComponent<RectTransform>().anchorMax.y);

        setArrowsAvailability();
        StartCoroutine(instantiateWorldLevelMenu(levelsGameObject, actualWorld, amtOfLevels, comptdLevels));
        StartCoroutine(instantiateWorldLevelMenu(beforeLevelsGameObject, actualWorld - 1, amtOfLevels, comptdLevels));
        StartCoroutine(instantiateWorldLevelMenu(AfterLevelsGameObject, actualWorld + 1, amtOfLevels, comptdLevels));
        StartAnimations();
        StartGoBackAnimation();
    }

    Color averageColorGradient(Color color1, Color color2){
        return new Color((color1.r+color2.r)/2f, (color1.g+color2.g)/2f, (color1.b+color2.b)/2f, 1);
    }

}

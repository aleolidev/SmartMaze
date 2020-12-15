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
    public GameObject settingsGroup;
    public GameObject levelsGroup;
    public GameObject levelsGUIGO;
    public GameObject gameGroup;
    public GameObject gameInterfaceGroup;
    public MazeSystem mazeSys;
    public GameObject darkmodeGO;
    public GameObject darkmodeFill;
    public GameObject darkmodeHandle;
    
    [Header("Status")]
    public SceneStatus sceneStat = SceneStatus.InGame;
    public bool darkmode = false;
    public float changeSceneDuration = 0.4f;
    [Header("Select Level Menu")]
    public GameObject levelsViewport;
    public GameObject levelsGameObject;
    public GameObject currentLevelGO;
    public GameObject beforeLevelsGameObject;
    public GameObject AfterLevelsGameObject;
    public GameObject scrollbar;
    public GameObject scrollList;
    public GameObject worldNameObject;
    public int amtOfLevels = 25;
    public int comptdLevels = 12;
    public int levelsPerRow = 5;
    public int actualWorld = 1; //Used for menu displacements
    public int currentLevelWorld = 1; //Used for general purposes outside the levels menu
    [Header("Prefabs Level Frames")]
    public GameObject completedLevel;
    public GameObject availableLevel;
    public GameObject UnavailableLevel;

    [Header("World Arrows")]
    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject moreLevelsIndicator;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject downArrow;
    public GameObject goBackArrow;

    bool showMoreLevels = false;

    private int worldInstantiations = 0;

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

    public Color darkmodeBGColor = new Color(0.165f, 0.165f, 0.165f, 1f);

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
    private bool isScrollbarNeeded = false;
    
    public enum Directions{
        LEFT = -1,
        RIGHT = 1
    };

    public enum SceneStatus{
        Disabled = 0,
        InGame = 1,
        WorldLevelSelector = 2,
        Settings = 3
    }

    void Start()
    {
        darkmode = PlayerPrefs.GetInt("darkmode") > 0;
        startWorldsMatrix();
        isActuallyMoving = false;

        isScrollbarNeeded = isScrollingActive();
        if(isScrollbarNeeded){
            scrollList.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Elastic;
        } else {
            scrollList.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
        }

        if(!darkmode){
            darkmodeFill.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
            darkmodeFill.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
            darkmodeHandle.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
            darkmodeHandle.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0.5f);
        } else {
            darkmodeFill.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            darkmodeFill.GetComponent<RectTransform>().offsetMax = new Vector2(-5f, 0f);
            darkmodeHandle.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
            darkmodeHandle.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        setScrollbarType();

        //Mostrar/Ocultar flecha indicadora de más niveles
        if(sceneStat == SceneStatus.WorldLevelSelector){
            if(scrollList.GetComponent<ScrollRect>().verticalNormalizedPosition > 0.9f && !showMoreLevels){
                showMoreLevels = true;
                DOTween.To(()=> downArrow.GetComponent<Image>().color, x=>downArrow.GetComponent<Image>().color = x, new Color(1f, 1f, 1f, 1f), 0.35f).SetEase(Ease.InOutQuad);
            } else if(scrollList.GetComponent<ScrollRect>().verticalNormalizedPosition <= 0.9f && showMoreLevels) {
                showMoreLevels = false;
                DOTween.To(()=> downArrow.GetComponent<Image>().color, x=>downArrow.GetComponent<Image>().color = x, new Color(1f, 1f, 1f, 0f), 0.35f).SetEase(Ease.InOutQuad);
            }
        }
    }

    IEnumerator instantiateWorldLevelMenu(GameObject goToModify, int worldToLoad , int amountOfLevels, int completedLevels){

        int rowAmount = 0;
        int addedBoxes = 0;
        float boxSize, boxVertical, marginBetweenFrames = 0.02f;
        Color avgColor;

        if(worldToLoad >= 1 && worldToLoad < totalWorlds){
            avgColor = averageColorGradient(downWorldColor[worldToLoad - 1], upWorldColor[worldToLoad - 1]);
        } else if (worldToLoad < 1){
            avgColor = averageColorGradient(downWorldColor[0], upWorldColor[0]);
        } else {
            avgColor = averageColorGradient(downWorldColor[totalWorlds - 1], upWorldColor[totalWorlds - 1]);
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

        if(worldInstantiations < 3){
            worldInstantiations++;
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
        StartMoreLevelsIndicatorAnimation();
    }

    void StartLeftArrowAnimation(){
        RectTransform leftRect = leftButton.GetComponent<RectTransform>();
        leftButton.GetComponent<RectTransform>().DOMoveX(leftRect.position.x - 0.015f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetId("leftArrowAnimation");
    }

    void StartRightArrowAnimation(){
        RectTransform rightRect = rightButton.GetComponent<RectTransform>();
        rightButton.GetComponent<RectTransform>().DOMoveX(rightRect.position.x + 0.015f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetId("rightArrowAnimation");
    }

    void StartMoreLevelsIndicatorAnimation(){
        RectTransform downRect = moreLevelsIndicator.GetComponent<RectTransform>();
        moreLevelsIndicator.GetComponent<RectTransform>().DOMoveY(downRect.position.y - 0.03f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetId("moreLevelsIndicatorAnimation");
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
                displaceWorldMatrix(direction, 0.33f);
                yield return new WaitForSecondsRealtime(0.34f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld < 1){
                actualWorld = 1;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.33f);
                yield return new WaitForSecondsRealtime(0.34f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld > totalWorlds){
                actualWorld = totalWorlds;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(Directions.RIGHT, 0.33f);
                yield return new WaitForSecondsRealtime(0.34f);
                StartCoroutine(resetWorldsMatrix());
            }
        } else if(direction == Directions.RIGHT){
            if(actualWorld < totalWorlds){
                actualWorld++;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.33f);
                yield return new WaitForSecondsRealtime(0.34f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld < 1){
                actualWorld = 1;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(direction, 0.33f);
                yield return new WaitForSecondsRealtime(0.34f);
                StartCoroutine(resetWorldsMatrix());
            } else if (actualWorld > totalWorlds){
                actualWorld = totalWorlds;
                setArrowsAvailability();
                StartCoroutine(fadeInformation());
                displaceWorldMatrix(Directions.LEFT, 0.33f);
                yield return new WaitForSecondsRealtime(0.34f);
                StartCoroutine(resetWorldsMatrix());
            }
        }

        yield return new WaitForSecondsRealtime(0.05f);

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
        levelsGameObject.transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
        StartCoroutine(instantiateWorldLevelMenu(levelsGameObject, actualWorld, amtOfLevels, comptdLevels));
        yield return new WaitForSecondsRealtime(0.01f);
        levelsGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
        levelsGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
        yield return new WaitForSecondsRealtime(0.01f);
        beforeLevelsGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(-0.7f, 1f);
        beforeLevelsGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(-0.7f, 1f);
        AfterLevelsGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1.7f, 1f);
        AfterLevelsGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1.7f, 1f);
        yield return new WaitForSecondsRealtime(0.05f);
        StartCoroutine(instantiateWorldLevelMenu(beforeLevelsGameObject, actualWorld - 1, amtOfLevels, comptdLevels));
        StartCoroutine(instantiateWorldLevelMenu(AfterLevelsGameObject, actualWorld + 1, amtOfLevels, comptdLevels));
    }

    IEnumerator fadeInformation(){

        //Fade bg color
        DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), downWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
        DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), upWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
        
        //Set Arrows Positions  
        DOTween.Kill("leftArrowAnimation");
        DOTween.Kill("rightArrowAnimation");
        DOTween.Kill("moreLevelsIndicatorAnimation");
        leftButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].x, leftButton.GetComponent<RectTransform>().anchorMin.y);
        leftButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].x, leftButton.GetComponent<RectTransform>().anchorMax.y);
        rightButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].y, rightButton.GetComponent<RectTransform>().anchorMin.y);
        rightButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].y, rightButton.GetComponent<RectTransform>().anchorMax.y);
        leftButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0); 
        rightButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);
        StartAnimations();

        //Set World Name
        Color worldNameColor = worldNameObject.GetComponent<TextMeshProUGUI>().color;
        DOTween.To(()=> worldNameObject.GetComponent<TextMeshProUGUI>().color, x=>worldNameObject.GetComponent<TextMeshProUGUI>().color = x, new Color(worldNameColor.r, worldNameColor.g, worldNameColor.b, 0f), 0.056f).SetEase(Ease.OutQuad);
        yield return new WaitForSecondsRealtime(0.056f);
        worldNameObject.GetComponent<TextMeshProUGUI>().text = worldname[actualWorld - 1];
        DOTween.To(()=> worldNameObject.GetComponent<TextMeshProUGUI>().color, x=>worldNameObject.GetComponent<TextMeshProUGUI>().color = x, new Color(worldNameColor.r, worldNameColor.g, worldNameColor.b, 1f), 0.056f).SetEase(Ease.OutQuad);


    }

    void startWorldsMatrix(){
        mazeSys.addShades = false;
        if(darkmode){
            //mazeSys.addShades = true;
            backgroundGradient.SetColor("_Color", darkmodeBGColor);
            backgroundGradient.SetColor("_Color2", darkmodeBGColor);
        } else {
            //mazeSys.addShades = false;
            backgroundGradient.SetColor("_Color", downWorldColor[actualWorld - 1]);
            backgroundGradient.SetColor("_Color2", upWorldColor[actualWorld - 1]);
        }
        

        //background.GetComponent<Image>().color = worldcolor[actualWorld - 1];
        worldNameObject.GetComponent<TextMeshProUGUI>().text = worldname[actualWorld - 1];

        setArrowsAvailability();
        StartCoroutine(instantiateWorldLevelMenu(levelsGameObject, actualWorld, amtOfLevels, comptdLevels));
        StartCoroutine(instantiateWorldLevelMenu(beforeLevelsGameObject, actualWorld - 1, amtOfLevels, comptdLevels));
        StartCoroutine(instantiateWorldLevelMenu(AfterLevelsGameObject, actualWorld + 1, amtOfLevels, comptdLevels));
        StartCoroutine(finishWorldSelectorInitialization());
    }

    Color averageColorGradient(Color color1, Color color2){
        return new Color((color1.r+color2.r)/2f, (color1.g+color2.g)/2f, (color1.b+color2.b)/2f, 1);
    }

    bool isScrollingActive(){
        if(scrollbar.activeSelf){
            return true;
        } else{
            return false;
        }
    }

    void setScrollbarType(){
        if(isScrollbarNeeded != isScrollingActive()){
            isScrollbarNeeded = isScrollingActive();
            if(isScrollbarNeeded){
                scrollList.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Elastic;
            } else {
                scrollList.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
            }
        }
    }

    public void returnToGame(){
        if(sceneStat == SceneStatus.Settings || sceneStat == SceneStatus.WorldLevelSelector){
            goBackArrow.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            goBackArrow.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            DOTween.To(()=> goBackArrow.GetComponent<RectTransform>().localScale, x=>goBackArrow.GetComponent<RectTransform>().localScale = x, new Vector3(0.95f, 0.95f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
            DOTween.To(()=> goBackArrow.GetComponent<Image>().color, x=>goBackArrow.GetComponent<Image>().color = x, new Color(1f, 1f, 1f, 0f), changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => {
                goBackArrow.SetActive(false);
            });

            switch(sceneStat){
                case SceneStatus.Settings:
                    settingsGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    settingsGroup.GetComponent<CanvasGroup>().alpha = 1f;
                    StartCoroutine(showGame());
                    DOTween.To(()=> settingsGroup.GetComponent<RectTransform>().localScale, x=>settingsGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
                    DOTween.To(()=> settingsGroup.GetComponent<CanvasGroup>().alpha, x=>settingsGroup.GetComponent<CanvasGroup>().alpha = x, 0f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => { 
                       settingsGroup.SetActive(false);
                    });
                break;
                case SceneStatus.WorldLevelSelector:
                    actualWorld = currentLevelWorld;

                    levelsGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    levelsGroup.GetComponent<CanvasGroup>().alpha = 1f;
                    StartCoroutine(showGame());

                    if(darkmode){
                        DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), darkmodeBGColor, 0.2f).SetEase(Ease.OutSine);
                        DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), darkmodeBGColor, 0.2f).SetEase(Ease.OutSine);
                    } else {
                        DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), downWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
                        DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), upWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
                    }

                    DOTween.To(()=> levelsGroup.GetComponent<RectTransform>().localScale, x=>levelsGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
                    DOTween.To(()=> levelsGroup.GetComponent<CanvasGroup>().alpha, x=>levelsGroup.GetComponent<CanvasGroup>().alpha = x, 0f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => { 
                        levelsGroup.SetActive(false);
                    });
                break;
            }
        } else if(sceneStat == SceneStatus.InGame) {
            levelsGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            levelsGroup.GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    public void openSettings(){
        if(sceneStat == SceneStatus.InGame){
            mazeSys.hideUnseenSlabs();

            gameGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            gameGroup.GetComponent<CanvasGroup>().alpha = 1f;
            //gameInterfaceGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            gameInterfaceGroup.GetComponent<CanvasGroup>().alpha = 1f;
            
            StartCoroutine(showSettings());

            DOTween.To(()=> gameGroup.GetComponent<RectTransform>().localScale, x=>gameGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
            DOTween.To(()=> gameGroup.GetComponent<CanvasGroup>().alpha, x=>gameGroup.GetComponent<CanvasGroup>().alpha = x, 0f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => { 
                gameGroup.SetActive(false);
            });
            //DOTween.To(()=> gameInterfaceGroup.GetComponent<RectTransform>().localScale, x=>gameInterfaceGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
            DOTween.To(()=> gameInterfaceGroup.GetComponent<CanvasGroup>().alpha, x=>gameInterfaceGroup.GetComponent<CanvasGroup>().alpha = x, 0f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => { 
                gameInterfaceGroup.SetActive(false);
            });
        }
    }

    public void openLevels(){
        if(sceneStat == SceneStatus.InGame){
            mazeSys.hideUnseenSlabs();

            currentLevelWorld = actualWorld;
            worldNameObject.GetComponent<TextMeshProUGUI>().text = worldname[actualWorld - 1];
            StartCoroutine(resetWorldsMatrix());

            gameGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            gameGroup.GetComponent<CanvasGroup>().alpha = 1f;
            //gameInterfaceGroup.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            gameInterfaceGroup.GetComponent<CanvasGroup>().alpha = 1f;
            
            StartCoroutine(showLevels());

            DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), downWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
            DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), upWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);

            DOTween.To(()=> gameGroup.GetComponent<RectTransform>().localScale, x=>gameGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
            DOTween.To(()=> gameGroup.GetComponent<CanvasGroup>().alpha, x=>gameGroup.GetComponent<CanvasGroup>().alpha = x, 0f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => { 
                gameGroup.SetActive(false);
            });
            //DOTween.To(()=> gameInterfaceGroup.GetComponent<RectTransform>().localScale, x=>gameInterfaceGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
            DOTween.To(()=> gameInterfaceGroup.GetComponent<CanvasGroup>().alpha, x=>gameInterfaceGroup.GetComponent<CanvasGroup>().alpha = x, 0f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() => { 
                gameInterfaceGroup.SetActive(false);
            });
        }
    }

    public void hideMazeCompleted(){
        mazeSys.hideUnseenSlabs();
        DOTween.To(()=> gameGroup.GetComponent<RectTransform>().localScale, x=>gameGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1.05f, 1.05f, 1f), 3f*changeSceneDuration/4f).SetEase(Ease.OutCubic);
        DOTween.To(()=> gameGroup.GetComponent<CanvasGroup>().alpha, x=>gameGroup.GetComponent<CanvasGroup>().alpha = x, 0f, 3f*changeSceneDuration/4f).SetEase(Ease.InCubic).OnComplete(() => {
            mazeSys.generateNewLevelOnComplete();
        });
    }

    public void showGameOnCreateNewLevel(){
        DOTween.To(()=> gameGroup.GetComponent<RectTransform>().localScale, x=>gameGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), 3f*changeSceneDuration/4f).SetEase(Ease.InCubic);
        DOTween.To(()=> gameGroup.GetComponent<CanvasGroup>().alpha, x=>gameGroup.GetComponent<CanvasGroup>().alpha = x, 1f, 3f*changeSceneDuration/4f).SetEase(Ease.OutCubic);
    }

    IEnumerator showGame(){

        yield return new WaitForSeconds(changeSceneDuration/2f);

        sceneStat = SceneStatus.InGame;

        gameGroup.GetComponent<RectTransform>().localScale = new Vector3(1.05f, 1.05f, 1f);
        gameGroup.GetComponent<CanvasGroup>().alpha = 0f;
        //gameInterfaceGroup.GetComponent<RectTransform>().localScale = new Vector3(1.05f, 1.05f, 1f);
        gameInterfaceGroup.GetComponent<CanvasGroup>().alpha = 0f;
        gameGroup.SetActive(true);
        mazeSys.showSeenSlabs();
        gameInterfaceGroup.SetActive(true);

        DOTween.To(()=> gameGroup.GetComponent<RectTransform>().localScale, x=>gameGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
        DOTween.To(()=> gameGroup.GetComponent<CanvasGroup>().alpha, x=>gameGroup.GetComponent<CanvasGroup>().alpha = x, 1f, changeSceneDuration).SetEase(Ease.OutCubic);
        //DOTween.To(()=> gameInterfaceGroup.GetComponent<RectTransform>().localScale, x=>gameInterfaceGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
        DOTween.To(()=> gameInterfaceGroup.GetComponent<CanvasGroup>().alpha, x=>gameInterfaceGroup.GetComponent<CanvasGroup>().alpha = x, 1f, changeSceneDuration).SetEase(Ease.OutCubic).OnComplete(() =>{
            mazeSys.showAllSlabs();
        });
    }

    IEnumerator showLevels(){

        yield return new WaitForSeconds(changeSceneDuration/2f);

        sceneStat = SceneStatus.WorldLevelSelector;
        
        levelsGroup.GetComponent<RectTransform>().localScale = new Vector3(1.05f, 1.05f, 1f);
        levelsGroup.GetComponent<CanvasGroup>().alpha = 0f;
        goBackArrow.GetComponent<RectTransform>().localScale = new Vector3(0.95f, 0.95f, 1f);
        goBackArrow.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        levelsGroup.SetActive(true);
        goBackArrow.SetActive(true);

        DOTween.To(()=> goBackArrow.GetComponent<RectTransform>().localScale, x=>goBackArrow.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
        DOTween.To(()=> goBackArrow.GetComponent<Image>().color, x=>goBackArrow.GetComponent<Image>().color = x, new Color(1f, 1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);

        DOTween.To(()=> levelsGroup.GetComponent<RectTransform>().localScale, x=>levelsGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
        DOTween.To(()=> levelsGroup.GetComponent<CanvasGroup>().alpha, x=>levelsGroup.GetComponent<CanvasGroup>().alpha = x, 1f, changeSceneDuration).SetEase(Ease.OutCubic);
    }

    IEnumerator showSettings(){

        yield return new WaitForSeconds(changeSceneDuration/2f);

        sceneStat = SceneStatus.Settings;
        
        settingsGroup.GetComponent<RectTransform>().localScale = new Vector3(1.05f, 1.05f, 1f);
        settingsGroup.GetComponent<CanvasGroup>().alpha = 0f;
        goBackArrow.GetComponent<RectTransform>().localScale = new Vector3(0.95f, 0.95f, 1f);
        goBackArrow.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        settingsGroup.SetActive(true);
        goBackArrow.SetActive(true);

        DOTween.To(()=> goBackArrow.GetComponent<RectTransform>().localScale, x=>goBackArrow.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
        DOTween.To(()=> goBackArrow.GetComponent<Image>().color, x=>goBackArrow.GetComponent<Image>().color = x, new Color(1f, 1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);

        DOTween.To(()=> settingsGroup.GetComponent<RectTransform>().localScale, x=>settingsGroup.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), changeSceneDuration).SetEase(Ease.OutCubic);
        DOTween.To(()=> settingsGroup.GetComponent<CanvasGroup>().alpha, x=>settingsGroup.GetComponent<CanvasGroup>().alpha = x, 1f, changeSceneDuration).SetEase(Ease.OutCubic);
    }

    public void switchDarkmode(){
        if(darkmode){
            darkmode = false;
            PlayerPrefs.SetInt("darkmode", 0);
            DOTween.To(()=> darkmodeFill.GetComponent<RectTransform>().anchorMax, x=>darkmodeFill.GetComponent<RectTransform>().anchorMax = x, new Vector2(0f, 1f), 0.2f).SetEase(Ease.OutCubic);
            DOTween.To(()=> darkmodeFill.GetComponent<RectTransform>().offsetMax, x=>darkmodeFill.GetComponent<RectTransform>().offsetMax = x, new Vector2(0f, 0f), 0.2f).SetEase(Ease.OutCubic);
            
            DOTween.To(()=> darkmodeHandle.GetComponent<RectTransform>().anchorMin, x=>darkmodeHandle.GetComponent<RectTransform>().anchorMin = x, new Vector2(0f, 0.5f), 0.2f).SetEase(Ease.OutCubic);
            DOTween.To(()=> darkmodeHandle.GetComponent<RectTransform>().anchorMax, x=>darkmodeHandle.GetComponent<RectTransform>().anchorMax = x, new Vector2(0f, 0.5f), 0.2f).SetEase(Ease.OutCubic);
            //DOTween.To(()=> darkmodeGO.GetComponent<RectTransform>().Find("Handle").GetComponent<RectTransform>().localPosition, x=>darkmodeGO.GetComponent<RectTransform>().Find("Handle").GetComponent<RectTransform>().localPosition = x, new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.OutCubic);
            
        } else {
            darkmode = true;

            PlayerPrefs.SetInt("darkmode", 1);
            DOTween.To(()=> darkmodeFill.GetComponent<RectTransform>().anchorMax, x=>darkmodeFill.GetComponent<RectTransform>().anchorMax = x, new Vector2(1f, 1f), 0.2f).SetEase(Ease.OutCubic);
            DOTween.To(()=> darkmodeFill.GetComponent<RectTransform>().offsetMax, x=>darkmodeFill.GetComponent<RectTransform>().offsetMax = x, new Vector2(-5f, 0f), 0.2f).SetEase(Ease.OutCubic);
            
            DOTween.To(()=> darkmodeHandle.GetComponent<RectTransform>().anchorMin, x=>darkmodeHandle.GetComponent<RectTransform>().anchorMin = x, new Vector2(1f, 0.5f), 0.2f).SetEase(Ease.OutCubic);
            DOTween.To(()=> darkmodeHandle.GetComponent<RectTransform>().anchorMax, x=>darkmodeHandle.GetComponent<RectTransform>().anchorMax = x, new Vector2(1f, 0.5f), 0.2f).SetEase(Ease.OutCubic);
            //DOTween.To(()=> darkmodeGO.GetComponent<RectTransform>().Find("Handle").GetComponent<RectTransform>().localPosition, x=>darkmodeGO.GetComponent<RectTransform>().Find("Handle").GetComponent<RectTransform>().localPosition = x, new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.OutCubic);
        }

        PlayerPrefs.Save();
        mazeSys.switchMazeColorMode();
    }

    public void switchBGState(){
        if(darkmode){
            DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), darkmodeBGColor, 0.2f).SetEase(Ease.OutSine);
            DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), darkmodeBGColor, 0.2f).SetEase(Ease.OutSine);
        } else {
            DOTween.To(()=> backgroundGradient.GetColor("_Color"), x=> backgroundGradient.SetColor("_Color", x), downWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
            DOTween.To(()=> backgroundGradient.GetColor("_Color2"), x=> backgroundGradient.SetColor("_Color2", x), upWorldColor[actualWorld - 1], 0.2f).SetEase(Ease.OutSine);
        }
    }

    public void setLevelName(){
        currentLevelGO.GetComponent<TextMeshProUGUI>().text = "Nivel " + PlayerPrefs.GetInt("completedLevels").ToString();
    }

    public void animateReloadButton(GameObject r){
        Quaternion rQ = r.GetComponent<RectTransform>().localRotation;
        r.GetComponent<RectTransform>().DORotate(new Vector3(0f, 0f, -360f), 0.35f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad);
    }

    public void animateButton(GameObject go){
        DOTween.To(()=> go.GetComponent<RectTransform>().localScale, x=>go.GetComponent<RectTransform>().localScale = x, new Vector3(0.9f, 0.9f, 1f), changeSceneDuration/1.5f).SetEase(Ease.OutCirc).OnComplete(() =>{
            go.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        });
    }

    private void resetScale(GameObject go){
        DOTween.To(()=> go.GetComponent<RectTransform>().localScale, x=>go.GetComponent<RectTransform>().localScale = x, new Vector3(1, 1, 1f), 0.05f).SetEase(Ease.OutExpo);
    }

    IEnumerator finishWorldSelectorInitialization(){
        while(worldInstantiations < 3){
            yield return null;
        } 

        RectTransform lvlVwprtRT = levelsViewport.GetComponent<RectTransform>();
        GameObject lvlBox = levelsViewport.transform.parent.parent.gameObject;
        float topMargin = -Mathf.Abs((levelsViewport.transform.parent.parent.gameObject.GetComponent<RectTransform>().rect.width - lvlVwprtRT.rect.width)/2f) - 4;
        float verticalViewportPixelSize = Mathf.Abs(topMargin) + levelsGameObject.GetComponent<GridLayoutGroup>().cellSize.y*6f + levelsGameObject.GetComponent<GridLayoutGroup>().spacing.y*6.5f;
        float verticalViewportSize = verticalViewportPixelSize/mazeSys.getCanvasHeight();
        lvlBox.GetComponent<RectTransform>().anchorMax = new Vector2(0.945f, 0.5f + verticalViewportSize/2f);
        lvlBox.GetComponent<RectTransform>().anchorMin = new Vector2(0.055f, 0.5f - verticalViewportSize/2f);
        
        worldNameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.54f + verticalViewportSize/2f);
        worldNameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.54f + verticalViewportSize/2f);
        leftButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].x, 0.54f + verticalViewportSize/2f);
        leftButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].x, 0.54f + verticalViewportSize/2f);
        rightButton.GetComponent<RectTransform>().anchorMin = new Vector2(arrowWorldPositions[actualWorld - 1].y, 0.54f + verticalViewportSize/2f);
        rightButton.GetComponent<RectTransform>().anchorMax = new Vector2(arrowWorldPositions[actualWorld - 1].y, 0.54f + verticalViewportSize/2f);

        levelsViewport.GetComponent<RectTransform>().offsetMax = new Vector2(lvlVwprtRT.offsetMax.x, topMargin);
        StartAnimations();
        StartGoBackAnimation();
    }

}

var coverflow;

  FWDS3DCovUtils.onReady(function()
  {
    coverflow = new FWDSimple3DCoverflow(
    {
      //required settings
      coverflowHolderDivId:"myDiv",
      coverflowDataListDivId:"coverflowData",
      displayType:"fluidwidth",
      autoScale:"yes",
      coverflowWidth:940,
      coverflowHeight:738,
      skinPath:"load/skin_black",

      //main settings
      backgroundColor:"#000000",
      backgroundImagePath:"",
      backgroundRepeat:"repeat-x",
      showDisplay2DAlways:"no",
      coverflowStartPosition:"left",
      coverflowTopology:"dualsided",
      coverflowXRotation:0,
      coverflowYRotation:0,
      numberOfThumbnailsToDisplayLeftAndRight:"all",
      infiniteLoop:"no",
      rightClickContextMenu:"developer",
      fluidWidthZIndex:1000,

      //thumbnail settings
      thumbnailWidth:400,
      thumbnailHeight:266,
      thumbnailXOffset3D:86,
      thumbnailXSpace3D:78,
      thumbnailZOffset3D:200,
      thumbnailZSpace3D:93,
      thumbnailYAngle3D:20,
      thumbnailXOffset2D:20,
      thumbnailXSpace2D:30,
      thumbnailHoverOffset:100,
      thumbnailBorderSize:0,
      thumbnailBackgroundColor:"#FFFFFF",
      thumbnailBorderColor1:"#FFFFFF",
      thumbnailBorderColor2:"#FFFFFF",
      transparentImages:"no",
      thumbnailsAlignment:"center",
      maxNumberOfThumbnailsOnMobile:13,
      showThumbnailsGradient:"yes",
      thumbnailGradientColor1:"rgba(0, 0, 0, 0)",
      thumbnailGradientColor2:"rgba(0, 0, 0, 1)",
      showText:"yes",
      textOffset:10,
      showThumbnailBoxShadow:"yes",
      thumbnailBoxShadowCss:"0px 2px 2px #111111",
      showTooltip:"no",
      dynamicTooltip:"yes",
      showReflection:"yes",
      reflectionHeight:40,
      reflectionDistance:0,
      reflectionOpacity:.4,

      //controls settings
      slideshowDelay:5000,
      autoplay:"no",
      disableNextAndPrevButtonsOnMobile:"no",
      controlsMaxWidth:700,
      slideshowTimerColor:"#FFFFFF",
      controlsPosition:"bottom",
      controlsOffset:15,
      showPrevButton:"yes",
      showNextButton:"yes",
      showSlideshowButton:"yes",
      showScrollbar:"yes",
      disableScrollbarOnMobile:"yes",
      enableMouseWheelScroll:"yes",
      scrollbarHandlerWidth:200,
      scrollbarTextColorNormal:"#000000",
      scrollbarTextColorSelected:"#FFFFFF",
      addKeyboardSupport:"yes",

      //categories settings
      showCategoriesMenu:"no",
      startAtCategory:1,
      categoriesMenuMaxWidth:700,
      categoriesMenuOffset:0,
      categoryColorNormal:"#999999",
      categoryColorSelected:"#FFFFFF",

      //lightbox settings
      addLightBoxKeyboardSupport:"yes",
      showLightBoxNextAndPrevButtons:"yes",
      showLightBoxZoomButton:"yes",
      showLightBoxInfoButton:"yes",
      showLightBoxSlideShowButton:"yes",
      showLightBoxInfoWindowByDefault:"no",
      slideShowAutoPlay:"no",
      lightBoxVideoAutoPlay:"no",
      lightBoxVideoWidth:640,
      lightBoxVideoHeight:480,
      lightBoxIframeWidth:800,
      lightBoxIframeHeight:600,
      lightBoxBackgroundColor:"#000000",
      lightBoxInfoWindowBackgroundColor:"#FFFFFF",
      lightBoxItemBorderColor1:"#fcfdfd",
      lightBoxItemBorderColor2:"#e4FFe4",
      lightBoxItemBackgroundColor:"#333333",
      lightBoxMainBackgroundOpacity:.8,
      lightBoxInfoWindowBackgroundOpacity:.9,
      lightBoxBorderSize:0,
      lightBoxBorderRadius:0,
      lightBoxSlideShowDelay:4000
    });

    coverflow.addListener(FWDSimple3DCoverflow.IS_API_READY, onApiReady);
    coverflow.addListener(FWDSimple3DCoverflow.THUMB_CHANGE, onThumbChange);
    coverflow.addListener(FWDSimple3DCoverflow.CATEGORY_CHANGE, onCategoryChange);
  });

  function gotoNextCategory()
  {
    coverflow.switchCategory(coverflow.getCurrentCategoryId() + 1);
  }

  function gotoNextThumb()
  {
    coverflow.gotoThumb(coverflow.getCurrentThumbId() + 1);
  }

  function gotoPrevThumb()
  {
    coverflow.gotoThumb(coverflow.getCurrentThumbId() - 1);
  }

  function startStopSlideshow()
  {
    if (coverflow.isSlideshowPlaying())
    {
      coverflow.stopSlideshow();
    }
    else
    {
      coverflow.startSlideshow();
    }
  }

  function onApiReady()
  {
    console.log("api ready");
  }

  function onThumbChange(ev)
  {
    console.log("thumb id: " + ev.id);
  }

  function onCategoryChange(ev)
  {
    console.log("category id: " + ev.id);
  }

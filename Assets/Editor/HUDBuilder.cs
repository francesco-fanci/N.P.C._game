#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class HUDBuilder
{
    [MenuItem("N.P.C./Build Diegetic HUD")]
    public static void BuildHUD()
    {
        // 1. Find or create Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("HUD_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create HUD Canvas");
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // 2. Ensure EventSystem exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // 3. Load font
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"
        );

        // Remove old HUD elements if existing
        Transform oldRoot = canvasObj.transform.Find("HUD_Root");
        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot.gameObject);
        }

        GameObject hudRoot = new GameObject("HUD_Root");
        hudRoot.transform.SetParent(canvasObj.transform, false);
        RectTransform rootRect = hudRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // 4. TOP-RIGHT: SERVER COUNTDOWN
        GameObject countdownObj = new GameObject("CountdownText");
        countdownObj.transform.SetParent(hudRoot.transform, false);
        RectTransform countRect = countdownObj.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1, 1);
        countRect.anchorMax = new Vector2(1, 1);
        countRect.pivot = new Vector2(1, 1);
        countRect.anchoredPosition = new Vector2(-40, -40);
        countRect.sizeDelta = new Vector2(500, 60);

        TextMeshProUGUI countTMP = countdownObj.AddComponent<TextMeshProUGUI>();
        if (font != null) countTMP.font = font;
        countTMP.text = "SERVER WIPE IN 19:59";
        countTMP.fontSize = 38;
        countTMP.fontStyle = FontStyles.Bold;
        countTMP.alignment = TextAlignmentOptions.Right;
        countTMP.color = new Color(1f, 0.25f, 0.25f, 1f);

        // Subtext under countdown
        GameObject gcAlertObj = new GameObject("GC_Subtext");
        gcAlertObj.transform.SetParent(countdownObj.transform, false);
        RectTransform gcRect = gcAlertObj.AddComponent<RectTransform>();
        gcRect.anchorMin = new Vector2(0, 0);
        gcRect.anchorMax = new Vector2(1, 0);
        gcRect.pivot = new Vector2(1, 1);
        gcRect.anchoredPosition = new Vector2(0, -5);
        gcRect.sizeDelta = new Vector2(0, 30);

        TextMeshProUGUI gcTMP = gcAlertObj.AddComponent<TextMeshProUGUI>();
        if (font != null) gcTMP.font = font;
        gcTMP.text = "DAEMON: Garbage Collector Active";
        gcTMP.fontSize = 18;
        gcTMP.alignment = TextAlignmentOptions.Right;
        gcTMP.color = new Color(1f, 0.5f, 0.2f, 0.9f);

        // 5. TOP-LEFT: MEMORY LEAK METER
        GameObject memPanel = new GameObject("MemoryLeakPanel");
        memPanel.transform.SetParent(hudRoot.transform, false);
        RectTransform memRect = memPanel.AddComponent<RectTransform>();
        memRect.anchorMin = new Vector2(0, 1);
        memRect.anchorMax = new Vector2(0, 1);
        memRect.pivot = new Vector2(0, 1);
        memRect.anchoredPosition = new Vector2(40, -40);
        memRect.sizeDelta = new Vector2(360, 140);

        // Title
        GameObject memTitle = new GameObject("Title");
        memTitle.transform.SetParent(memPanel.transform, false);
        RectTransform titleRect = memTitle.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0, 1);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0, 32);

        TextMeshProUGUI titleTMP = memTitle.AddComponent<TextMeshProUGUI>();
        if (font != null) titleTMP.font = font;
        titleTMP.text = "MEMORY LEAK";
        titleTMP.fontSize = 26;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(0.2f, 0.95f, 1f, 1f); // Neon Cyan

        // Slider (RAM Bar)
        GameObject sliderObj = new GameObject("RAM_Slider");
        sliderObj.transform.SetParent(memPanel.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(1, 1);
        sliderRect.pivot = new Vector2(0, 1);
        sliderRect.anchoredPosition = new Vector2(0, -36);
        sliderRect.sizeDelta = new Vector2(0, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.18f;
        slider.interactable = false;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.15f, 0.2f, 0.8f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.1f, 0.9f, 0.9f, 1f); // Cyan fill

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImg;

        // RAM Usage Text
        GameObject ramUsageObj = new GameObject("RAM_UsageText");
        ramUsageObj.transform.SetParent(memPanel.transform, false);
        RectTransform rutRect = ramUsageObj.AddComponent<RectTransform>();
        rutRect.anchorMin = new Vector2(0, 1);
        rutRect.anchorMax = new Vector2(1, 1);
        rutRect.pivot = new Vector2(0, 1);
        rutRect.anchoredPosition = new Vector2(0, -62);
        rutRect.sizeDelta = new Vector2(0, 24);

        TextMeshProUGUI rutTMP = ramUsageObj.AddComponent<TextMeshProUGUI>();
        if (font != null) rutTMP.font = font;
        rutTMP.text = "RAM USAGE: 18%";
        rutTMP.fontSize = 18;
        rutTMP.color = Color.white;

        // Warning Text
        GameObject warnObj = new GameObject("WarningText");
        warnObj.transform.SetParent(memPanel.transform, false);
        RectTransform warnRect = warnObj.AddComponent<RectTransform>();
        warnRect.anchorMin = new Vector2(0, 1);
        warnRect.anchorMax = new Vector2(1, 1);
        warnRect.pivot = new Vector2(0, 1);
        warnRect.anchoredPosition = new Vector2(0, -86);
        warnRect.sizeDelta = new Vector2(0, 24);

        TextMeshProUGUI warnTMP = warnObj.AddComponent<TextMeshProUGUI>();
        if (font != null) warnTMP.font = font;
        warnTMP.text = "STATUS: MEMORY_LEAK_TRACKED";
        warnTMP.fontSize = 16;
        warnTMP.fontStyle = FontStyles.Bold;
        warnTMP.color = new Color(0.2f, 0.9f, 0.9f);

        // Crates Counter Text
        GameObject cratesObj = new GameObject("CratesCounterText");
        cratesObj.transform.SetParent(memPanel.transform, false);
        RectTransform cratesRect = cratesObj.AddComponent<RectTransform>();
        cratesRect.anchorMin = new Vector2(0, 1);
        cratesRect.anchorMax = new Vector2(1, 1);
        cratesRect.pivot = new Vector2(0, 1);
        cratesRect.anchoredPosition = new Vector2(0, -112);
        cratesRect.sizeDelta = new Vector2(0, 26);

        TextMeshProUGUI cratesTMP = cratesObj.AddComponent<TextMeshProUGUI>();
        if (font != null) cratesTMP.font = font;
        cratesTMP.text = "Active Crates: 0/5";
        cratesTMP.fontSize = 18;
        cratesTMP.color = new Color(1f, 0.85f, 0.3f); // Light yellow

        // 6. CENTER RETICLE
        GameObject reticle = new GameObject("CenterReticle");
        reticle.transform.SetParent(hudRoot.transform, false);
        RectTransform reticleRect = reticle.AddComponent<RectTransform>();
        reticleRect.anchorMin = new Vector2(0.5f, 0.5f);
        reticleRect.anchorMax = new Vector2(0.5f, 0.5f);
        reticleRect.pivot = new Vector2(0.5f, 0.5f);
        reticleRect.anchoredPosition = Vector2.zero;
        reticleRect.sizeDelta = new Vector2(6, 6);

        Image reticleImg = reticle.AddComponent<Image>();
        reticleImg.color = new Color(1f, 1f, 1f, 0.85f);

        // 7. BOTTOM-LEFT: PROMPT
        GameObject promptObj = new GameObject("ActionPrompt");
        promptObj.transform.SetParent(hudRoot.transform, false);
        RectTransform promptRect = promptObj.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0, 0);
        promptRect.anchorMax = new Vector2(0, 0);
        promptRect.pivot = new Vector2(0, 0);
        promptRect.anchoredPosition = new Vector2(40, 40);
        promptRect.sizeDelta = new Vector2(500, 40);

        TextMeshProUGUI promptTMP = promptObj.AddComponent<TextMeshProUGUI>();
        if (font != null) promptTMP.font = font;
        promptTMP.text = "[E] Interact / Duplicate Crate";
        promptTMP.fontSize = 22;
        promptTMP.color = new Color(0.2f, 0.95f, 1f, 0.9f);

        // 8. CENTER-BOTTOM: CORRUPTED DIALOGUE POPUP
        GameObject diagPanel = new GameObject("CorruptedDialoguePanel");
        diagPanel.transform.SetParent(hudRoot.transform, false);
        RectTransform diagRect = diagPanel.AddComponent<RectTransform>();
        diagRect.anchorMin = new Vector2(0.5f, 0f);
        diagRect.anchorMax = new Vector2(0.5f, 0f);
        diagRect.pivot = new Vector2(0.5f, 0f);
        diagRect.anchoredPosition = new Vector2(0, 110);
        diagRect.sizeDelta = new Vector2(750, 90);

        Image diagBg = diagPanel.AddComponent<Image>();
        diagBg.color = new Color(0.05f, 0.08f, 0.12f, 0.92f); // Dark translucent console background

        GameObject diagTextObj = new GameObject("DialogueText");
        diagTextObj.transform.SetParent(diagPanel.transform, false);
        RectTransform dtRect = diagTextObj.AddComponent<RectTransform>();
        dtRect.anchorMin = Vector2.zero;
        dtRect.anchorMax = Vector2.one;
        dtRect.offsetMin = new Vector2(20, 10);
        dtRect.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI dtTMP = diagTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) dtTMP.font = font;
        dtTMP.text = "Error 404: DialogueString Missing\n[Uncaught NullPointerException]";
        dtTMP.fontSize = 20;
        dtTMP.alignment = TextAlignmentOptions.Center;
        dtTMP.color = new Color(1f, 0.2f, 0.7f, 1f); // Neon Magenta/Pink glitch text

        diagPanel.SetActive(false); // Hidden by default

        // 9. CENTER: CRASH LOG MODAL
        GameObject logModal = new GameObject("CrashLogModal");
        logModal.transform.SetParent(hudRoot.transform, false);
        RectTransform lmRect = logModal.AddComponent<RectTransform>();
        lmRect.anchorMin = new Vector2(0.5f, 0.5f);
        lmRect.anchorMax = new Vector2(0.5f, 0.5f);
        lmRect.pivot = new Vector2(0.5f, 0.5f);
        lmRect.anchoredPosition = Vector2.zero;
        lmRect.sizeDelta = new Vector2(760, 380);

        Image lmBg = logModal.AddComponent<Image>();
        lmBg.color = new Color(0.04f, 0.06f, 0.1f, 0.96f);

        // Header Title
        GameObject lmTitleObj = new GameObject("LogTitle");
        lmTitleObj.transform.SetParent(logModal.transform, false);
        RectTransform lmtRect = lmTitleObj.AddComponent<RectTransform>();
        lmtRect.anchorMin = new Vector2(0, 1);
        lmtRect.anchorMax = new Vector2(1, 1);
        lmtRect.pivot = new Vector2(0.5f, 1);
        lmtRect.anchoredPosition = new Vector2(0, -15);
        lmtRect.sizeDelta = new Vector2(-40, 35);

        TextMeshProUGUI lmtTMP = lmTitleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) lmtTMP.font = font;
        lmtTMP.text = "CRASH_LOG_2012_SECTOR01.TXT";
        lmtTMP.fontSize = 22;
        lmtTMP.fontStyle = FontStyles.Bold;
        lmtTMP.color = new Color(0.2f, 0.95f, 1f); // Neon Cyan

        // Content Body
        GameObject lmBodyObj = new GameObject("LogBody");
        lmBodyObj.transform.SetParent(logModal.transform, false);
        RectTransform lmbRect = lmBodyObj.AddComponent<RectTransform>();
        lmbRect.anchorMin = Vector2.zero;
        lmbRect.anchorMax = Vector2.one;
        lmbRect.offsetMin = new Vector2(25, 45);
        lmbRect.offsetMax = new Vector2(-25, -55);

        TextMeshProUGUI lmbTMP = lmBodyObj.AddComponent<TextMeshProUGUI>();
        if (font != null) lmbTMP.font = font;
        lmbTMP.fontSize = 17;
        lmbTMP.color = new Color(0.85f, 0.9f, 0.95f);
        lmbTMP.text = "Developer memo content...";

        // Close Hint at bottom of modal
        GameObject lmHintObj = new GameObject("CloseHint");
        lmHintObj.transform.SetParent(logModal.transform, false);
        RectTransform lmhRect = lmHintObj.AddComponent<RectTransform>();
        lmhRect.anchorMin = new Vector2(0, 0);
        lmhRect.anchorMax = new Vector2(1, 0);
        lmhRect.pivot = new Vector2(0.5f, 0);
        lmhRect.anchoredPosition = new Vector2(0, 12);
        lmhRect.sizeDelta = new Vector2(0, 24);

        TextMeshProUGUI lmhTMP = lmHintObj.AddComponent<TextMeshProUGUI>();
        if (font != null) lmhTMP.font = font;
        lmhTMP.text = "[ Press E to close file ]";
        lmhTMP.fontSize = 16;
        lmhTMP.alignment = TextAlignmentOptions.Center;
        lmhTMP.color = new Color(0.5f, 0.6f, 0.7f);

        logModal.SetActive(false); // Hidden by default

        // 10. TOP-CENTER: SECTOR COMPLETE BANNER
        GameObject bannerObj = new GameObject("SectorCompleteBanner");
        bannerObj.transform.SetParent(hudRoot.transform, false);
        RectTransform bannerRect = bannerObj.AddComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0.5f, 1f);
        bannerRect.anchorMax = new Vector2(0.5f, 1f);
        bannerRect.pivot = new Vector2(0.5f, 1f);
        bannerRect.anchoredPosition = new Vector2(0, -120);
        bannerRect.sizeDelta = new Vector2(850, 80);

        Image bannerBg = bannerObj.AddComponent<Image>();
        bannerBg.color = new Color(0.02f, 0.2f, 0.1f, 0.92f); // Matrix dark green background

        GameObject bannerTextObj = new GameObject("BannerText");
        bannerTextObj.transform.SetParent(bannerObj.transform, false);
        RectTransform btRect = bannerTextObj.AddComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.offsetMin = new Vector2(15, 5);
        btRect.offsetMax = new Vector2(-15, -5);

        TextMeshProUGUI btTMP = bannerTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) btTMP.font = font;
        btTMP.text = "SECTOR 01: THE BROKEN VILLAGE ESCAPED\nDATA EXPORTED TO WIREFRAME BACKSTAGE";
        btTMP.fontSize = 22;
        btTMP.fontStyle = FontStyles.Bold;
        btTMP.alignment = TextAlignmentOptions.Center;
        btTMP.color = new Color(0.3f, 1f, 0.5f); // Bright Matrix green

        bannerObj.SetActive(false); // Hidden by default

        // 11. TOP-CENTER: OBJECTIVE TRACKER
        GameObject objPanel = new GameObject("ObjectiveTrackerPanel");
        objPanel.transform.SetParent(hudRoot.transform, false);
        RectTransform objRect = objPanel.AddComponent<RectTransform>();
        objRect.anchorMin = new Vector2(0.5f, 1f);
        objRect.anchorMax = new Vector2(0.5f, 1f);
        objRect.pivot = new Vector2(0.5f, 1f);
        objRect.anchoredPosition = new Vector2(0, -25);
        objRect.sizeDelta = new Vector2(820, 36);

        Image objBg = objPanel.AddComponent<Image>();
        objBg.color = new Color(0.04f, 0.08f, 0.12f, 0.88f);

        GameObject objTextObj = new GameObject("ObjectiveText");
        objTextObj.transform.SetParent(objPanel.transform, false);
        RectTransform otRect = objTextObj.AddComponent<RectTransform>();
        otRect.anchorMin = Vector2.zero;
        otRect.anchorMax = Vector2.one;
        otRect.offsetMin = new Vector2(15, 0);
        otRect.offsetMax = new Vector2(-15, 0);

        TextMeshProUGUI objTMP = objTextObj.AddComponent<TextMeshProUGUI>();
        if (font != null) objTMP.font = font;
        objTMP.text = "PROTOCOL 01: Find an exploit to bypass village perimeter before server wipe.";
        objTMP.fontSize = 16;
        objTMP.fontStyle = FontStyles.Bold;
        objTMP.alignment = TextAlignmentOptions.Center;
        objTMP.color = new Color(0.2f, 0.95f, 0.7f); // Mint green debugger color

        // 12. CENTER: PAUSE / DEBUGGER MODAL (ESC)
        GameObject pauseModal = new GameObject("PauseMenuModal");
        pauseModal.transform.SetParent(hudRoot.transform, false);
        RectTransform pmRect = pauseModal.AddComponent<RectTransform>();
        pmRect.anchorMin = new Vector2(0.5f, 0.5f);
        pmRect.anchorMax = new Vector2(0.5f, 0.5f);
        pmRect.pivot = new Vector2(0.5f, 0.5f);
        pmRect.anchoredPosition = Vector2.zero;
        pmRect.sizeDelta = new Vector2(560, 480);

        Image pmBg = pauseModal.AddComponent<Image>();
        pmBg.color = new Color(0.03f, 0.05f, 0.08f, 0.97f);

        // Pause Title
        GameObject pmTitleObj = new GameObject("Title");
        pmTitleObj.transform.SetParent(pauseModal.transform, false);
        RectTransform pmtRect = pmTitleObj.AddComponent<RectTransform>();
        pmtRect.anchorMin = new Vector2(0, 1);
        pmtRect.anchorMax = new Vector2(1, 1);
        pmtRect.pivot = new Vector2(0.5f, 1);
        pmtRect.anchoredPosition = new Vector2(0, -25);
        pmtRect.sizeDelta = new Vector2(0, 40);

        TextMeshProUGUI pmtTMP = pmTitleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) pmtTMP.font = font;
        pmtTMP.text = "=== ENGINE EXECUTION PAUSED ===";
        pmtTMP.fontSize = 22;
        pmtTMP.fontStyle = FontStyles.Bold;
        pmtTMP.alignment = TextAlignmentOptions.Center;
        pmtTMP.color = new Color(0.2f, 0.95f, 1f);

        // Pause Subtitle
        GameObject pmSubObj = new GameObject("Subtitle");
        pmSubObj.transform.SetParent(pauseModal.transform, false);
        RectTransform pmsRect = pmSubObj.AddComponent<RectTransform>();
        pmsRect.anchorMin = new Vector2(0, 1);
        pmsRect.anchorMax = new Vector2(1, 1);
        pmsRect.pivot = new Vector2(0.5f, 1);
        pmsRect.anchoredPosition = new Vector2(0, -65);
        pmsRect.sizeDelta = new Vector2(0, 24);

        TextMeshProUGUI pmsTMP = pmSubObj.AddComponent<TextMeshProUGUI>();
        if (font != null) pmsTMP.font = font;
        pmsTMP.text = "[THREAD STATUS: SUSPENDED // EXPLOITS READY]";
        pmsTMP.fontSize = 14;
        pmsTMP.alignment = TextAlignmentOptions.Center;
        pmsTMP.color = new Color(1f, 0.55f, 0.2f); // Orange debugger warning

        // Buttons
        Button btnResume = CreateMenuButton(pauseModal.transform, "Button_Resume", "[ RESUME EXECUTION ]", new Vector2(0, 45), new Vector2(400, 48), font);
        Button btnRespawn = CreateMenuButton(pauseModal.transform, "Button_RespawnCheckpoint", "[ RESTORE LAST CHECKPOINT ]", new Vector2(0, -15), new Vector2(400, 48), font);
        Button btnRestart = CreateMenuButton(pauseModal.transform, "Button_RestartSector", "[ RESTART SECTOR 01 ]", new Vector2(0, -75), new Vector2(400, 48), font);
        Button btnControls = CreateMenuButton(pauseModal.transform, "Button_Controls", "[ EXPLOITS & CONTROLS MANUAL ]", new Vector2(0, -135), new Vector2(400, 48), font);

        // Return hint
        GameObject pmHintObj = new GameObject("Hint");
        pmHintObj.transform.SetParent(pauseModal.transform, false);
        RectTransform pmhRect = pmHintObj.AddComponent<RectTransform>();
        pmhRect.anchorMin = new Vector2(0, 0);
        pmhRect.anchorMax = new Vector2(1, 0);
        pmhRect.pivot = new Vector2(0.5f, 0);
        pmhRect.anchoredPosition = new Vector2(0, 20);
        pmhRect.sizeDelta = new Vector2(0, 24);

        TextMeshProUGUI pmhTMP = pmHintObj.AddComponent<TextMeshProUGUI>();
        if (font != null) pmhTMP.font = font;
        pmhTMP.text = "[ Press ESC to resume execution ]";
        pmhTMP.fontSize = 15;
        pmhTMP.alignment = TextAlignmentOptions.Center;
        pmhTMP.color = new Color(0.5f, 0.6f, 0.7f);

        // 13. CONTROLS & EXPLOITS SUB-PANEL (MODAL)
        GameObject controlsModal = new GameObject("ControlsGuideModal");
        controlsModal.transform.SetParent(hudRoot.transform, false);
        RectTransform cmRect = controlsModal.AddComponent<RectTransform>();
        cmRect.anchorMin = new Vector2(0.5f, 0.5f);
        cmRect.anchorMax = new Vector2(0.5f, 0.5f);
        cmRect.pivot = new Vector2(0.5f, 0.5f);
        cmRect.anchoredPosition = Vector2.zero;
        cmRect.sizeDelta = new Vector2(740, 520);

        Image cmBg = controlsModal.AddComponent<Image>();
        cmBg.color = new Color(0.02f, 0.04f, 0.08f, 0.98f);

        GameObject cmTitleObj = new GameObject("ManualTitle");
        cmTitleObj.transform.SetParent(controlsModal.transform, false);
        RectTransform cmtRect = cmTitleObj.AddComponent<RectTransform>();
        cmtRect.anchorMin = new Vector2(0, 1);
        cmtRect.anchorMax = new Vector2(1, 1);
        cmtRect.pivot = new Vector2(0.5f, 1);
        cmtRect.anchoredPosition = new Vector2(0, -20);
        cmtRect.sizeDelta = new Vector2(0, 35);

        TextMeshProUGUI cmtTMP = cmTitleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) cmtTMP.font = font;
        cmtTMP.text = "NULL POINTER CHARACTER // EXPLOIT MANUAL";
        cmtTMP.fontSize = 20;
        cmtTMP.fontStyle = FontStyles.Bold;
        cmtTMP.alignment = TextAlignmentOptions.Center;
        cmtTMP.color = new Color(0.2f, 0.95f, 1f);

        GameObject cmBodyObj = new GameObject("ManualBody");
        cmBodyObj.transform.SetParent(controlsModal.transform, false);
        RectTransform cmbRect = cmBodyObj.AddComponent<RectTransform>();
        cmbRect.anchorMin = Vector2.zero;
        cmbRect.anchorMax = Vector2.one;
        cmbRect.offsetMin = new Vector2(35, 75);
        cmbRect.offsetMax = new Vector2(-35, -60);

        TextMeshProUGUI cmbTMP = cmBodyObj.AddComponent<TextMeshProUGUI>();
        if (font != null) cmbTMP.font = font;
        cmbTMP.fontSize = 16;
        cmbTMP.lineSpacing = 10;
        cmbTMP.color = new Color(0.85f, 0.92f, 1f);
        cmbTMP.text =
            "<b>[ BASIC CONTROLS ]</b>\n" +
            "  • <b>WASD</b>: Move Character (Camera-relative)\n" +
            "  • <b>MOUSE</b>: Orbital Camera Rotation\n" +
            "  • <b>LEFT SHIFT</b>: Sprint Acceleration\n" +
            "  • <b>SPACE</b>: Jump\n\n" +
            "<b>[ ENGINE EXPLOITS / GLITCHES ]</b>\n" +
            "  • <b>[E] ITEM DUPLICATION</b>: Duplicates physics crates (cap: 5) to build stairs & climb barriers.\n" +
            "  • <b>COYOTE JUMP (\"False Jump\")</b>: 0.2s grace buffer allows jumping even after leaving a ledge.\n" +
            "  • <b>WALL PHASING</b>: Walk into untextured glitch seams (#FF00FF) to phase into unallocated space.\n" +
            "  • <b>[E] INTERACTION</b>: Speaks to corrupted NPCs & reads developer crash logs.";

        Button btnCloseControls = CreateMenuButton(controlsModal.transform, "Button_CloseManual", "[ CLOSE MANUAL ]", new Vector2(0, -215), new Vector2(250, 42), font);

        controlsModal.SetActive(false);
        pauseModal.SetActive(false);

        // 14. Attach and Wire HUDManager
        HUDManager hudManager = canvasObj.GetComponent<HUDManager>();
        if (hudManager == null) hudManager = canvasObj.AddComponent<HUDManager>();

        // Wire Button persistent click events
        UnityEventTools.AddPersistentListener(btnResume.onClick, hudManager.ResumeGame);
        UnityEventTools.AddPersistentListener(btnRespawn.onClick, hudManager.RespawnFromCheckpoint);
        UnityEventTools.AddPersistentListener(btnRestart.onClick, hudManager.RestartSector);
        UnityEventTools.AddPersistentListener(btnControls.onClick, hudManager.ToggleControlsModal);
        UnityEventTools.AddPersistentListener(btnCloseControls.onClick, hudManager.ToggleControlsModal);

        SerializedObject so = new SerializedObject(hudManager);
        so.FindProperty("_countdownText").objectReferenceValue = countTMP;
        so.FindProperty("_ramSlider").objectReferenceValue = slider;
        so.FindProperty("_ramUsageText").objectReferenceValue = rutTMP;
        so.FindProperty("_crateCounterText").objectReferenceValue = cratesTMP;
        so.FindProperty("_warningBannerText").objectReferenceValue = warnTMP;
        so.FindProperty("_centerReticle").objectReferenceValue = reticle;
        so.FindProperty("_actionPromptText").objectReferenceValue = promptTMP;
        so.FindProperty("_dialoguePanel").objectReferenceValue = diagPanel;
        so.FindProperty("_dialogueText").objectReferenceValue = dtTMP;
        so.FindProperty("_crashLogPanel").objectReferenceValue = logModal;
        so.FindProperty("_crashLogTitleText").objectReferenceValue = lmtTMP;
        so.FindProperty("_crashLogBodyText").objectReferenceValue = lmbTMP;
        so.FindProperty("_sectorCompleteBanner").objectReferenceValue = bannerObj;
        so.FindProperty("_sectorCompleteText").objectReferenceValue = btTMP;
        so.FindProperty("_objectivePanel").objectReferenceValue = objPanel;
        so.FindProperty("_objectiveText").objectReferenceValue = objTMP;
        so.FindProperty("_pauseMenuPanel").objectReferenceValue = pauseModal;
        so.FindProperty("_controlsSubPanel").objectReferenceValue = controlsModal;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = canvasObj;
        Debug.Log("<color=cyan><b>[N.P.C.] Diegetic HUD with Objectives & Pause Menu successfully built and wired!</b></color>");
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, TMP_FontAsset font)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.10f, 0.20f, 0.32f, 0.95f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.10f, 0.20f, 0.32f, 0.95f);
        cb.highlightedColor = new Color(0.18f, 0.38f, 0.58f, 1f);
        cb.pressedColor = new Color(0.06f, 0.12f, 0.20f, 1f);
        btn.colors = cb;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 17;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.85f, 0.95f, 1f);

        return btn;
    }

    [MenuItem("N.P.C./Spawn Garbage Collector Enemy")]
    public static void SpawnGarbageCollector()
    {
        // 1. Root Monolith
        GameObject gc = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gc.name = "GarbageCollector_Daemon";
        gc.transform.position = new Vector3(-5f, 1.25f, 5f);
        gc.transform.localScale = new Vector3(1.2f, 2.5f, 1.2f);

        // Dark material or color
        Renderer rend = gc.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.sharedMaterial = CreateLitMaterial(new Color(0.12f, 0.14f, 0.18f)); // Dark monolithic grey
        }

        // 2. Spotlight
        GameObject lightObj = new GameObject("Red_Sensor_Spotlight");
        lightObj.transform.SetParent(gc.transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 0.8f, 0.5f);
        lightObj.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);

        Light spot = lightObj.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.color = new Color(1f, 0.15f, 0.15f);
        spot.intensity = 15f;
        spot.range = 10f;
        spot.spotAngle = 55f;

        // 3. Detection Trigger Zone
        GameObject detectorObj = new GameObject("DetectionCone_Trigger");
        detectorObj.transform.SetParent(lightObj.transform, false);
        detectorObj.transform.localPosition = new Vector3(0f, 0f, 3.5f);
        detectorObj.transform.localRotation = Quaternion.identity;
        detectorObj.transform.localScale = new Vector3(2.5f, 2.5f, 5f);

        BoxCollider triggerCol = detectorObj.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;

        GarbageCollectorDetector detector = detectorObj.AddComponent<GarbageCollectorDetector>();

        // 4. FSM Component
        GarbageCollectorFSM fsm = gc.AddComponent<GarbageCollectorFSM>();

        // 5. Waypoints Root & 2 Waypoints
        GameObject wpRoot = new GameObject("Waypoints_" + gc.name);
        wpRoot.transform.position = gc.transform.position;

        GameObject wp1 = new GameObject("WP_A");
        wp1.transform.SetParent(wpRoot.transform, false);
        wp1.transform.position = new Vector3(-5f, 1.25f, 2f);

        GameObject wp2 = new GameObject("WP_B");
        wp2.transform.SetParent(wpRoot.transform, false);
        wp2.transform.position = new Vector3(-5f, 1.25f, 12f);

        // Wire FSM
        SerializedObject so = new SerializedObject(fsm);
        so.FindProperty("_spotlight").objectReferenceValue = spot;

        SerializedProperty wpProp = so.FindProperty("_waypoints");
        wpProp.ClearArray();
        wpProp.InsertArrayElementAtIndex(0);
        wpProp.GetArrayElementAtIndex(0).objectReferenceValue = wp1.transform;
        wpProp.InsertArrayElementAtIndex(1);
        wpProp.GetArrayElementAtIndex(1).objectReferenceValue = wp2.transform;

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(gc, "Spawn Garbage Collector");
        Undo.RegisterCreatedObjectUndo(wpRoot, "Spawn GC Waypoints");

        Selection.activeGameObject = gc;
        Debug.Log("<color=red><b>[N.P.C.] Garbage Collector spawned with automated patrol waypoints!</b></color>");
    }

    [MenuItem("N.P.C./Spawn Barkeep NPC (Corrupted Dialogue)")]
    public static void SpawnBarkeepNPC()
    {
        GameObject barkeep = CreateBarkeepCharacter(null, new Vector3(2f, 0f, 3f));
        Undo.RegisterCreatedObjectUndo(barkeep, "Spawn Barkeep NPC");
        Selection.activeGameObject = barkeep;
        Debug.Log("<color=yellow><b>[N.P.C.] Stylized 3D Barkeep NPC spawned with lively idle animations!</b></color>");
    }

    [MenuItem("N.P.C./Replace Player Capsule with 3D Merchant Model")]
    public static void UpgradePlayerModelMenu()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        if (player != null)
        {
            AttachMerchantModelToPlayer(player);
            Selection.activeGameObject = player;
            Debug.Log("<color=green><b>[N.P.C.] Player successfully upgraded to 3D Stylized Merchant Model with walk, sprint, jump and squash animations!</b></color>");
        }
        else
        {
            Debug.LogWarning("[N.P.C.] No GameObject tagged 'Player' or named 'Player' found in the scene.");
        }
    }

    [MenuItem("N.P.C./Build Level 1 Greybox (Village Square)")]
    public static void BuildLevel1Greybox()
    {
        // 0. Clean up obsolete test objects from hierarchy
        string[] tempObjNames = new string[] { "Platform_A", "Platform_B", "Plane", "GlitchWall" };
        foreach (string tempName in tempObjNames)
        {
            GameObject tempObj = GameObject.Find(tempName);
            if (tempObj != null)
            {
                Undo.DestroyObjectImmediate(tempObj);
                Debug.Log($"[N.P.C.] Removed obsolete test object: {tempName}");
            }
        }

        // Clean up previous level instance if present
        GameObject oldRoot = GameObject.Find("--- LEVEL 01: THE BROKEN VILLAGE ---");
        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(oldRoot);
        }

        // 1. Root Level Object
        GameObject levelRoot = new GameObject("--- LEVEL 01: THE BROKEN VILLAGE ---");
        Undo.RegisterCreatedObjectUndo(levelRoot, "Create Level 1");

        // 2. Materials Setup (Medieval Palette)
        Material wallStoneMat = CreateLitMaterial(new Color(0.72f, 0.70f, 0.65f));      // Warm stone / plaster
        Material cobbleFloorMat = CreateLitMaterial(new Color(0.38f, 0.38f, 0.36f));    // Cobblestone ground
        Material timberDarkMat = CreateLitMaterial(new Color(0.28f, 0.18f, 0.10f));     // Dark timber beams
        Material timberLightMat = CreateLitMaterial(new Color(0.50f, 0.35f, 0.20f));    // Wood planks & counter
        Material roofTerracottaMat = CreateLitMaterial(new Color(0.55f, 0.25f, 0.18f)); // Terracotta tiles
        Material roofSlateMat = CreateLitMaterial(new Color(0.25f, 0.27f, 0.30f));      // Dark slate roof
        Material canopyRedMat = CreateLitMaterial(new Color(0.75f, 0.18f, 0.18f));      // Tavern awning red
        Material canopyGoldMat = CreateLitMaterial(new Color(0.85f, 0.65f, 0.18f));     // Market gold
        Material metalMat = CreateLitMaterial(new Color(0.18f, 0.18f, 0.20f));          // Wrought iron
        Material windowGlowMat = CreateUnlitMaterial(new Color(1f, 0.88f, 0.45f));      // Candlelight interior
        Material waterMat = CreateUnlitMaterial(new Color(0.20f, 0.50f, 0.65f));        // Well water
        Material gridMat = CreateUnlitMaterial(new Color(0.12f, 0.15f, 0.18f));         // Backstage dev grid
        Material logMat = CreateUnlitMaterial(new Color(0.1f, 1f, 0.6f));               // Crash log green
        Material magentaMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MAT_Magenta_Glitch.mat");
        if (magentaMat == null)
        {
            magentaMat = CreateUnlitMaterial(Color.magenta);
        }

        // 3. Village Square Ground (18m wide x 22m long)
        GameObject square = GameObject.CreatePrimitive(PrimitiveType.Cube);
        square.name = "VillageSquare_Ground";
        square.transform.SetParent(levelRoot.transform);
        square.transform.position = new Vector3(0f, -0.25f, 10f);
        square.transform.localScale = new Vector3(18f, 0.5f, 22f);
        square.GetComponent<Renderer>().sharedMaterial = cobbleFloorMat;

        // 4. Perimeter Boundary Walls
        CreateWall("Perimeter_Wall_West", levelRoot.transform, new Vector3(-9.25f, 2.5f, 10f), new Vector3(0.5f, 5f, 22f), wallStoneMat);
        CreateWall("Perimeter_Wall_East", levelRoot.transform, new Vector3(9.25f, 2.5f, 10f), new Vector3(0.5f, 5f, 22f), wallStoneMat);
        CreateWall("Perimeter_Wall_South", levelRoot.transform, new Vector3(0f, 2.5f, -1.25f), new Vector3(19f, 5f, 0.5f), wallStoneMat);

        // 5. South Town Gate (Entrance Archway)
        CreateWall("TownGate_Tower_Left", levelRoot.transform, new Vector3(-3.5f, 3f, -1.1f), new Vector3(2f, 6f, 2f), wallStoneMat);
        CreateWall("TownGate_Tower_Right", levelRoot.transform, new Vector3(3.5f, 3f, -1.1f), new Vector3(2f, 6f, 2f), wallStoneMat);
        CreateWall("TownGate_Arch_Top", levelRoot.transform, new Vector3(0f, 5.2f, -1.1f), new Vector3(5f, 1.6f, 1.8f), wallStoneMat);
        CreateWall("TownGate_Portcullis", levelRoot.transform, new Vector3(0f, 2f, -1.1f), new Vector3(4.8f, 4f, 0.3f), timberDarkMat);

        // 6. West Side: Medieval Houses & The Tavern
        CreateBuildingFacade(levelRoot.transform, "House_West_01", new Vector3(-6.5f, 0f, 2f), new Vector3(4.5f, 4.5f, 4f), 1.8f, wallStoneMat, timberDarkMat, roofSlateMat, windowGlowMat);

        // The Tavern ("Null Pointer Inn")
        GameObject tavernObj = new GameObject("NullPointer_Tavern");
        tavernObj.transform.SetParent(levelRoot.transform);
        tavernObj.transform.position = new Vector3(-6.2f, 0f, 8.5f);

        // Tavern Main Body
        GameObject tavernBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tavernBody.name = "Tavern_Backroom";
        tavernBody.transform.SetParent(tavernObj.transform, false);
        tavernBody.transform.localPosition = new Vector3(0f, 2.25f, 1.5f);
        tavernBody.transform.localScale = new Vector3(5f, 4.5f, 4f);
        tavernBody.GetComponent<Renderer>().sharedMaterial = wallStoneMat;

        // Tavern Porch Canopy
        GameObject tavernCanopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tavernCanopy.name = "Tavern_PorchCanopy";
        tavernCanopy.transform.SetParent(tavernObj.transform, false);
        tavernCanopy.transform.localPosition = new Vector3(1f, 3.4f, -1.2f);
        tavernCanopy.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
        tavernCanopy.transform.localScale = new Vector3(4.2f, 0.2f, 3f);
        tavernCanopy.GetComponent<Renderer>().sharedMaterial = canopyRedMat;

        // Porch Timber Pillars
        Vector3[] tavernPillars = { new Vector3(-0.8f, 1.6f, -2.4f), new Vector3(2.8f, 1.6f, -2.4f) };
        foreach (var p in tavernPillars)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.transform.SetParent(tavernObj.transform, false);
            pillar.transform.localPosition = p;
            pillar.transform.localScale = new Vector3(0.2f, 3.2f, 0.2f);
            pillar.GetComponent<Renderer>().sharedMaterial = timberDarkMat;
        }

        // Tavern Serving Counter
        GameObject tavernCounter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tavernCounter.name = "Tavern_Counter";
        tavernCounter.transform.SetParent(tavernObj.transform, false);
        tavernCounter.transform.localPosition = new Vector3(1f, 0.6f, -1.8f);
        tavernCounter.transform.localScale = new Vector3(3.6f, 1.2f, 0.9f);
        tavernCounter.GetComponent<Renderer>().sharedMaterial = timberLightMat;

        // Stylized 3D Barkeep behind counter
        GameObject barkeep = CreateBarkeepCharacter(tavernObj.transform, new Vector3(1f, 0f, -0.6f));

        // Decorative Barrels at Tavern
        CreateBarrel(tavernObj.transform, "Tavern_Barrel_A", new Vector3(-0.5f, 0.6f, -1.8f), timberDarkMat);
        CreateBarrel(tavernObj.transform, "Tavern_Barrel_B", new Vector3(2.6f, 0.6f, -1.8f), timberDarkMat);

        // House West 02 (North end of square)
        CreateBuildingFacade(levelRoot.transform, "House_West_02", new Vector3(-6.5f, 0f, 16f), new Vector3(4.5f, 5f, 5f), 2f, wallStoneMat, timberDarkMat, roofTerracottaMat, windowGlowMat);

        // 7. East Side: Market Stalls & Houses
        CreateBuildingFacade(levelRoot.transform, "House_East_01", new Vector3(6.5f, 0f, 2f), new Vector3(4.5f, 4.5f, 4f), 1.8f, wallStoneMat, timberDarkMat, roofTerracottaMat, windowGlowMat);
        CreateMarketStall(levelRoot.transform, "MarketStall_Produce", new Vector3(5f, 0f, 7.5f), -90f, timberLightMat, canopyRedMat, timberDarkMat);
        CreateMarketStall(levelRoot.transform, "MarketStall_General", new Vector3(5f, 0f, 12.5f), -90f, timberLightMat, canopyGoldMat, timberDarkMat);
        CreateBuildingFacade(levelRoot.transform, "House_East_02", new Vector3(6.5f, 0f, 17f), new Vector3(4.5f, 5.2f, 4.5f), 2f, wallStoneMat, timberDarkMat, roofSlateMat, windowGlowMat);

        // 8. Central Village Square: The Well & Street Lanterns
        CreateVillageWell(levelRoot.transform, new Vector3(0f, 0f, 9.5f), wallStoneMat, timberDarkMat, roofTerracottaMat, waterMat);

        CreateStreetLantern(levelRoot.transform, new Vector3(-3.5f, 0f, 3.5f), timberDarkMat, metalMat, windowGlowMat);
        CreateStreetLantern(levelRoot.transform, new Vector3(3.5f, 0f, 3.5f), timberDarkMat, metalMat, windowGlowMat);
        CreateStreetLantern(levelRoot.transform, new Vector3(-3.5f, 0f, 16f), timberDarkMat, metalMat, windowGlowMat);
        CreateStreetLantern(levelRoot.transform, new Vector3(3.5f, 0f, 16f), timberDarkMat, metalMat, windowGlowMat);

        // 9. PUZZLE 1: High Wooden Barricade (2.2m high — requires crate stacking!)
        GameObject fence = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fence.name = "Fence_Blockade_Requires_Crates";
        fence.transform.SetParent(levelRoot.transform);
        fence.transform.position = new Vector3(0f, 1.1f, 21.25f);
        fence.transform.localScale = new Vector3(6f, 2.2f, 0.4f);
        fence.GetComponent<Renderer>().sharedMaterial = timberDarkMat;

        // Objective Trigger on approach to the fence
        GameObject trigFenceObj = new GameObject("ObjectiveTrigger_FenceApproach");
        trigFenceObj.transform.SetParent(levelRoot.transform);
        trigFenceObj.transform.position = new Vector3(0f, 1f, 16f);
        BoxCollider tfc = trigFenceObj.AddComponent<BoxCollider>();
        tfc.isTrigger = true;
        tfc.size = new Vector3(14f, 3f, 3f);
        ObjectiveTrigger otFence = trigFenceObj.AddComponent<ObjectiveTrigger>();
        otFence.SetText("EXPLOIT REQUIRED: High barricade ahead. Press [E] to duplicate physics crates and stack them to jump over.");

        // Barricade Cross Braces
        GameObject brace1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brace1.transform.SetParent(fence.transform, false);
        brace1.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
        brace1.transform.localScale = new Vector3(1.15f, 0.15f, 1.2f);
        brace1.GetComponent<Renderer>().sharedMaterial = timberLightMat;

        GameObject brace2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brace2.transform.SetParent(fence.transform, false);
        brace2.transform.localRotation = Quaternion.Euler(0f, 0f, -32f);
        brace2.transform.localScale = new Vector3(1.15f, 0.15f, 1.2f);
        brace2.GetComponent<Renderer>().sharedMaterial = timberLightMat;

        // Stone flanking walls leaving only the 6m gap at the fence
        CreateWall("NorthWall_Left", levelRoot.transform, new Vector3(-6.25f, 2.5f, 21.25f), new Vector3(6.5f, 5f, 0.6f), wallStoneMat);
        CreateWall("NorthWall_Right", levelRoot.transform, new Vector3(6.25f, 2.5f, 21.25f), new Vector3(6.5f, 5f, 0.6f), wallStoneMat);

        // 10. Checkpoint 1 (Just past the fence at Z = 23.5)
        GameObject cpFence = new GameObject("Checkpoint_PostFence");
        cpFence.transform.SetParent(levelRoot.transform);
        cpFence.transform.position = new Vector3(0f, 0.5f, 23.5f);
        BoxCollider cpCol = cpFence.AddComponent<BoxCollider>();
        cpCol.isTrigger = true;
        cpCol.size = new Vector3(5f, 2f, 2f);
        cpFence.AddComponent<Checkpoint>();
        ObjectiveTrigger otPastFence = cpFence.AddComponent<ObjectiveTrigger>();
        otPastFence.SetText("INVESTIGATION: Checkpoint saved. Investigate the collision anomaly in the north alley.");

        // 11. PUZZLE 2: The North Alley behind the fence (6m wide x 14m long)
        GameObject alleyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        alleyFloor.name = "Alley_Floor";
        alleyFloor.transform.SetParent(levelRoot.transform);
        alleyFloor.transform.position = new Vector3(0f, -0.25f, 28.5f);
        alleyFloor.transform.localScale = new Vector3(6f, 0.5f, 14.5f);
        alleyFloor.GetComponent<Renderer>().sharedMaterial = cobbleFloorMat;

        CreateWall("Alley_Wall_West", levelRoot.transform, new Vector3(-3.25f, 2.5f, 28.5f), new Vector3(0.5f, 5f, 14.5f), wallStoneMat);
        CreateWall("Alley_Wall_East", levelRoot.transform, new Vector3(3.25f, 2.5f, 28.5f), new Vector3(0.5f, 5f, 14.5f), wallStoneMat);

        // Overhead timber crossbeams in the alley for atmospheric depth
        for (float z = 24f; z <= 33f; z += 3.5f)
        {
            GameObject crossBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossBeam.name = "Alley_Overhead_Beam";
            crossBeam.transform.SetParent(levelRoot.transform);
            crossBeam.transform.position = new Vector3(0f, 4.2f, z);
            crossBeam.transform.localScale = new Vector3(6.5f, 0.25f, 0.25f);
            crossBeam.GetComponent<Renderer>().sharedMaterial = timberDarkMat;
        }

        // 12. CRASH LOG COLLECTIBLE (Floating in the North Alley)
        GameObject crashLog = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crashLog.name = "CrashLog_01_Collectible";
        crashLog.transform.SetParent(levelRoot.transform);
        crashLog.transform.position = new Vector3(0f, 1.2f, 28f);
        crashLog.transform.localScale = new Vector3(0.45f, 0.6f, 0.1f);
        crashLog.transform.rotation = Quaternion.Euler(0f, 25f, 10f);
        crashLog.GetComponent<Renderer>().sharedMaterial = logMat;

        BoxCollider clCol = crashLog.GetComponent<BoxCollider>();
        clCol.isTrigger = true;
        clCol.size = new Vector3(4f, 4f, 4f);
        crashLog.AddComponent<CrashLogCollectible>();

        // 13. GLITCH SEAM WALL (Dead end with clipping exploit)
        GameObject glitchWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glitchWall.name = "GlitchWall_Seam_Exit";
        glitchWall.transform.SetParent(levelRoot.transform);
        glitchWall.transform.position = new Vector3(0f, 2.5f, 35.25f);
        glitchWall.transform.localScale = new Vector3(6f, 5f, 0.5f);
        glitchWall.GetComponent<Renderer>().sharedMaterial = wallStoneMat;

        // Visual magenta glitch seam indicator
        GameObject seamIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seamIndicator.name = "Magenta_Seam_Visual";
        seamIndicator.transform.SetParent(glitchWall.transform, false);
        seamIndicator.transform.localPosition = new Vector3(0f, 0f, -0.26f);
        seamIndicator.transform.localScale = new Vector3(0.15f, 1f, 0.05f);
        seamIndicator.GetComponent<Renderer>().sharedMaterial = magentaMat;

        // Secondary trigger collider for clipping
        BoxCollider trigCol = glitchWall.AddComponent<BoxCollider>();
        trigCol.isTrigger = true;
        trigCol.size = new Vector3(1f, 1f, 3.5f);

        ClippingTrigger clip = glitchWall.AddComponent<ClippingTrigger>();
        if (magentaMat != null)
        {
            SerializedObject so = new SerializedObject(clip);
            so.FindProperty("_glitchMaterial").objectReferenceValue = magentaMat;
            so.FindProperty("_requireKeyPress").boolValue = false;
            so.ApplyModifiedProperties();
        }

        // 14. OUT OF BOUNDS / BACKSTAGE LEDGE (Beyond the glitch wall)
        GameObject backstage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backstage.name = "Backstage_Ledge (Out Of Bounds)";
        backstage.transform.SetParent(levelRoot.transform);
        backstage.transform.position = new Vector3(0f, -0.25f, 42f);
        backstage.transform.localScale = new Vector3(10f, 0.5f, 12f);
        backstage.GetComponent<Renderer>().sharedMaterial = gridMat;

        // Sector Exit Trigger on the backstage platform
        GameObject exitTrigger = new GameObject("Sector01_Complete_Trigger");
        exitTrigger.transform.SetParent(levelRoot.transform);
        exitTrigger.transform.position = new Vector3(0f, 1.5f, 42f);
        exitTrigger.transform.localScale = Vector3.one;

        BoxCollider exitCol = exitTrigger.AddComponent<BoxCollider>();
        exitCol.isTrigger = true;
        exitCol.size = new Vector3(8f, 3f, 8f);
        exitTrigger.AddComponent<SectorCompleteTrigger>();

        // Distant Monolith Garbage Collector prop in the digital sky
        GameObject distantMonolith = GameObject.CreatePrimitive(PrimitiveType.Cube);
        distantMonolith.name = "Distant_GarbageCollector_Prop";
        distantMonolith.transform.SetParent(levelRoot.transform);
        distantMonolith.transform.position = new Vector3(15f, 8f, 55f);
        distantMonolith.transform.localScale = new Vector3(4f, 12f, 4f);
        distantMonolith.GetComponent<Renderer>().sharedMaterial = CreateLitMaterial(new Color(0.08f, 0.1f, 0.12f));

        // 15. Setup Warm Late-Afternoon Sun
        Light sun = RenderSettings.sun;
        if (sun == null)
        {
            GameObject sunObj = GameObject.Find("Directional Light");
            if (sunObj != null) sun = sunObj.GetComponent<Light>();
        }
        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            sun.color = new Color(1f, 0.94f, 0.84f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
        }

        // 16. Wire URP Post-Processing Volumes
        SetupAtmosphere();

        // 17. Position Player at village start and dress with 3D Merchant Model
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1f, 2f);
            player.transform.rotation = Quaternion.identity;
            AttachMerchantModelToPlayer(player);
        }

        Selection.activeGameObject = levelRoot;
        Debug.Log("<color=green><b>[N.P.C.] Level 01: The Broken Village fully generated with medieval scenery and lighting!</b></color>");
    }

    private static void CreateBuildingFacade(Transform parent, string name, Vector3 pos, Vector3 size, float roofHeight, Material wallMat, Material timberMat, Material roofMat, Material windowMat)
    {
        GameObject building = new GameObject(name);
        building.transform.SetParent(parent);
        building.transform.position = pos;

        // Main plaster body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(building.transform, false);
        body.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
        body.transform.localScale = size;
        body.GetComponent<Renderer>().sharedMaterial = wallMat;

        // 4 Timber corner beams
        float hw = size.x * 0.5f;
        float hd = size.z * 0.5f;
        float beamW = 0.35f;

        Vector3[] corners = new Vector3[]
        {
            new Vector3(-hw, size.y * 0.5f, -hd),
            new Vector3(hw, size.y * 0.5f, -hd),
            new Vector3(-hw, size.y * 0.5f, hd),
            new Vector3(hw, size.y * 0.5f, hd)
        };

        foreach (var c in corners)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Beam_Corner";
            beam.transform.SetParent(building.transform, false);
            beam.transform.localPosition = c;
            beam.transform.localScale = new Vector3(beamW, size.y, beamW);
            beam.GetComponent<Renderer>().sharedMaterial = timberMat;
        }

        // Horizontal mid beam across front
        GameObject midBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        midBeam.name = "Beam_Mid";
        midBeam.transform.SetParent(building.transform, false);
        midBeam.transform.localPosition = new Vector3(0f, size.y * 0.5f, -hd - 0.05f);
        midBeam.transform.localScale = new Vector3(size.x, 0.3f, 0.2f);
        midBeam.GetComponent<Renderer>().sharedMaterial = timberMat;

        // Pitched Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(building.transform, false);
        roof.transform.localPosition = new Vector3(0f, size.y + roofHeight * 0.35f, 0f);
        roof.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        float rScale = Mathf.Max(size.x, size.z) * 0.72f;
        roof.transform.localScale = new Vector3(rScale, rScale, size.z + 0.6f);
        roof.GetComponent<Renderer>().sharedMaterial = roofMat;

        // Candlelit Window
        GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.name = "Window_Glow";
        win.transform.SetParent(building.transform, false);
        win.transform.localPosition = new Vector3(0f, size.y * 0.72f, -hd - 0.08f);
        win.transform.localScale = new Vector3(0.9f, 0.9f, 0.1f);
        win.GetComponent<Renderer>().sharedMaterial = windowMat;
    }

    private static void CreateMarketStall(Transform parent, string name, Vector3 pos, float yaw, Material timberMat, Material canopyMat, Material propMat)
    {
        GameObject stall = new GameObject(name);
        stall.transform.SetParent(parent);
        stall.transform.position = pos;
        stall.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Counter bench
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Bench";
        table.transform.SetParent(stall.transform, false);
        table.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        table.transform.localScale = new Vector3(2.4f, 0.9f, 1.2f);
        table.GetComponent<Renderer>().sharedMaterial = timberMat;

        // 4 Canopy posts
        float px = 1.1f;
        float pz = 0.55f;
        Vector3[] posts = { new Vector3(-px, 1.3f, -pz), new Vector3(px, 1.3f, -pz), new Vector3(-px, 1.5f, pz), new Vector3(px, 1.5f, pz) };
        foreach (var p in posts)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.transform.SetParent(stall.transform, false);
            post.transform.localPosition = p;
            post.transform.localScale = new Vector3(0.12f, 2.4f, 0.12f);
            post.GetComponent<Renderer>().sharedMaterial = timberMat;
        }

        // Tilted Fabric Canopy
        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canopy.name = "Canopy";
        canopy.transform.SetParent(stall.transform, false);
        canopy.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        canopy.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        canopy.transform.localScale = new Vector3(2.6f, 0.12f, 1.6f);
        canopy.GetComponent<Renderer>().sharedMaterial = canopyMat;

        // Wares / Goods
        GameObject box1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box1.transform.SetParent(stall.transform, false);
        box1.transform.localPosition = new Vector3(-0.6f, 1.05f, 0f);
        box1.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
        box1.GetComponent<Renderer>().sharedMaterial = propMat;

        GameObject box2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        box2.transform.SetParent(stall.transform, false);
        box2.transform.localPosition = new Vector3(0.5f, 1.05f, 0f);
        box2.transform.localScale = new Vector3(0.4f, 0.25f, 0.4f);
        box2.GetComponent<Renderer>().sharedMaterial = propMat;
    }

    private static void CreateStreetLantern(Transform parent, Vector3 pos, Material timberMat, Material metalMat, Material glowMat)
    {
        GameObject lamp = new GameObject("StreetLantern_Post");
        lamp.transform.SetParent(parent);
        lamp.transform.position = pos;

        // Timber post
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(lamp.transform, false);
        pole.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        pole.transform.localScale = new Vector3(0.2f, 1.6f, 0.2f);
        pole.GetComponent<Renderer>().sharedMaterial = timberMat;

        // Bracket Arm
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Bracket";
        arm.transform.SetParent(lamp.transform, false);
        arm.transform.localPosition = new Vector3(0.35f, 3f, 0f);
        arm.transform.localScale = new Vector3(0.7f, 0.12f, 0.12f);
        arm.GetComponent<Renderer>().sharedMaterial = metalMat;

        // Lantern head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Lantern_Housing";
        head.transform.SetParent(lamp.transform, false);
        head.transform.localPosition = new Vector3(0.65f, 2.8f, 0f);
        head.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
        head.GetComponent<Renderer>().sharedMaterial = glowMat;

        // Point Light
        GameObject lightObj = new GameObject("Lantern_PointLight");
        lightObj.transform.SetParent(head.transform, false);
        Light pl = lightObj.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.color = new Color(1f, 0.76f, 0.42f);
        pl.intensity = 1.6f;
        pl.range = 7f;
    }

    private static void CreateVillageWell(Transform parent, Vector3 pos, Material stoneMat, Material timberMat, Material roofMat, Material waterMat)
    {
        GameObject well = new GameObject("Village_Well");
        well.transform.SetParent(parent);
        well.transform.position = pos;

        // Stone base
        GameObject baseRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseRim.name = "Stone_Rim";
        baseRim.transform.SetParent(well.transform, false);
        baseRim.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        baseRim.transform.localScale = new Vector3(2.4f, 0.45f, 2.4f);
        baseRim.GetComponent<Renderer>().sharedMaterial = stoneMat;

        // Water surface
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.name = "Water_Surface";
        water.transform.SetParent(well.transform, false);
        water.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        water.transform.localScale = new Vector3(1.9f, 0.05f, 1.9f);
        water.GetComponent<Renderer>().sharedMaterial = waterMat;

        // 2 Side posts
        GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postL.transform.SetParent(well.transform, false);
        postL.transform.localPosition = new Vector3(-1f, 1.5f, 0f);
        postL.transform.localScale = new Vector3(0.2f, 1.8f, 0.2f);
        postL.GetComponent<Renderer>().sharedMaterial = timberMat;

        GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postR.transform.SetParent(well.transform, false);
        postR.transform.localPosition = new Vector3(1f, 1.5f, 0f);
        postR.transform.localScale = new Vector3(0.2f, 1.8f, 0.2f);
        postR.GetComponent<Renderer>().sharedMaterial = timberMat;

        // Crossbeam
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.transform.SetParent(well.transform, false);
        beam.transform.localPosition = new Vector3(0f, 2.3f, 0f);
        beam.transform.localScale = new Vector3(2.3f, 0.2f, 0.2f);
        beam.GetComponent<Renderer>().sharedMaterial = timberMat;

        // Well roof
        GameObject wRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wRoof.transform.SetParent(well.transform, false);
        wRoof.transform.localPosition = new Vector3(0f, 2.7f, 0f);
        wRoof.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        wRoof.transform.localScale = new Vector3(1.3f, 1.3f, 1.8f);
        wRoof.GetComponent<Renderer>().sharedMaterial = roofMat;

        // Bucket
        GameObject bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bucket.name = "Bucket";
        bucket.transform.SetParent(well.transform, false);
        bucket.transform.localPosition = new Vector3(0.7f, 0.95f, 0.3f);
        bucket.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
        bucket.GetComponent<Renderer>().sharedMaterial = timberMat;
    }

    private static void CreateBarrel(Transform parent, string name, Vector3 pos, Material timberMat)
    {
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = name;
        barrel.transform.SetParent(parent);
        barrel.transform.position = pos;
        barrel.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
        barrel.GetComponent<Renderer>().sharedMaterial = timberMat;
    }

    private static Material CreateUnlitMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        Material mat = new Material(shader != null ? shader : Shader.Find("Hidden/InternalErrorShader"));
        mat.color = color;
        return mat;
    }

    private static Material CreateLitMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        Material mat = new Material(shader != null ? shader : Shader.Find("Hidden/InternalErrorShader"));
        mat.color = color;
        return mat;
    }

    private static void CreateWall(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = mat;
    }

    [MenuItem("N.P.C./Setup Post-Processing & Glitch Atmosphere")]
    public static void SetupAtmosphere()
    {
        // 1. GLOBAL VOLUME — Village Fantasy Façade
        GameObject globalVolObj = GameObject.Find("GlobalVolume_VillageFantasy");
        if (globalVolObj == null)
        {
            globalVolObj = new GameObject("GlobalVolume_VillageFantasy");
            Undo.RegisterCreatedObjectUndo(globalVolObj, "Create Global Volume");
        }

        Volume globalVol = globalVolObj.GetComponent<Volume>();
        if (globalVol == null) globalVol = globalVolObj.AddComponent<Volume>();
        globalVol.isGlobal = true;
        globalVol.priority = 0;

        string globalPath = "Assets/Settings/VillageFantasy_Profile.asset";
        VolumeProfile globalProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(globalPath);
        if (globalProfile == null)
        {
            globalProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(globalProfile, globalPath);
        }

        // Configure Global Effects (Warm, fairy-tale MMORPG glow)
        if (!globalProfile.Has<Bloom>()) globalProfile.Add<Bloom>();
        if (globalProfile.TryGet(out Bloom bloom))
        {
            bloom.threshold.Override(0.85f);
            bloom.intensity.Override(0.45f);
        }

        if (!globalProfile.Has<Tonemapping>()) globalProfile.Add<Tonemapping>();
        if (globalProfile.TryGet(out Tonemapping tone))
        {
            tone.mode.Override(TonemappingMode.ACES);
        }

        if (!globalProfile.Has<ColorAdjustments>()) globalProfile.Add<ColorAdjustments>();
        if (globalProfile.TryGet(out ColorAdjustments colorAdj))
        {
            colorAdj.postExposure.Override(0.15f);
            colorAdj.contrast.Override(12f);
            colorAdj.colorFilter.Override(new Color(1f, 0.96f, 0.88f)); // Warm golden sunlight
        }

        if (!globalProfile.Has<ChromaticAberration>()) globalProfile.Add<ChromaticAberration>();
        if (globalProfile.TryGet(out ChromaticAberration ca))
        {
            ca.intensity.Override(0.08f);
        }

        if (!globalProfile.Has<LensDistortion>()) globalProfile.Add<LensDistortion>();
        if (globalProfile.TryGet(out LensDistortion ld))
        {
            ld.intensity.Override(0f);
        }

        globalVol.profile = globalProfile;

        // Add ScreenGlitchEffect component
        ScreenGlitchEffect sge = globalVolObj.GetComponent<ScreenGlitchEffect>();
        if (sge == null) sge = globalVolObj.AddComponent<ScreenGlitchEffect>();

        EditorUtility.SetDirty(globalProfile);

        // 2. LOCAL VOLUME — Out of Bounds / Backstage Glitch Atmosphere
        GameObject localVolObj = GameObject.Find("LocalVolume_BackstageGlitch");
        if (localVolObj == null)
        {
            localVolObj = new GameObject("LocalVolume_BackstageGlitch");
            Undo.RegisterCreatedObjectUndo(localVolObj, "Create Local Volume");
        }

        localVolObj.transform.position = new Vector3(0f, 2f, 42f);

        BoxCollider volCol = localVolObj.GetComponent<BoxCollider>();
        if (volCol == null) volCol = localVolObj.AddComponent<BoxCollider>();
        volCol.isTrigger = true;
        volCol.size = new Vector3(16f, 8f, 16f); // Covers entire backstage ledge

        Volume localVol = localVolObj.GetComponent<Volume>();
        if (localVol == null) localVol = localVolObj.AddComponent<Volume>();
        localVol.isGlobal = false;
        localVol.priority = 10; // Overrides global volume inside the trigger
        localVol.blendDistance = 3.5f; // Smooth transition as player clips through wall!

        string localPath = "Assets/Settings/BackstageGlitch_Profile.asset";
        VolumeProfile localProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(localPath);
        if (localProfile == null)
        {
            localProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(localProfile, localPath);
        }

        // Configure Backstage Effects (Cold, cyan wireframe, CRT monitor distortion, film grain)
        if (!localProfile.Has<ColorAdjustments>()) localProfile.Add<ColorAdjustments>();
        if (localProfile.TryGet(out ColorAdjustments lColor))
        {
            lColor.contrast.Override(40f);
            lColor.saturation.Override(-25f);
            lColor.colorFilter.Override(new Color(0.48f, 0.9f, 1f)); // Cold cyan terminal glow
        }

        if (!localProfile.Has<ChromaticAberration>()) localProfile.Add<ChromaticAberration>();
        if (localProfile.TryGet(out ChromaticAberration lCa))
        {
            lCa.intensity.Override(0.9f); // Intense RGB channel split
        }

        if (!localProfile.Has<FilmGrain>()) localProfile.Add<FilmGrain>();
        if (localProfile.TryGet(out FilmGrain fg))
        {
            fg.type.Override(FilmGrainLookup.Large01);
            fg.intensity.Override(0.85f); // CRT monitor static noise
        }

        if (!localProfile.Has<LensDistortion>()) localProfile.Add<LensDistortion>();
        if (localProfile.TryGet(out LensDistortion lLd))
        {
            lLd.intensity.Override(-0.2f); // CRT glass curved lens
        }

        if (!localProfile.Has<Vignette>()) localProfile.Add<Vignette>();
        if (localProfile.TryGet(out Vignette vig))
        {
            vig.intensity.Override(0.45f);
            vig.smoothness.Override(0.4f);
            vig.color.Override(new Color(0.02f, 0.05f, 0.1f));
        }

        localVol.profile = localProfile;
        EditorUtility.SetDirty(localProfile);

        AssetDatabase.SaveAssets();

        Debug.Log("<color=cyan><b>[N.P.C.] URP Fantasy & Glitch Post-Processing Volumes successfully created!</b></color>");
    }

    #region Character 3D Model Generation

    private static GameObject CreateVisualPart(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localScale = localScale;

        // Strip primitive colliders so they do not interfere with CharacterController
        Collider col = part.GetComponent<Collider>();
        if (col != null)
        {
            Object.DestroyImmediate(col);
        }

        if (mat != null)
        {
            part.GetComponent<Renderer>().sharedMaterial = mat;
        }

        return part;
    }

    public static void AttachMerchantModelToPlayer(GameObject player)
    {
        if (player == null) return;

        // 1. Hide/remove old capsule mesh on Player root
        MeshRenderer playerRend = player.GetComponent<MeshRenderer>();
        if (playerRend != null)
        {
            playerRend.enabled = false;
        }

        // Remove old Visuals if already attached
        Transform oldVisuals = player.transform.Find("Visuals_Merchant");
        if (oldVisuals != null)
        {
            Undo.DestroyObjectImmediate(oldVisuals.gameObject);
        }

        // Palette materials
        Material skinMat = CreateLitMaterial(new Color(0.95f, 0.78f, 0.65f));      // Skin tone
        Material tunicMat = CreateLitMaterial(new Color(0.22f, 0.44f, 0.32f));     // Merchant tunic green
        Material trouserMat = CreateLitMaterial(new Color(0.36f, 0.26f, 0.18f));   // Earth brown pants
        Material bootMat = CreateLitMaterial(new Color(0.16f, 0.12f, 0.08f));      // Dark leather boots
        Material beltMat = CreateLitMaterial(new Color(0.30f, 0.18f, 0.10f));      // Leather belt & bag
        Material goldMat = CreateLitMaterial(new Color(0.85f, 0.70f, 0.20f));      // Gold buckle
        Material capMat = CreateLitMaterial(new Color(0.48f, 0.20f, 0.20f));       // Wine red merchant cap
        Material packMat = CreateLitMaterial(new Color(0.45f, 0.35f, 0.22f));      // Traveler backpack canvas

        // Root Visuals (offset Y = -1 so feet align with CharacterController bottom)
        GameObject visuals = new GameObject("Visuals_Merchant");
        visuals.transform.SetParent(player.transform, false);
        visuals.transform.localPosition = new Vector3(0f, -1f, 0f);
        visuals.transform.localRotation = Quaternion.identity;

        // Hips (Pivot at Y = 0.95m)
        GameObject hips = new GameObject("Hips");
        hips.transform.SetParent(visuals.transform, false);
        hips.transform.localPosition = new Vector3(0f, 0.95f, 0f);

        // Torso / Chest (Y = 0.35 above hips)
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(hips.transform, false);
        torso.transform.localPosition = new Vector3(0f, 0.35f, 0f);

        CreateVisualPart(PrimitiveType.Cube, "Tunic_Body", torso.transform, Vector3.zero, new Vector3(0.55f, 0.55f, 0.36f), tunicMat);
        CreateVisualPart(PrimitiveType.Cube, "Belt", torso.transform, new Vector3(0f, -0.22f, 0f), new Vector3(0.58f, 0.12f, 0.38f), beltMat);
        CreateVisualPart(PrimitiveType.Cube, "Buckle", torso.transform, new Vector3(0f, -0.22f, 0.20f), new Vector3(0.14f, 0.10f, 0.04f), goldMat);
        CreateVisualPart(PrimitiveType.Cube, "Satchel", torso.transform, new Vector3(0.32f, -0.25f, 0.05f), new Vector3(0.20f, 0.25f, 0.16f), beltMat);
        CreateVisualPart(PrimitiveType.Cube, "Backpack", torso.transform, new Vector3(0f, 0.02f, -0.26f), new Vector3(0.42f, 0.46f, 0.22f), packMat);

        // Head (Pivot at Y = 0.45 above torso)
        GameObject head = new GameObject("Head");
        head.transform.SetParent(torso.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.45f, 0f);

        CreateVisualPart(PrimitiveType.Cube, "Head_Mesh", head.transform, Vector3.zero, new Vector3(0.36f, 0.38f, 0.36f), skinMat);

        GameObject cap = CreateVisualPart(PrimitiveType.Cube, "Merchant_Cap", head.transform, new Vector3(0f, 0.21f, -0.02f), new Vector3(0.42f, 0.16f, 0.44f), capMat);
        cap.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);

        CreateVisualPart(PrimitiveType.Cube, "Eye_L", head.transform, new Vector3(-0.09f, 0.02f, 0.19f), new Vector3(0.06f, 0.06f, 0.02f), bootMat);
        CreateVisualPart(PrimitiveType.Cube, "Eye_R", head.transform, new Vector3(0.09f, 0.02f, 0.19f), new Vector3(0.06f, 0.06f, 0.02f), bootMat);

        // Arms
        GameObject armL = new GameObject("Arm_L");
        armL.transform.SetParent(torso.transform, false);
        armL.transform.localPosition = new Vector3(-0.36f, 0.18f, 0f);
        CreateVisualPart(PrimitiveType.Cylinder, "Sleeve_L", armL.transform, new Vector3(0f, -0.22f, 0f), new Vector3(0.14f, 0.22f, 0.14f), tunicMat);
        CreateVisualPart(PrimitiveType.Sphere, "Hand_L", armL.transform, new Vector3(0f, -0.48f, 0f), new Vector3(0.14f, 0.14f, 0.14f), skinMat);

        GameObject armR = new GameObject("Arm_R");
        armR.transform.SetParent(torso.transform, false);
        armR.transform.localPosition = new Vector3(0.36f, 0.18f, 0f);
        CreateVisualPart(PrimitiveType.Cylinder, "Sleeve_R", armR.transform, new Vector3(0f, -0.22f, 0f), new Vector3(0.14f, 0.22f, 0.14f), tunicMat);
        CreateVisualPart(PrimitiveType.Sphere, "Hand_R", armR.transform, new Vector3(0f, -0.48f, 0f), new Vector3(0.14f, 0.14f, 0.14f), skinMat);

        // Legs
        GameObject legL = new GameObject("Leg_L");
        legL.transform.SetParent(hips.transform, false);
        legL.transform.localPosition = new Vector3(-0.16f, 0f, 0f);
        CreateVisualPart(PrimitiveType.Cylinder, "Thigh_L", legL.transform, new Vector3(0f, -0.35f, 0f), new Vector3(0.17f, 0.35f, 0.17f), trouserMat);
        CreateVisualPart(PrimitiveType.Cube, "Boot_L", legL.transform, new Vector3(0f, -0.80f, 0.06f), new Vector3(0.18f, 0.22f, 0.28f), bootMat);

        GameObject legR = new GameObject("Leg_R");
        legR.transform.SetParent(hips.transform, false);
        legR.transform.localPosition = new Vector3(0.16f, 0f, 0f);
        CreateVisualPart(PrimitiveType.Cylinder, "Thigh_R", legR.transform, new Vector3(0f, -0.35f, 0f), new Vector3(0.17f, 0.35f, 0.17f), trouserMat);
        CreateVisualPart(PrimitiveType.Cube, "Boot_R", legR.transform, new Vector3(0f, -0.80f, 0.06f), new Vector3(0.18f, 0.22f, 0.28f), bootMat);

        // Attach Procedural Animator
        PlayerModelAnimator anim = visuals.AddComponent<PlayerModelAnimator>();
        anim.SetupBones(hips.transform, torso.transform, legL.transform, legR.transform, armL.transform, armR.transform, head.transform);

        // Align CrateSpawnPoint forward
        Transform spawnPoint = player.transform.Find("CrateSpawnPoint");
        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("CrateSpawnPoint");
            sp.transform.SetParent(player.transform, false);
            sp.transform.localPosition = new Vector3(0f, 0.5f, 1.4f);
            spawnPoint = sp.transform;
        }

        ItemDuplication dup = player.GetComponent<ItemDuplication>();
        if (dup != null)
        {
            SerializedObject so = new SerializedObject(dup);
            so.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint;
            so.ApplyModifiedProperties();
        }

        Undo.RegisterCreatedObjectUndo(visuals, "Attach Merchant Model");
    }

    public static GameObject CreateBarkeepCharacter(Transform parent, Vector3 localPos)
    {
        GameObject barkeep = new GameObject("Barkeep_01.npc");
        if (parent != null)
        {
            barkeep.transform.SetParent(parent, false);
        }
        barkeep.transform.localPosition = localPos;

        SphereCollider sc = barkeep.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 2.5f;
        sc.center = new Vector3(0f, 1f, 0f);
        barkeep.AddComponent<PassiveNPC>();

        // Materials
        Material skinMat = CreateLitMaterial(new Color(0.95f, 0.78f, 0.65f));
        Material shirtMat = CreateLitMaterial(new Color(0.88f, 0.85f, 0.80f));    // Linen shirt
        Material apronMat = CreateLitMaterial(new Color(0.38f, 0.22f, 0.12f));    // Leather apron
        Material trouserMat = CreateLitMaterial(new Color(0.25f, 0.22f, 0.20f));  // Dark trousers
        Material woodMugMat = CreateLitMaterial(new Color(0.48f, 0.32f, 0.18f));  // Wooden tankard
        Material hairMat = CreateLitMaterial(new Color(0.20f, 0.15f, 0.12f));     // Dark hair/mustache

        // Legs
        CreateVisualPart(PrimitiveType.Cylinder, "Leg_L", barkeep.transform, new Vector3(-0.16f, 0.45f, 0f), new Vector3(0.18f, 0.45f, 0.18f), trouserMat);
        CreateVisualPart(PrimitiveType.Cylinder, "Leg_R", barkeep.transform, new Vector3(0.16f, 0.45f, 0f), new Vector3(0.18f, 0.45f, 0.18f), trouserMat);

        // Torso (Pivot at Y = 1.15m)
        GameObject torso = new GameObject("Torso");
        torso.transform.SetParent(barkeep.transform, false);
        torso.transform.localPosition = new Vector3(0f, 1.15f, 0f);

        CreateVisualPart(PrimitiveType.Cube, "Shirt_Body", torso.transform, Vector3.zero, new Vector3(0.60f, 0.60f, 0.38f), shirtMat);
        CreateVisualPart(PrimitiveType.Cube, "Leather_Apron", torso.transform, new Vector3(0f, -0.05f, 0.10f), new Vector3(0.52f, 0.65f, 0.22f), apronMat);

        // Head (Pivot at Y = 0.45m above torso)
        GameObject head = new GameObject("Head");
        head.transform.SetParent(torso.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.45f, 0f);

        CreateVisualPart(PrimitiveType.Cube, "Head_Mesh", head.transform, Vector3.zero, new Vector3(0.38f, 0.40f, 0.38f), skinMat);
        CreateVisualPart(PrimitiveType.Cube, "Hair", head.transform, new Vector3(0f, 0.20f, -0.05f), new Vector3(0.40f, 0.14f, 0.42f), hairMat);
        CreateVisualPart(PrimitiveType.Cube, "Mustache", head.transform, new Vector3(0f, -0.08f, 0.20f), new Vector3(0.22f, 0.08f, 0.04f), hairMat);

        // Left Arm (holding wooden tankard/mug)
        GameObject armL = new GameObject("Arm_L");
        armL.transform.SetParent(torso.transform, false);
        armL.transform.localPosition = new Vector3(-0.38f, 0.20f, 0f);
        CreateVisualPart(PrimitiveType.Cylinder, "Arm_Mesh_L", armL.transform, new Vector3(0f, -0.22f, 0f), new Vector3(0.14f, 0.22f, 0.14f), shirtMat);
        CreateVisualPart(PrimitiveType.Sphere, "Hand_L", armL.transform, new Vector3(0f, -0.46f, 0f), new Vector3(0.13f, 0.13f, 0.13f), skinMat);

        // Tankard / Mug in left hand
        CreateVisualPart(PrimitiveType.Cylinder, "Wooden_Tankard", armL.transform, new Vector3(0f, -0.46f, 0.15f), new Vector3(0.18f, 0.18f, 0.18f), woodMugMat);

        // Right Arm (wiping cloth)
        GameObject armR = new GameObject("Arm_R");
        armR.transform.SetParent(torso.transform, false);
        armR.transform.localPosition = new Vector3(0.38f, 0.20f, 0f);
        CreateVisualPart(PrimitiveType.Cylinder, "Arm_Mesh_R", armR.transform, new Vector3(0f, -0.22f, 0f), new Vector3(0.14f, 0.22f, 0.14f), shirtMat);
        CreateVisualPart(PrimitiveType.Sphere, "Hand_R", armR.transform, new Vector3(0f, -0.46f, 0f), new Vector3(0.13f, 0.13f, 0.13f), skinMat);

        // Cleaning Rag in right hand
        CreateVisualPart(PrimitiveType.Cube, "Cleaning_Cloth", armR.transform, new Vector3(0f, -0.48f, 0.10f), new Vector3(0.18f, 0.12f, 0.14f), shirtMat);

        // Idle Animator
        PassiveNPCAnimator anim = barkeep.AddComponent<PassiveNPCAnimator>();
        anim.SetupBones(torso.transform, armL.transform, armR.transform, head.transform);

        return barkeep;
    }

    #endregion
}
#endif

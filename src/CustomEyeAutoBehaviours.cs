using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Vs1Plugin {

	public class CustomEyeAutoBehaviours : MVRScript {

		const string pluginName = "Custom Eye Auto Behaviours";
		const string versionString = "0.1.0";

		static string about {
			get {
				string s = "<color=#d45583><size=35><b>About</b></size></color>\n"
					+ "\n"
					+ "<b>Custom Eye Auto Behaviors</b> is a replacement for VaM's Auto Behaviors \"Auto Blink\".\n"
					+ "\n"
					+ "This plugin was created because the blinking is not enough when the eyes of the Anime style character are big.\n"
					+ "\n"
					+ "With vs1 original blink implementation, you can increase the amount of blink morphs applied and select morphs.\n"
					+ "\n"
					+ "Select the button \"Set Example Values ​​For Anime Style\" to set sample parameter values ​​suitable for Anime style characters.\n"
					+ "\n"
					+ "<b>Notice</b>: If this plugin is enabled, Auto Behaviors \"Auto Blink\" and \"Auto Eyelid Morphs\" will be disabled";

				return s;
			}
		}

		// Blink --------------------------------

		protected class EyeBlink
		{

			float blinkStartTime;
			float blinkEndTime;
			float blinkDuration;

			public float blinkDurationBase = 0.15f;
			public float blinkDurationRandom = 0.1f;
			public float blinkInterval = 5f;
			public float blinkIntervalRandom = 4f;

			float blinkProgress = 0f;
			public float blinkRatio = 0f;

			void Blink()
			{
				blinkStartTime = Time.time;
				BlinkReady();
			}

			void BlinkSchedule()
			{
				blinkStartTime = Time.time + blinkInterval + UnityEngine.Random.Range(0, blinkIntervalRandom);
				BlinkReady();
			}

			void BlinkReady()
			{
				blinkProgress = 0f;
				blinkRatio = 0f;
				blinkDuration = blinkDurationBase + UnityEngine.Random.Range(0, blinkDurationRandom);
				blinkEndTime = blinkStartTime + blinkDuration;

				Update();
			}

			public void BlinkClear()
			{
				BlinkSchedule();
			}

			public void Update()
			{
				var time = Time.time;
				var blinkMorphRatioNew = blinkRatio;

				if (blinkEndTime < time)
				{
					BlinkSchedule();
				}
				else if (blinkStartTime <= time)
				{
					blinkProgress = (time - blinkStartTime) / blinkDuration;
					blinkMorphRatioNew = animationCurve.Evaluate(blinkProgress);
				}
				else
				{
					blinkProgress = 0;
					blinkMorphRatioNew = 0f;
				}

				blinkRatio = blinkMorphRatioNew;
			}
		}


		// --------------------------------

		private DAZBone[] bones;

		protected DAZBone lEyeDAZBone;
		protected Transform lEye;

		protected DAZBone rEyeDAZBone;
		protected Transform rEye;

		private GenerateDAZMorphsControlUI morphControl;

		// -------------------------------

		protected EyeBlink eyeBlink;
		
		// Morph Names -----------------------

		const string morphNoneName = "<None>";
		List<string> morphNames;

		void InitMorphNames()
		{
			morphNames = new List<string>();
			morphNames.Add(morphNoneName);

			DAZCharacterSelector characterSelector = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
			morphControl = characterSelector.morphsControlUI;
			if (morphControl != null)
			{
				morphControl.GetMorphDisplayNames().ForEach((name) =>
				{
					morphNames.Add(name);
				});
			}
		}

		// UI --------------

		UIDynamicButton btnSetDefault;
		UIDynamicButton btnSetExampleForAnimeStyle;

		JSONStorableBool enableCustomAutoBlink;
		UIDynamicToggle enableCustomAutoBlinkToggle;

		JSONStorableBool enableCustomAutoEyelidMorph;
		UIDynamicToggle enableCustomAutoEyelidMorphToggle;

		const string eyelidTopDownLeftMorphDefault = "Eyelids Top Down Left";
		const float eyelidTopDownLeftMorphMultiplyDefault = 1f;
		JSONStorableStringChooser eyelidTopDownLeftMorphChooser;
		UIDynamicPopup eyelidTopDownLeftMorphPopup;
		JSONStorableFloat eyelidTopDownLeftMorphFloatMax;
		UIDynamicSlider eyelidTopDownLeftMorphSlider;

		const string eyelidTopDownRightMorphDefault = "Eyelids Top Down Right";
		const float eyelidTopDownRightMorphMultiplyDefault = 1f;
		JSONStorableStringChooser eyelidTopDownRightMorphChooser;
		UIDynamicPopup eyelidTopDownRightMorphPopup;
		JSONStorableFloat eyelidTopDownRightMorphFloatMax;
		UIDynamicSlider eyelidTopDownRightMorphSlider;

		const string eyelidUpLeftMorphDefault = "Eyelids Top Up Left";
		const float eyelidUpLeftMorphMultiplyDefault = 0.5f;
		JSONStorableStringChooser eyelidTopUpLeftMorphChooser;
		UIDynamicPopup eyelidTopUpLeftMorphPopup;
		JSONStorableFloat eyelidTopUpLeftMorphFloatMax;
		UIDynamicSlider eyelidTopUpLeftMorphSlider;

		const string eyelidUpRightMorphDefault = "Eyelids Top Up Right";
		const float eyelidUpRightMorphMultiplyDefault = 0.5f;
		JSONStorableStringChooser eyelidTopUpRightMorphChooser;
		UIDynamicPopup eyelidTopUpRightMorphPopup;
		JSONStorableFloat eyelidTopUpRightMorphFloatMax;
		UIDynamicSlider eyelidTopUpRightMorphSlider;

		const string eyelidBottomUpLeftMorphDefault = "Eyelids Bottom Up Left";
		const float eyelidBottomUpLeftMorphMultiplyDefault = 0.25f;
		JSONStorableStringChooser eyelidBottomUpLeftMorphChooser;
		UIDynamicPopup eyelidBottomUpLeftMorphPopup;
		JSONStorableFloat eyelidBottomUpLeftMorphFloatMax;
		UIDynamicSlider eyelidBottomUpLeftMorphSlider;

		const string eyelidBottomUpRightMorphDefault = "Eyelids Bottom Up Right";
		const float eyelidBottomUpRightMorphMultiplyDefault = 0.25f;
		JSONStorableStringChooser eyelidBottomUpRightMorphChooser;
		UIDynamicPopup eyelidBottomUpRightMorphPopup;
		JSONStorableFloat eyelidBottomUpRightMorphFloatMax;
		UIDynamicSlider eyelidBottomUpRightMorphSlider;


		// Animation Curve -----------------------

		static AnimationCurve _animationCurve;

		static public AnimationCurve animationCurve
		{
			get
			{
				if (_animationCurve == null)
				{
					Keyframe[] keyframes = new Keyframe[3] {
						new Keyframe(0f, 0f, 2f, 2f, 0.3333333f, 0.3333333f),
						new Keyframe(0.5f, 1f, 0f, 0f, 0.3333333f, 0.3333333f),
						new Keyframe(1f, 0f, -2f, -2f, 0.3333333f, 0f),
					};

					_animationCurve = new AnimationCurve(keyframes);
				}

				return _animationCurve;
			}
		}


		// Info TextField -----------------

		private JSONStorableString info;
		string infoString;
		bool infoChange;
		const float infoUpdateInterval = 0.5f;
		float infoNextUpdateTime;
		UIDynamicTextField infoTextfield;

		protected void SetInfo(string value)
		{
			infoString = value;
			infoChange = true;
		}

		protected void UpdateInfo()
		{
			if (!infoChange)
			{
				return;
			}

			var time = Time.time;

			if (time < infoNextUpdateTime)
			{
				return;
			}

			info.val = infoString;
			infoChange = false;
			infoNextUpdateTime = time + infoUpdateInterval;
		}

		void CreateMorphChooser(string name, string displayName, string defaultValue, float defaultMultiply, JSONStorableStringChooser.SetStringCallback callback, 
			out JSONStorableStringChooser chooser, out UIDynamicPopup popup, out JSONStorableFloat floatMax, out UIDynamicSlider slider,
			bool rightSide)
        {
			chooser = new JSONStorableStringChooser(name, morphNames, defaultValue, displayName, callback);
			RegisterStringChooser(chooser);
			popup = CreateFilterablePopup(chooser, rightSide);
			popup.popupPanelHeight = 440f;

			floatMax = new JSONStorableFloat(displayName + " Multiply", defaultMultiply, DummyFloatCallback, 0f, 3f, true);
			RegisterFloat(floatMax);
			slider = CreateSlider(floatMax, rightSide);
		}

		public JSONStorableString CreateText(string paramName, string text, int fieldHeight, bool rightSide)
		{
			JSONStorableString textJson = new JSONStorableString(paramName, text);
			UIDynamicTextField textField = CreateTextField(textJson, rightSide);
			textField.backgroundColor = new Color(1f, 1f, 1f, 0f);
			
			LayoutElement layout = textField.GetComponent<LayoutElement>();
			layout.preferredHeight = layout.minHeight = fieldHeight;
			textField.height = fieldHeight;

			ScrollRect scrollRect = textField.UItext.transform.parent.transform.parent.transform.parent.GetComponent<ScrollRect>();
			if (scrollRect != null)
			{
				scrollRect.horizontal = false;
				scrollRect.vertical = false;
			}

			return textJson;
		}

		public override void Init() {
			try {
				pluginLabelJSON.val = $"{pluginName} {versionString}";

				if (containingAtom.type != "Person")
				{
					enabled = false;
					return;
				}

				InitMorphNames();

				// -------------------
				bones = containingAtom.transform.Find("rescale2").GetComponentsInChildren<DAZBone>();
				
				lEyeDAZBone = bones.First(eye => eye.name == "lEye");
				lEye = lEyeDAZBone.transform;

				rEyeDAZBone = bones.First(eye => eye.name == "rEye");
				rEye = rEyeDAZBone.transform;

				eyeBlink = new EyeBlink();

				// UI ---------------------------------

				CreateText("Title", "<color=#000><size=35><b>Custom Eye Auto Behaviours</b></size></color>", 40, false);

				CreateText("TitleVersionString", $"<color=#000><size=30>Version: {versionString}</size></color>", 34, false);

				enableCustomAutoBlink = new JSONStorableBool("Enable Custom Auto Blink", true, EnableCustomAutoBlinkCallback);
				RegisterBool(enableCustomAutoBlink);
				enableCustomAutoBlinkToggle = CreateToggle(enableCustomAutoBlink, false);

				btnSetDefault = CreateButton("Set Default Values");
				if (btnSetDefault != null)
				{
					btnSetDefault.button.onClick.AddListener(SetDefault);
				}

				btnSetExampleForAnimeStyle = CreateButton("Set Example Values For Anime Style");
				if (btnSetExampleForAnimeStyle != null)
				{
					btnSetExampleForAnimeStyle.button.onClick.AddListener(SetExampleForAnimeStyle);
				}

				info = new JSONStorableString("info", "");
				infoTextfield = CreateTextField(info, false);
				infoTextfield.height = 800.0f;
				info.val = about;

				// Right Side ---------------

				CreateText("Title Eyelid Top Down", "<color=#000><size=30>Eyelid Top Down</size></color>", 34, true);

				CreateMorphChooser("EyelidTopDownLeft", "Eyelid Top Down Left Morph", eyelidTopDownLeftMorphDefault, eyelidTopDownLeftMorphMultiplyDefault, EyelidTopDownLeftCallback,
					out eyelidTopDownLeftMorphChooser, out eyelidTopDownLeftMorphPopup, out eyelidTopDownLeftMorphFloatMax, out eyelidTopDownLeftMorphSlider,
					true);

				CreateMorphChooser("EyelidTopDownRight", "Eyelid Top Down Right Morph", eyelidTopDownRightMorphDefault, eyelidTopDownRightMorphMultiplyDefault, EyelidTopDownRightCallback,
					out eyelidTopDownRightMorphChooser, out eyelidTopDownRightMorphPopup, out eyelidTopDownRightMorphFloatMax, out eyelidTopDownRightMorphSlider,
					true);

				CreateText("Title Eyelid Bottom Up", "<color=#000><size=30>Eyelid Bottom Up</size></color>", 34, true);

				CreateMorphChooser("EyelidBottomUpLeft", "Eyelid Bottom Up Left Morph", eyelidBottomUpLeftMorphDefault, eyelidBottomUpLeftMorphMultiplyDefault, EyelidBottomUpLeftCallback,
					out eyelidBottomUpLeftMorphChooser, out eyelidBottomUpLeftMorphPopup, out eyelidBottomUpLeftMorphFloatMax, out eyelidBottomUpLeftMorphSlider,
					true);

				CreateMorphChooser("EyelidBottomUpRight", "Eyelid Bottom Up Right Morph", eyelidBottomUpRightMorphDefault, eyelidBottomUpRightMorphMultiplyDefault, EyelidBottomUpRightCallback,
					out eyelidBottomUpRightMorphChooser, out eyelidBottomUpRightMorphPopup, out eyelidBottomUpRightMorphFloatMax, out eyelidBottomUpRightMorphSlider,
					true);

				// Space for Popup -------------

				for (var i = 0; i < 3; i++)
                {
					CreateSpacer(true);
				}
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		protected void Update() {
			try {
				if (enableCustomAutoBlink.val)
                {
					DisableAutoBehaviour();

					eyeBlink.Update();

					UpdateMorphs();
				}
			}
			catch (Exception e) 
			{
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void UpdateMorphs()
		{
			var morphName = eyelidTopDownLeftMorphChooser.val;
			var morph = morphControl.GetMorphByDisplayName(morphName);
			morph.morphValueAdjustLimits = eyeBlink.blinkRatio * eyelidTopDownLeftMorphFloatMax.val;
			//morph.SetValue(blinkMorphRatio * 2f);

			morphName = eyelidBottomUpLeftMorphChooser.val;
			morph = morphControl.GetMorphByDisplayName(morphName);
			morph.morphValueAdjustLimits = eyeBlink.blinkRatio * eyelidBottomUpLeftMorphFloatMax.val;

			morphName = eyelidTopDownRightMorphChooser.val;
			morph = morphControl.GetMorphByDisplayName(morphName);
			morph.morphValueAdjustLimits = eyeBlink.blinkRatio * eyelidTopDownRightMorphFloatMax.val;

			morphName = eyelidBottomUpRightMorphChooser.val;
			morph = morphControl.GetMorphByDisplayName(morphName);
			morph.morphValueAdjustLimits = eyeBlink.blinkRatio * eyelidBottomUpRightMorphFloatMax.val;
		}
		void OnDestroy() {
		}

		// ------------------------------
		
		void DisableAutoBehaviour()
        {
			SetAutoBlinkState(false);
			SetAutoEyelidMorphState(false);
		}

		bool GetAutoBlinkState()
        {
			return GetAutoBehaviour("EyelidControl", "blinkEnabled");
		}

		void SetAutoBlinkState(bool val)
		{
			SetAutoBehaviour("EyelidControl", "blinkEnabled", val);
		}

		bool GetAutoEyelidMorphState()
		{
			return GetAutoBehaviour("EyelidControl", "eyelidLookMorphsEnabled");
		}

		void SetAutoEyelidMorphState(bool val)
		{
			SetAutoBehaviour("EyelidControl", "eyelidLookMorphsEnabled", val);
		}

		bool GetAutoBehaviour(string storableID, string boolParamName)
        {
			JSONStorableBool isAutoBehaviourEnabled = containingAtom.GetStorableByID(storableID)?.GetBoolJSONParam(boolParamName);
			if (isAutoBehaviourEnabled == null)
			{
				return false;
			}

			return isAutoBehaviourEnabled.val;
		}

		private void SetAutoBehaviour(string storableID, string boolParamName, bool enabled)
		{
			JSONStorableBool isAutoBehaviourEnabled = containingAtom.GetStorableByID(storableID)?.GetBoolJSONParam(boolParamName);
			if (isAutoBehaviourEnabled == null)
			{
				return;
			}

			if (isAutoBehaviourEnabled.val != enabled)
            {
				isAutoBehaviourEnabled.val = enabled;
			}
		}


		void SetDefault()
        {
			eyelidTopDownLeftMorphChooser.val = eyelidTopDownLeftMorphDefault;
			eyelidTopDownLeftMorphFloatMax.val = eyelidTopDownLeftMorphMultiplyDefault;
			eyelidTopDownRightMorphChooser.val = eyelidTopDownRightMorphDefault;
			eyelidTopDownRightMorphFloatMax.val = eyelidTopDownRightMorphMultiplyDefault;

			eyelidTopUpLeftMorphChooser.val = eyelidUpLeftMorphDefault;
			eyelidTopUpLeftMorphFloatMax.val = eyelidUpLeftMorphMultiplyDefault;
			eyelidTopUpRightMorphChooser.val = eyelidUpRightMorphDefault;
			eyelidTopUpRightMorphFloatMax.val = eyelidUpRightMorphMultiplyDefault;

			eyelidBottomUpLeftMorphChooser.val = eyelidBottomUpLeftMorphDefault;
			eyelidTopUpLeftMorphFloatMax.val = eyelidBottomUpLeftMorphMultiplyDefault;
			eyelidBottomUpRightMorphChooser.val = eyelidBottomUpRightMorphDefault;
			eyelidTopUpRightMorphFloatMax.val = eyelidBottomUpRightMorphMultiplyDefault;
		}


		void SetExampleForAnimeStyle()
		{
			eyelidTopDownLeftMorphChooser.val = eyelidTopDownLeftMorphDefault;
			eyelidTopDownLeftMorphFloatMax.val = 2.5f;
			eyelidTopDownRightMorphChooser.val = eyelidTopDownRightMorphDefault;
			eyelidTopDownRightMorphFloatMax.val = 2.5f;

			eyelidTopUpLeftMorphChooser.val = eyelidUpLeftMorphDefault;
			eyelidTopUpLeftMorphFloatMax.val = 1f;
			eyelidTopUpRightMorphChooser.val = eyelidUpRightMorphDefault;
			eyelidTopUpRightMorphFloatMax.val = 1f;

			eyelidBottomUpLeftMorphChooser.val = eyelidBottomUpLeftMorphDefault;
			eyelidTopUpLeftMorphFloatMax.val = 0.5f;
			eyelidBottomUpRightMorphChooser.val = eyelidBottomUpRightMorphDefault;
			eyelidTopUpRightMorphFloatMax.val = 0.5f;
		}

        protected void DummyFloatCallback(JSONStorableFloat jf)
        {
        }

        private void EyelidTopDownLeftCallback(string s)
		{
		}

		private void EyelidTopDownRightCallback(string s)
		{
		}

		private void EyelidBottomUpLeftCallback(string s)
		{
		}

		private void EyelidBottomUpRightCallback(string s)
		{
		}

		private void EnableCustomAutoBlinkCallback(bool val)
		{
			if (!val)
            {
				eyeBlink.BlinkClear();
			}
		}
	}
}
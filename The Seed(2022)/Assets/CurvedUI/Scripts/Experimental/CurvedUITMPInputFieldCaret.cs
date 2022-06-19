using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if CURVEDUI_TMP
using TMPro;
#endif 

namespace CurvedUI
{

#if !UNITY_5_1 && !UNITY_5_2 && CURVEDUI_TMP
    /// <summary>
    /// Creates a recttransform caret with image component that can be curved with curvedUI. Hides inputfield's original caret.
    /// </summary>
    [ExecuteInEditMode]
    public class CurvedUITMPInputFieldCaret : MonoBehaviour, ISelectHandler, IDeselectHandler
    {

        //references
        TMP_InputField myField;
        RectTransform myCaret;
        Color origCaretColor;
        Color origSelectionColor;

        //variables
        bool selected = false;
        bool selectingText = false;



        void Awake()
        {
            myField = this.GetComponent<TMP_InputField>();

            if (myField)
                CheckAndConvertMask();
        }

        void Update()
        {
            //only update the caret's position when the field is selected.
            if (selected)
                UpdateCaret();
        } 

        /// <summary>
        /// On select, set the caret active and start blinker coroutine.
        /// </summary>
        /// <param name="eventData"></param>
		public void OnSelect(BaseEventData eventData)
        {
            if (myCaret == null)
                CreateCaret();

            selected = true;
            myCaret.gameObject.SetActive(true);
            StartCoroutine(CaretBlinker());
        }

        /// <summary>
        /// Hide the caret on deselect.
        /// </summary>
        /// <param name="eventData"></param>
		public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            myCaret.gameObject.SetActive(false);

        }

        /// <summary>
        /// Simple blinker. Blinks the caret if inputfield is selected and user is not selecting text.
        /// </summary>
        /// <returns></returns>
		IEnumerator CaretBlinker()
        {
            while (selected)
            {
                myCaret.gameObject.SetActive(selectingText ? true : !myCaret.gameObject.activeSelf);
                yield return new WaitForSeconds(0.5f / (float)myField.caretBlinkRate);
            }

        }

        void CreateCaret()
        {

            //lets create a curvedui caret and copy the settings from our input field
            GameObject go = new GameObject("CurvedUI_TMPCaret");
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>();
            go.AddComponent<CurvedUIVertexEffect>();
            go.transform.SetParent(transform.GetChild(0).GetChild(0));//Nest the cursor down with the scrolling child so it properly moves with text.
            go.transform.localScale = Vector3.one;
            (go.transform as RectTransform).anchoredPosition3D = Vector3.zero;
            //(go.transform as RectTransform).pivot = new Vector2(0, 0.5f);
            (go.transform as RectTransform).pivot = new Vector2(0, 1.0f);

            //Copy caret color into new image.
            go.GetComponent<Image>().color = myField.caretColor;

            myCaret = go.transform as RectTransform;
            go.transform.SetAsFirstSibling();

            //save original color and hide the original caret

            myField.customCaretColor = true;
            origCaretColor = myField.caretColor;
            myField.caretColor = new Color(0f, 0f, 0f, 0f);

            origSelectionColor = myField.selectionColor;
            myField.selectionColor = new Color(0f, 0f, 0f, 0f);

            go.gameObject.SetActive(false);
        }



        void UpdateCaret()
        {
            if (myCaret == null)
                CreateCaret();

            //Debug.Log("caret:" + myField.caretPosition + " / focus:"+ myField.selectionFocusPosition + " / anchor:" + myField.selectionAnchorPosition  + " / charc:" + myField.textComponent.cachedTextGenerator.characterCount + " / charvis:" + myField.textComponent.cachedTextGenerator.characterCountVisible);
            Vector2 newCaretPos = GetLocalPositionInText(myField.caretPosition);
            //RectTransform originalCaret = (RectTransform)myField.transform.FindChild(myField.name + " Input Caret");

            if (myField.selectionFocusPosition != myField.selectionAnchorPosition) // user is selecting text is those two are not equal.
            {
                selectingText = true;

                Vector2 selectionSize = new Vector2(
                    GetLocalPositionInText(myField.selectionAnchorPosition).x - GetLocalPositionInText(myField.selectionFocusPosition).x,
                    GetLocalPositionInText(myField.selectionAnchorPosition).y - GetLocalPositionInText(myField.selectionFocusPosition).y
                    );
                newCaretPos = selectionSize.x < 0 ? GetLocalPositionInText(myField.selectionAnchorPosition) : GetLocalPositionInText(myField.selectionFocusPosition);

                selectionSize = new Vector2(Mathf.Abs(selectionSize.x), Mathf.Abs(selectionSize.y) + myField.textComponent.fontSize);

                myCaret.sizeDelta = new Vector2(selectionSize.x, selectionSize.y);
                myCaret.anchoredPosition = newCaretPos;
                myCaret.GetComponent<Image>().color = origSelectionColor;
            }
            else { // user is not selecting text, just update the caret position.

                selectingText = false;

                //myCaret.sizeDelta = new Vector2(myField.caretWidth, originalCaret == null ? 10 : originalCaret.rect.height);
                myCaret.sizeDelta = new Vector2(myField.caretWidth, myField.textComponent.fontSize);
                myCaret.anchoredPosition = newCaretPos;
                myCaret.GetComponent<Image>().color = origCaretColor;

            }
        }

        /// <summary>
        /// Returns the position in TMP_Inputfield's rectransform local space, based on character position in text. Pretty neat actually. 
        /// </summary>
        /// <param name="charNo"></param>
        /// <returns></returns>
        Vector2 GetLocalPositionInText(int charNo)
        {
            if (myField.isFocused)
            {
                TMP_TextInfo txtInfo = myField.textComponent.textInfo;

                if (charNo > txtInfo.characterCount - 1) //do not go over the text length.
                    charNo = txtInfo.characterCount - 1;

                TMP_CharacterInfo charInfo = txtInfo.characterInfo[charNo];

                return new Vector2(charInfo.topLeft.x, charInfo.ascender);
            }
            else return Vector2.zero; // field not focused, return 0,0
        }


        #region MASK CONVERTING
        /// <summary>
        /// Converts InputField's RectMask2D to a Mask + Image component combination that works better with CurvedUI
        /// </summary>
        void CheckAndConvertMask()
        {
            foreach(Transform trans in this.transform)
            {
                if(trans.GetComponent<RectMask2D>()!= null)
                {
                    DestroyImmediate (trans.GetComponent<RectMask2D>());

                    trans.AddComponentIfMissing<Image>();
                    trans.AddComponentIfMissing<Mask>();
                }
            }
        }
        #endregion


        #region SETTERS AND GETTERS
        public Color CaretColor {
            get { return origCaretColor; }
            set { origCaretColor = value; }
        }

        public Color SelectionColor {
            get { return origSelectionColor; }
            set { origSelectionColor = value; }
        }

        public float CaretBlinkRate {
            get { return myField.caretBlinkRate; }
            set { myField.caretBlinkRate = value; }
        }
        #endregion

#else
    public class CurvedUITMPInputFieldCaret : MonoBehaviour
    {
        //Unity before 5.3 does not contain methods necessary to curve the input field caret without changing original script.
#endif
    }

}

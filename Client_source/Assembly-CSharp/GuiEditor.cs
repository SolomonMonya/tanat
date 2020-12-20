using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class GuiEditor : MonoBehaviour
{
	private enum ParamType
	{
		NONE,
		IMAGE,
		RECT,
		BTN
	}

	private class EditorElement
	{
		public Texture2D mImage;

		public Rect mRect = default(Rect);

		public string mPath = string.Empty;

		public int mLayer;

		public string mId = "none";

		public bool mButton;

		public EditorElement(EditorElement _element)
		{
			mImage = _element.mImage;
			mPath = _element.mPath;
			mRect = _element.mRect;
			mLayer = _element.mLayer;
			mId = _element.mId;
			mButton = _element.mButton;
		}

		public EditorElement()
		{
		}
	}

	private class EditorImage
	{
		public Texture2D mImage;

		public string mPath = string.Empty;

		public EditorImage(Texture2D _img, string _path)
		{
			mImage = _img;
			mPath = _path;
		}
	}

	private class ElementComparer : IComparer<EditorElement>
	{
		public int Compare(EditorElement _data1, EditorElement _data2)
		{
			return CompareInt(_data1.mLayer, _data2.mLayer);
		}

		private int CompareInt(int _val1, int _val2)
		{
			if (_val1 > _val2)
			{
				return 1;
			}
			if (_val1 < _val2)
			{
				return -1;
			}
			return 0;
		}
	}

	private enum EditMode
	{
		NONE,
		GUI,
		MAIN
	}

	private List<EditorElement> mElements;

	private Dictionary<string, List<EditorImage>> mAvailableImages;

	private ElementComparer mElementComparer;

	private string mGuiPath = string.Empty;

	private EditMode mEditMode;

	private Vector2 mScrollPathPos = Vector2.zero;

	private Vector2 mScrollImgPos = Vector2.zero;

	private string mSelectedImgId = string.Empty;

	private EditorImage mPreviewImage;

	private EditorElement mSelectedElement;

	private EditorElement mOverElement;

	private int mCurLayer;

	private int mScaleDelta = 2;

	private bool mShowEditWnd;

	private Rect mEditWndRect = default(Rect);

	private string mFilename = string.Empty;

	public void Start()
	{
		mElements = new List<EditorElement>();
		mAvailableImages = new Dictionary<string, List<EditorImage>>();
		mElementComparer = new ElementComparer();
		mGuiPath = "Gui/misc";
		mEditMode = EditMode.GUI;
		mEditWndRect = new Rect(Screen.width - 200, 0f, 200f, 200f);
		if (!Application.isEditor)
		{
			Screen.SetResolution(1280, 960, fullscreen: false);
		}
	}

	public void OnGUI()
	{
		foreach (EditorElement mElement in mElements)
		{
			GUI.DrawTexture(mElement.mRect, mElement.mImage);
		}
		if (Event.current.type == EventType.KeyDown && !mShowEditWnd)
		{
			if (Event.current.keyCode == KeyCode.F1)
			{
				mEditMode = EditMode.GUI;
			}
			else if (Event.current.keyCode == KeyCode.F2)
			{
				mEditMode = EditMode.MAIN;
			}
		}
		switch (mEditMode)
		{
		case EditMode.GUI:
			RenderGUIMode();
			break;
		case EditMode.MAIN:
			RenderMainMode();
			break;
		}
	}

	private void RenderGUIMode()
	{
		RenderSelectGUI();
		RenderPreviewGUI();
		mFilename = GUI.TextField(new Rect(Screen.width - 105, Screen.height - 100, 100f, 20f), mFilename);
		if (GUI.Button(new Rect(Screen.width - 105, Screen.height - 75, 100f, 20f), "Save"))
		{
			Save(mFilename);
		}
		if (GUI.Button(new Rect(Screen.width - 105, Screen.height - 50, 100f, 20f), "Load"))
		{
			Load(mFilename);
		}
		if (GUI.Button(new Rect(Screen.width - 105, Screen.height - 25, 100f, 20f), "GenerateCS"))
		{
			GenerateCS(mFilename);
		}
	}

	private void RenderSelectGUI()
	{
		mGuiPath = GUI.TextField(new Rect(10f, 10f, 100f, 20f), mGuiPath);
		if (GUI.Button(new Rect(120f, 10f, 100f, 20f), "Load Images"))
		{
			LoadImages(mGuiPath);
		}
		if (GUI.Button(new Rect(230f, 10f, 100f, 20f), "Unload Images"))
		{
			UnloadImages();
		}
		if (mAvailableImages.Count > 0)
		{
			mScrollPathPos = GUI.BeginScrollView(new Rect(10f, 35f, 200f, Screen.height - 35), mScrollPathPos, new Rect(0f, 0f, 180f, mAvailableImages.Count * 20));
			int num = 0;
			foreach (KeyValuePair<string, List<EditorImage>> mAvailableImage in mAvailableImages)
			{
				if (GUI.Button(new Rect(0f, num * 20, 180f, 20f), mAvailableImage.Key))
				{
					SetImages(mAvailableImage.Key);
				}
				num++;
			}
			GUI.EndScrollView();
		}
		if (!mAvailableImages.ContainsKey(mSelectedImgId))
		{
			return;
		}
		List<EditorImage> list = mAvailableImages[mSelectedImgId];
		mScrollImgPos = GUI.BeginScrollView(new Rect(220f, 35f, 200f, Screen.height - 35), mScrollImgPos, new Rect(0f, 0f, 180f, list.Count * 20));
		int num2 = 0;
		foreach (EditorImage item in list)
		{
			if (GUI.Button(new Rect(0f, num2 * 20, 180f, 20f), item.mImage.name))
			{
				SetCurImage(item);
			}
			num2++;
		}
		GUI.EndScrollView();
	}

	private void RenderPreviewGUI()
	{
		if (mPreviewImage != null)
		{
			GUI.Box(new Rect(Screen.width - 512, 10f, 512f, 512f), string.Empty);
			float num = ((mPreviewImage.mImage.width < mPreviewImage.mImage.height) ? ((float)mPreviewImage.mImage.height / 512f) : ((float)mPreviewImage.mImage.width / 512f));
			float num2 = (float)mPreviewImage.mImage.width / num;
			float num3 = (float)mPreviewImage.mImage.height / num;
			num2 = ((!(num2 <= (float)mPreviewImage.mImage.width)) ? ((float)mPreviewImage.mImage.width) : num2);
			num3 = ((!(num3 <= (float)mPreviewImage.mImage.height)) ? ((float)mPreviewImage.mImage.height) : num3);
			Rect position = new Rect((float)(Screen.width - 512) + (512f - num2) / 2f, 10f + (512f - num3) / 2f, num2, num3);
			GUI.DrawTexture(position, mPreviewImage.mImage);
			if (GUI.Button(new Rect(Screen.width - 512, 532f, 100f, 20f), "Add Image"))
			{
				AddElement(mPreviewImage);
			}
		}
	}

	private void RenderMainMode()
	{
		GUI.Label(new Rect(Screen.width - 200, Screen.height - 20, 200f, 20f), " l : " + mCurLayer);
		mOverElement = GetElementByPos(Event.current.mousePosition);
		if (Event.current.type == EventType.MouseDrag)
		{
			if (mSelectedElement != null && !mEditWndRect.Contains(Event.current.mousePosition))
			{
				mSelectedElement.mRect.x = Mathf.RoundToInt(Event.current.mousePosition.x - mSelectedElement.mRect.width / 2f);
				mSelectedElement.mRect.y = Mathf.RoundToInt(Event.current.mousePosition.y - mSelectedElement.mRect.height / 2f);
			}
		}
		else if (Event.current.type == EventType.MouseDown)
		{
			if (!mShowEditWnd && Event.current.button == 0)
			{
				mSelectedElement = GetElementByPos(Event.current.mousePosition);
				mShowEditWnd = mSelectedElement != null;
			}
			if (Event.current.button == 1)
			{
				mSelectedElement = null;
				mShowEditWnd = false;
			}
		}
		else if (Event.current.type == EventType.KeyUp)
		{
			switch (Event.current.keyCode)
			{
			case KeyCode.UpArrow:
				if (Event.current.control)
				{
					mCurLayer++;
				}
				break;
			case KeyCode.DownArrow:
				if (Event.current.control)
				{
					mCurLayer--;
				}
				break;
			}
		}
		if (mOverElement != null)
		{
			GUI.Box(mOverElement.mRect, string.Empty);
			int num = (int)mOverElement.mRect.x;
			int num2 = (int)mOverElement.mRect.y;
			int num3 = (int)mOverElement.mRect.width;
			int num4 = (int)mOverElement.mRect.height;
			GUI.Label(new Rect(0f, Screen.height - 20, 300f, 20f), "id : " + mOverElement.mId + " x : " + num + " y : " + num2 + " w : " + num3 + " h : " + num4 + " l : " + mOverElement.mLayer);
		}
		if (mSelectedElement == null)
		{
			return;
		}
		GUI.Box(mSelectedElement.mRect, string.Empty);
		if (Event.current.type == EventType.KeyUp)
		{
			switch (Event.current.keyCode)
			{
			case KeyCode.LeftArrow:
				if (!Event.current.control && !Event.current.alt)
				{
					mSelectedElement.mRect.x -= 1f;
				}
				break;
			case KeyCode.RightArrow:
				if (!Event.current.control && !Event.current.alt)
				{
					mSelectedElement.mRect.x += 1f;
				}
				break;
			case KeyCode.UpArrow:
				if (Event.current.control)
				{
					mSelectedElement.mLayer++;
					SortElements();
				}
				else if (Event.current.alt)
				{
					mSelectedElement.mRect.width += mScaleDelta;
					mSelectedElement.mRect.height += mScaleDelta;
				}
				else
				{
					mSelectedElement.mRect.y -= 1f;
				}
				break;
			case KeyCode.DownArrow:
				if (Event.current.control)
				{
					mSelectedElement.mLayer--;
					if (mSelectedElement.mLayer < 0)
					{
						mSelectedElement.mLayer = 0;
					}
					SortElements();
				}
				else if (Event.current.alt)
				{
					mSelectedElement.mRect.width -= mScaleDelta;
					mSelectedElement.mRect.height -= mScaleDelta;
				}
				else
				{
					mSelectedElement.mRect.y += 1f;
				}
				break;
			case KeyCode.Delete:
				if (Event.current.control)
				{
					RemoveElement(ref mSelectedElement);
				}
				break;
			case KeyCode.D:
				if (Event.current.control)
				{
					DuplicateElement(mSelectedElement);
				}
				break;
			}
		}
		if (mShowEditWnd)
		{
			RenderEditWnd();
		}
	}

	private void RenderEditWnd()
	{
		if (mSelectedElement != null)
		{
			GUI.Box(mEditWndRect, string.Empty);
			GUI.Label(new Rect(mEditWndRect.x + 110f, mEditWndRect.y + 5f, 100f, 20f), "Id");
			mSelectedElement.mId = GUI.TextField(new Rect(mEditWndRect.x + 5f, mEditWndRect.y + 5f, 100f, 20f), mSelectedElement.mId);
			GUI.Label(new Rect(mEditWndRect.x + 110f, mEditWndRect.y + 30f, 100f, 20f), "Pos");
			string text = mSelectedElement.mRect.x.ToString();
			string text2 = mSelectedElement.mRect.y.ToString();
			text = GUI.TextField(new Rect(mEditWndRect.x + 5f, mEditWndRect.y + 30f, 50f, 20f), text);
			text2 = GUI.TextField(new Rect(mEditWndRect.x + 60f, mEditWndRect.y + 30f, 50f, 20f), text2);
			int result = 0;
			int result2 = 0;
			if (int.TryParse(text, out result))
			{
				mSelectedElement.mRect.x = result;
			}
			if (int.TryParse(text2, out result2))
			{
				mSelectedElement.mRect.y = result2;
			}
			GUI.Label(new Rect(mEditWndRect.x + 110f, mEditWndRect.y + 55f, 100f, 20f), "Size");
			string text3 = mSelectedElement.mRect.width.ToString();
			string text4 = mSelectedElement.mRect.height.ToString();
			text3 = GUI.TextField(new Rect(mEditWndRect.x + 5f, mEditWndRect.y + 55f, 50f, 20f), text3);
			text4 = GUI.TextField(new Rect(mEditWndRect.x + 60f, mEditWndRect.y + 55f, 50f, 20f), text4);
			int result3 = 0;
			int result4 = 0;
			if (int.TryParse(text3, out result3))
			{
				mSelectedElement.mRect.width = result3;
			}
			if (int.TryParse(text4, out result4))
			{
				mSelectedElement.mRect.height = result4;
			}
			mSelectedElement.mButton = GUI.Toggle(new Rect(mEditWndRect.x + 5f, mEditWndRect.y + 80f, 100f, 20f), mSelectedElement.mButton, "Button");
		}
	}

	private void SortElements()
	{
		mElements.Sort(mElementComparer);
	}

	private void LoadImages(string _path)
	{
		if (mAvailableImages.ContainsKey(_path))
		{
			return;
		}
		mAvailableImages.Add(_path, new List<EditorImage>());
		Object[] array = Resources.LoadAll(_path, typeof(Texture2D));
		Texture2D texture2D = null;
		Object[] array2 = array;
		foreach (Object @object in array2)
		{
			texture2D = @object as Texture2D;
			if (texture2D != null)
			{
				mAvailableImages[_path].Add(new EditorImage(texture2D, _path));
			}
		}
	}

	private void UnloadImages()
	{
		if (mAvailableImages.ContainsKey(mGuiPath))
		{
			mAvailableImages[mGuiPath].Clear();
			mAvailableImages.Remove(mGuiPath);
			if (mSelectedImgId == mGuiPath)
			{
				mSelectedImgId = string.Empty;
			}
		}
	}

	private void SetImages(string _id)
	{
		mSelectedImgId = _id;
	}

	private void SetCurImage(EditorImage _img)
	{
		mPreviewImage = _img;
	}

	private void AddElement(EditorImage _image)
	{
		EditorElement editorElement = new EditorElement();
		editorElement.mImage = _image.mImage;
		editorElement.mPath = _image.mPath;
		editorElement.mRect = new Rect(0f, 0f, editorElement.mImage.width, editorElement.mImage.height);
		mElements.Add(editorElement);
	}

	private void AddElement(string _id, string _path, string img, int _layer, Rect _rect, bool _isBtn)
	{
		if (!mAvailableImages.ContainsKey(_path))
		{
			LoadImages(_path);
		}
		Texture2D mImage = null;
		List<EditorImage> list = mAvailableImages[_path];
		foreach (EditorImage item in list)
		{
			if (item.mImage.name == img)
			{
				mImage = item.mImage;
			}
		}
		EditorElement editorElement = new EditorElement();
		editorElement.mId = _id;
		editorElement.mImage = mImage;
		editorElement.mPath = _path;
		editorElement.mRect = _rect;
		editorElement.mLayer = _layer;
		editorElement.mButton = _isBtn;
		if (editorElement.mImage != null)
		{
			mElements.Add(editorElement);
		}
	}

	private EditorElement GetElementByPos(Vector2 _mousePos)
	{
		foreach (EditorElement mElement in mElements)
		{
			if (mElement.mRect.Contains(_mousePos) && mElement.mLayer == mCurLayer)
			{
				return mElement;
			}
		}
		return null;
	}

	private void RemoveElement(ref EditorElement _element)
	{
		mElements.Remove(_element);
		_element = null;
		mShowEditWnd = false;
	}

	private void DuplicateElement(EditorElement _element)
	{
		mElements.Add(new EditorElement(_element));
	}

	private void Save(string _path)
	{
		XmlWriter xmlWriter = XmlWriter.Create(_path + ".xml");
		if (xmlWriter == null)
		{
			return;
		}
		xmlWriter.WriteStartElement("GUI");
		foreach (EditorElement mElement in mElements)
		{
			xmlWriter.WriteStartElement("Element");
			xmlWriter.WriteStartElement("Rect");
			xmlWriter.WriteAttributeString("x", ((int)mElement.mRect.x).ToString());
			xmlWriter.WriteAttributeString("y", ((int)mElement.mRect.y).ToString());
			xmlWriter.WriteAttributeString("width", ((int)mElement.mRect.width).ToString());
			xmlWriter.WriteAttributeString("height", ((int)mElement.mRect.height).ToString());
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("Data");
			xmlWriter.WriteAttributeString("id", mElement.mId);
			xmlWriter.WriteAttributeString("layer", mElement.mLayer.ToString());
			xmlWriter.WriteAttributeString("path", mElement.mPath);
			xmlWriter.WriteAttributeString("img", mElement.mImage.name);
			xmlWriter.WriteAttributeString("isBtn", (!mElement.mButton) ? "0" : "1");
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
	}

	private void Load(string _path)
	{
		XmlReader xmlReader = XmlReader.Create(_path + ".xml");
		if (xmlReader == null)
		{
			return;
		}
		CleanEditor();
		while (xmlReader.Read())
		{
			if (xmlReader.NodeType != XmlNodeType.Element || !("Element" == xmlReader.Name))
			{
				continue;
			}
			Rect rect = default(Rect);
			string id = string.Empty;
			int layer = 0;
			string path = string.Empty;
			string img = string.Empty;
			bool isBtn = false;
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					switch (xmlReader.Name)
					{
					case "Rect":
						xmlReader.MoveToAttribute("x");
						rect.x = int.Parse(xmlReader.Value);
						xmlReader.MoveToAttribute("y");
						rect.y = int.Parse(xmlReader.Value);
						xmlReader.MoveToAttribute("width");
						rect.width = int.Parse(xmlReader.Value);
						xmlReader.MoveToAttribute("height");
						rect.height = int.Parse(xmlReader.Value);
						break;
					case "Data":
						xmlReader.MoveToAttribute("id");
						id = xmlReader.Value;
						xmlReader.MoveToAttribute("layer");
						layer = int.Parse(xmlReader.Value);
						xmlReader.MoveToAttribute("path");
						path = xmlReader.Value;
						xmlReader.MoveToAttribute("img");
						img = xmlReader.Value;
						xmlReader.MoveToAttribute("isBtn");
						isBtn = int.Parse(xmlReader.Value) == 1;
						break;
					}
				}
				else if (xmlReader.NodeType == XmlNodeType.EndElement)
				{
					AddElement(id, path, img, layer, rect, isBtn);
					break;
				}
			}
		}
		xmlReader.Close();
	}

	private void GenerateCS(string _path)
	{
		if (!CheckElements())
		{
			Debug.LogError("Problem with element layers");
			return;
		}
		_path += ".cs";
		if (File.Exists(_path))
		{
			File.Delete(_path);
		}
		FileStream fs = File.Create(_path);
		UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
		WriteParamsToFile(fs, encoding);
		WriteInitToFile(fs, encoding);
		WriteSetSizeToFile(fs, encoding);
		WriteRenderToFile(fs, encoding);
		WriteCheckEventToFile(fs, encoding);
		WriteOnButtonToFile(fs, encoding);
	}

	private bool CheckElements()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<string> list = new List<string>();
		foreach (EditorElement mElement in mElements)
		{
			if (!flag)
			{
				flag = mElement.mLayer == 0;
			}
			else if (flag)
			{
				flag2 = mElement.mLayer == 0;
				if (flag2)
				{
					Debug.LogError("mLayer == 0 in " + mElement.mId + " but there is another one! Can't generate *.cs");
					break;
				}
			}
			flag3 = list.Contains(mElement.mId);
			if (!flag3)
			{
				list.Add(mElement.mId);
				continue;
			}
			Debug.LogError("mLayer == 0 in " + mElement.mId + " but there is another one! Can't generate *.cs");
			break;
		}
		return flag && !flag2 && !flag3;
	}

	private void WriteParamsToFile(FileStream _fs, UTF8Encoding _encoding)
	{
		foreach (EditorElement mElement in mElements)
		{
			if (!mElement.mButton)
			{
				AddText(_fs, _encoding, "private Texture2D " + GenerateParamString(mElement.mId, ParamType.IMAGE) + ";\n");
				if (mElement.mLayer != 0)
				{
					AddText(_fs, _encoding, "private Rect " + GenerateParamString(mElement.mId, ParamType.RECT) + ";\n");
				}
			}
			else
			{
				AddText(_fs, _encoding, "private GuiButton " + GenerateParamString(mElement.mId, ParamType.BTN) + ";\n");
			}
		}
		AddText(_fs, _encoding, "\n");
	}

	private void WriteInitToFile(FileStream _fs, UTF8Encoding _encoding)
	{
		AddText(_fs, _encoding, "public override void Init()\n{\n");
		foreach (EditorElement mElement in mElements)
		{
			string text = "\"" + mElement.mPath + "/" + mElement.mImage.name + "\"";
			if (!mElement.mButton)
			{
				AddText(_fs, _encoding, "\t" + GenerateParamString(mElement.mId, ParamType.IMAGE) + " = GuiSystem.GetImage(" + text + ");\n");
				continue;
			}
			AddText(_fs, _encoding, "\n");
			AddText(_fs, _encoding, "\t" + GenerateParamString(mElement.mId, ParamType.BTN) + " = GuiSystem.CreateButton(" + text + ", " + text + ", " + text + ", \"\", \"\");\n");
			AddText(_fs, _encoding, "\t" + GenerateParamString(mElement.mId, ParamType.BTN) + ".mElementId = \"" + mElement.mId.ToUpper() + "\";\n");
			AddText(_fs, _encoding, "\t" + GenerateParamString(mElement.mId, ParamType.BTN) + ".mOnMouseUp += OnButton;\n");
			AddText(_fs, _encoding, "\t" + GenerateParamString(mElement.mId, ParamType.BTN) + ".Init();\n\n");
		}
		AddText(_fs, _encoding, "}\n\n");
	}

	private void WriteSetSizeToFile(FileStream _fs, UTF8Encoding _encoding)
	{
		AddText(_fs, _encoding, "public override void SetSize()\n{\n");
		Rect rect = default(Rect);
		foreach (EditorElement mElement in mElements)
		{
			if (!mElement.mButton)
			{
				string text = GenerateParamString(mElement.mId, ParamType.RECT);
				if (mElement.mLayer == 0)
				{
					rect = mElement.mRect;
					AddText(_fs, _encoding, "\tmZoneRect = new Rect(" + mElement.mRect.x + ", " + mElement.mRect.y + ", " + mElement.mRect.width + ", " + mElement.mRect.height + ");\n");
					AddText(_fs, _encoding, "\tGuiSystem.GetRectScaled(ref mZoneRect);\n");
				}
				else
				{
					AddText(_fs, _encoding, "\t" + text + " = new Rect(" + (mElement.mRect.x - rect.x).ToString() + ", " + (mElement.mRect.y - rect.y).ToString() + ", " + mElement.mRect.width + ", " + mElement.mRect.height + ");\n");
					AddText(_fs, _encoding, "\tGuiSystem.SetChildRect(mZoneRect, ref " + text + ");\n");
				}
			}
			else
			{
				string text2 = GenerateParamString(mElement.mId, ParamType.BTN);
				AddText(_fs, _encoding, "\t" + text2 + ".mZoneRect = new Rect(" + mElement.mRect.x + ", " + mElement.mRect.y + ", " + mElement.mRect.width + ", " + mElement.mRect.height + ");\n");
				AddText(_fs, _encoding, "\tGuiSystem.SetChildRect(mZoneRect, ref " + text2 + ".mZoneRect);\n");
			}
		}
		AddText(_fs, _encoding, "}\n\n");
	}

	private void WriteRenderToFile(FileStream _fs, UTF8Encoding _encoding)
	{
		AddText(_fs, _encoding, "public override void RenderElement()\n{\n");
		foreach (EditorElement mElement in mElements)
		{
			if (!mElement.mButton)
			{
				string text = GenerateParamString(mElement.mId, ParamType.IMAGE);
				string text2 = GenerateParamString(mElement.mId, ParamType.RECT);
				if (mElement.mLayer == 0)
				{
					AddText(_fs, _encoding, "\tGuiSystem.DrawImage(" + text + ", mZoneRect);\n");
					continue;
				}
				AddText(_fs, _encoding, "\tGuiSystem.DrawImage(" + text + ", " + text2 + ");\n");
			}
			else
			{
				string text3 = GenerateParamString(mElement.mId, ParamType.BTN);
				AddText(_fs, _encoding, "\t" + text3 + ".RenderElement();\n");
			}
		}
		AddText(_fs, _encoding, "}\n\n");
	}

	private void WriteCheckEventToFile(FileStream _fs, UTF8Encoding _encoding)
	{
		AddText(_fs, _encoding, "public override void CheckEvent(Event _curEvent)\n{\n");
		foreach (EditorElement mElement in mElements)
		{
			if (mElement.mButton)
			{
				string text = GenerateParamString(mElement.mId, ParamType.BTN);
				AddText(_fs, _encoding, "\t" + text + ".CheckEvent(_curEvent);\n");
			}
		}
		AddText(_fs, _encoding, "\tbase.CheckEvent(_curEvent);\n");
		AddText(_fs, _encoding, "}\n\n");
	}

	private void WriteOnButtonToFile(FileStream _fs, UTF8Encoding _encoding)
	{
		AddText(_fs, _encoding, "private void OnButton(GuiElement _sender, int _buttonId)\n{\n");
		foreach (EditorElement mElement in mElements)
		{
			if (mElement.mButton)
			{
				AddText(_fs, _encoding, "\tif (_buttonId == 0 && _sender.mElementId == \"" + mElement.mId.ToUpper() + "\")\n");
				AddText(_fs, _encoding, "\t{\n\n");
				AddText(_fs, _encoding, "\t}\n");
			}
		}
		AddText(_fs, _encoding, "}\n\n");
	}

	private static void AddText(FileStream _fs, UTF8Encoding _encoding, string _data)
	{
		byte[] bytes = _encoding.GetBytes(_data);
		_fs.Write(bytes, 0, bytes.Length);
	}

	private string GenerateParamString(string _name, ParamType _type)
	{
		string text = "m";
		text += _name.Substring(0, 1).ToUpper();
		text += _name.Substring(1);
		switch (_type)
		{
		case ParamType.IMAGE:
			return text + "Image";
		case ParamType.RECT:
			return text + "Rect";
		case ParamType.BTN:
			return text + "Button";
		default:
			Debug.LogError("Bad param type : " + _type);
			return string.Empty;
		}
	}

	private void CleanEditor()
	{
		int i = 0;
		for (int count = mElements.Count; i < count; i++)
		{
			mElements[i] = null;
		}
		mElements.Clear();
		mShowEditWnd = false;
	}
}

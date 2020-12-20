using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class AriseTextMgr
{
	public enum Style
	{
		TEXT,
		DMG,
		MONEY
	}

	private UnityEngine.Object mPrefab;

	private Dictionary<Style, Font> mFonts = new Dictionary<Style, Font>();

	private Dictionary<Style, Color> mFontColors = new Dictionary<Style, Color>();

	private Dictionary<Style, Queue<GameObject>> mFree = new Dictionary<Style, Queue<GameObject>>();

	public AriseTextMgr()
	{
		AssetLoader.Instance?.LoadAsset("VFX_ArisingText", typeof(GameObject), new Notifier<ILoadedAsset, object>(OnLoaded, null));
		UnityEngine.Object original = Resources.Load("Fonts/Tahoma_14");
		Font font = null;
		for (Style style = Style.TEXT; style <= Style.MONEY; style++)
		{
			font = UnityEngine.Object.Instantiate(original) as Font;
			mFonts.Add(style, font);
			mFree.Add(style, new Queue<GameObject>());
		}
		mFontColors[Style.TEXT] = Color.white;
		mFontColors[Style.DMG] = Color.red;
		mFontColors[Style.MONEY] = Color.yellow;
	}

	public AriseBehavior Show(string _txt, Vector3 _pos, Style _style)
	{
		if (null == mPrefab)
		{
			return null;
		}
		GameObject gameObject = null;
		if (mFree[_style].Count > 0)
		{
			gameObject = mFree[_style].Dequeue();
			gameObject.SetActiveRecursively(state: true);
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(mPrefab) as GameObject;
			GameObjUtil.TrySetParent(gameObject, "/effects");
			AriseBehavior ariseBehavior = gameObject.GetComponent(typeof(AriseBehavior)) as AriseBehavior;
			ariseBehavior.mOnDie = (AriseBehavior.OnDie)Delegate.Combine(ariseBehavior.mOnDie, new AriseBehavior.OnDie(OnTextDie));
			ariseBehavior.mFontStyle = _style;
		}
		TextMesh textMesh = gameObject.GetComponent(typeof(TextMesh)) as TextMesh;
		Font font2 = (textMesh.font = mFonts[_style]);
		textMesh.renderer.material = font2.material;
		textMesh.renderer.material.color = mFontColors[_style];
		textMesh.text = _txt;
		gameObject.transform.position = _pos;
		return gameObject.GetComponent(typeof(AriseBehavior)) as AriseBehavior;
	}

	private void OnLoaded(bool _success, ILoadedAsset _asset, object _data)
	{
		if (_success)
		{
			mPrefab = _asset.Asset;
		}
	}

	public AriseBehavior Show(string _txt, GameObject _obj, Vector3 _offset, Style _style)
	{
		Vector3 position = _obj.transform.position;
		CapsuleCollider capsuleCollider = _obj.GetComponent(typeof(CapsuleCollider)) as CapsuleCollider;
		if (null != capsuleCollider)
		{
			position.y += capsuleCollider.height;
		}
		position += _offset;
		return Show(_txt, position, _style);
	}

	public AriseBehavior Show(string _txt, GameObject _obj)
	{
		return Show(_txt, _obj, Vector3.zero, Style.TEXT);
	}

	public AriseBehavior Show(string _txt, GameObject _obj, Style _style)
	{
		return Show(_txt, _obj, Vector3.zero, _style);
	}

	public AriseBehavior Show(string _txt, GameObject _obj, Vector3 _offset, Style _style, float _flySpeed, float _ttl)
	{
		AriseBehavior ariseBehavior = Show(_txt, _obj, _offset, _style);
		if (null == ariseBehavior)
		{
			return null;
		}
		ariseBehavior.flySpeed = _flySpeed;
		ariseBehavior.ttl = _ttl;
		return ariseBehavior;
	}

	private void OnTextDie(AriseBehavior _beh)
	{
		_beh.gameObject.SetActiveRecursively(state: false);
		mFree[_beh.mFontStyle].Enqueue(_beh.gameObject);
	}
}

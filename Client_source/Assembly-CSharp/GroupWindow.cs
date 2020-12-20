using System;
using System.Collections.Generic;
using TanatKernel;
using UnityEngine;

public class GroupWindow : GuiElement
{
	private class MemberView
	{
		public string mName;

		public Rect mNameRect;
	}

	private Group mGroup;

	private DateTime mLastReinitTime;

	private List<MemberView> mMembers = new List<MemberView>();

	private bool mIsValidState;

	private void ReinitMembers()
	{
		Uninit();
		float num = 60f;
		foreach (Group.Member member in mGroup.Members)
		{
			MemberView memberView = new MemberView();
			memberView.mName = member.Name;
			if (member.IsLeader)
			{
				memberView.mName += " (leader)";
			}
			if (!member.IsOnline)
			{
				memberView.mName += " (offline)";
			}
			memberView.mNameRect = new Rect(50f, num, 200f, 20f);
			GuiSystem.GetRectScaled(ref memberView.mNameRect);
			num += 20f;
			mMembers.Add(memberView);
		}
		mIsValidState = true;
		mLastReinitTime = DateTime.Now;
	}

	public override void Uninit()
	{
		mMembers.Clear();
	}

	public override void RenderElement()
	{
		foreach (MemberView mMember in mMembers)
		{
			GuiSystem.DrawString(mMember.mName, mMember.mNameRect, "middle_left");
		}
	}

	public override void Update()
	{
		if (mIsValidState && DateTime.Now.Subtract(mLastReinitTime).TotalSeconds > 10.0)
		{
			mIsValidState = false;
		}
		if (!mIsValidState)
		{
			ReinitMembers();
		}
	}

	private void OnGroupChanged(Group _group)
	{
		ReinitMembers();
	}

	public void SetData(Group _group)
	{
		if (_group == null)
		{
			throw new ArgumentNullException("_group");
		}
		mGroup = _group;
		mGroup.SubscribeChanged(OnGroupChanged);
		mIsValidState = false;
	}

	public void Clean()
	{
		if (mGroup != null)
		{
			mGroup.UnsubscribeChanged(OnGroupChanged);
			mGroup = null;
		}
	}
}

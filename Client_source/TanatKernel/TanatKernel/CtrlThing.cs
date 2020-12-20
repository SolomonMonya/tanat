using System;

namespace TanatKernel
{
	public class CtrlThing : Thing
	{
		private CtrlPrototype mProto;

		public override CtrlPrototype CtrlProto => mProto;

		public override PlaceType Place => mProto.Article.mTypeId switch
		{
			1 => PlaceType.HERO, 
			2 => PlaceType.AVATAR, 
			3 => PlaceType.QUEST, 
			_ => PlaceType.HERO, 
		};

		public CtrlThing(int _id, CtrlPrototype _proto)
			: base(_id)
		{
			if (_proto == null)
			{
				throw new ArgumentNullException("_proto");
			}
			if (_proto.Article == null)
			{
				throw new ArgumentNullException("_proto.Article");
			}
			mProto = _proto;
		}
	}
}

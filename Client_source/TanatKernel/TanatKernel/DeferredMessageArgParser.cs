using AMF;

namespace TanatKernel
{
	internal class DeferredMessageArgParser : CtrlPacketArgParser<DeferredMessageArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref DeferredMessageArg _arg)
		{
			MixedArray mixedArray = SafeGet<MixedArray>(_packet, "arguments");
			_arg.mType = SafeGet<int>(mixedArray, "type");
			if (mixedArray.Associative.ContainsKey("param1"))
			{
				_arg.mStrParameter = SafeGet<string>(mixedArray, "param1");
			}
			if (mixedArray.Associative.ContainsKey("param2"))
			{
				_arg.mIntParameter = SafeGet<int>(mixedArray, "param2");
			}
		}
	}
}

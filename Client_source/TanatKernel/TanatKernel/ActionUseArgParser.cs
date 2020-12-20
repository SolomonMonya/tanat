namespace TanatKernel
{
	internal class ActionUseArgParser : CtrlPacketArgParser<ActionUseArg>
	{
		protected override void ParseArg(CtrlPacket _packet, ref ActionUseArg _arg)
		{
			if (_packet.Arguments.Associative.Count > 1)
			{
				_arg.mConflictItemId = SafeGet(_packet, "conflict_item", -1);
				_arg.mCurrentItemId = SafeGet(_packet, "current_item", -1);
			}
		}
	}
}


CtrlName = {
	Prompt = "PromptCtrl",
	Message = "MessageCtrl"
}

--Э������--
ProtocalType = {
	BINARY = 0,
	PB_LUA = 1,
	PBC = 2,
	SPROTO = 3,
}
--��ǰʹ�õ�Э������--
TestProtoType = ProtocalType.BINARY;

Util = SimpleFramework.Util;
AppConst = SimpleFramework.AppConst;
LuaHelper = SimpleFramework.LuaHelper;
ByteBuffer = SimpleFramework.ByteBuffer;

ResManager = LuaHelper.GetResManager();
NetManager = LuaHelper.GetNetManager();
PanelManager = LuaHelper.GetPanelManager();
MusicManager = LuaHelper.GetMusicManager();
require "common/define"

require "Controller/BottomCtrl"


GameManager={}

local this = GameManager

function GameManager.LuaScriptPanel()
	return 'Bottom'
end

function GameManager.OnInitOK()
	AppConst.SocketPort = 2012;
    AppConst.SocketAddress = "127.0.0.1";
    NetManager:SendConnect();
	
	BottomCtrl.Awake()
end
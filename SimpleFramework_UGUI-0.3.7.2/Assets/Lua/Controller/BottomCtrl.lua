require "Common/define"

BottomCtrl={}
local this=BottomCtrl
local transform
local gameobject
local lua

function BottomCtrl.New()
	return this
end
function BottomCtrl.Awake()
	PanelManager:CreatePanel("Bottom",this.OnCreate)
end

function BottomCtrl.OnCreate(obj)
	gameobject=obj
	transform = gameobject.transfrom
	
	lua = gameobject:GetComponet("LuaBehaviour")
	lua:AddClick(BottomPanel.buttonSettings,this.OnButtonSettingsClick)
end

function BottomCtrl.OnButtonSettingsClick()
	
end

function BottomCtrl.OnButtonPeopleClick()
	
end
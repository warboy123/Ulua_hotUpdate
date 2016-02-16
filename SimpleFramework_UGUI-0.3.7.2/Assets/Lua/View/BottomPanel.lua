bottomPanel={}
local this =bottomPanel
local transfrom
local gameObject

function bottomPanel.Awake(obj)
	gameObject = obj
	transfrom = obj.transfrom
	
	this.InitPanel()
end

function bottomPanel.InitPanel()
	this.buttonSettings=transfrom:FindChild("ButtonSettings").gameObject
	this.buttonPeople=transfrom:FindChild("ButtonPeople").gameObject
end
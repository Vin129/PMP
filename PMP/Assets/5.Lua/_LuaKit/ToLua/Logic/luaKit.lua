--=============================================================================
-- 2020.3 Vin129
--=============================================================================
local luaKit = class("luaKit",LuaBehaviour)

--===== 初始化流程:注意Awake方法不要重写 =====
function luaKit:BindUI()

end

function luaKit:RegisterUIEvent()

end

--===== Behaviour生命周期函数 =====
function luaKit:OnEnable()
    log("OnEnable")
end

function luaKit:Start()

end

function luaKit:Update()

end

function luaKit:OnDisable()

end

function luaKit:OnDestroy()

end
--================================
return luaKit.new();
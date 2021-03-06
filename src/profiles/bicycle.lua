--[[
	Lua configuration file for the profiles used when routing, defines properties,
	preprocessing actions, routing behaviour and instruction generation.
--]]

name = "bicycle"
vehicle_types = { "vehicle", "bicycle" }

-- bicycle speeds
minspeed = 13
maxspeed = 13

speed_profile = {
	["primary"] = { speed = 15, access = true },
	["primary_link"] = { speed = 15, access = true },
	["secondary"] = { speed = 15, access = true },
	["secondary_link"] = { speed = 15, access = true },
	["tertiary"] = { speed = 15, access = true },
	["tertiary_link"] = { speed = 15, access = true },
	["unclassified"] = { speed = 15, access = true },
	["residential"] = { speed = 15, access = true },
	["service"] = { speed = 15, access = true },
	["services"] = { speed = 15, access = true },
	["road"] = { speed = 15, access = true },
	["track"] = { speed = 15, access = true },
	["cycleway"] = { speed = 15, access = true },
	["footway"] = { speed = 15, access = true },
	["pedestrian"] = { speed = 15, access = true },
	["path"] = { speed = 15, access = true },
	["living_street"] = { speed = 15, access = true },
	["ferry"] = { speed = 15, access = true },
	["movable"] = { speed = 15, access = true },
	["shuttle_train"] = { speed = 15, access = true },
  	["default"] = { speed = 15, access = true }
}

access_values = {
	["private"] = false,
	["yes"] = true,
	["no"] = false,
	["permissive"] = true,
	["destination"] = true,
	["customers"] = false,
	["designated"] = true,
	["public"] = true,
	["delivery"] = true,
	["use_sidepath"] = false
}

-- Properties to add to the metadata in the routerdb
profile_whitelist = {
	"highway",
	"oneway",
	"bicycle",
	"vehicle",
	"access",
	"maxspeed",
	"roundabout",
	"cycleway",
    "cycleway:left",
    "cycleway:right",
	"cyclenetwork",
	"brussels",
	"oneway:bicycle",
	"operator",
	"cycleref",
	"cyclecolour",
    "surface",
    "railway",
    "parking:lane:both",
    "parking:lane:right"
}

-- Tags of the osm data to add to the metadata in the routerdb
meta_whitelist = {
	"name",
	"bridge",
	"tunnel",
	"colour",
	"ref"
}

-- The different profiles
profiles = {
	{
		name = "",
		function_name = "factor_and_speed",
		metric = "time"
	},
	{
		name = "shortest",
		function_name = "factor_and_speed",
		metric = "distance"
	},
	{
		name = "balanced",
		function_name = "factor_and_speed_balanced",
		metric = "custom"
	},
--[[	{
		name = "networks",
		function_name = "factor_and_speed_networks",
		metric = "custom"
	},
--]]
	{
		name = "brussels",
		function_name = "factor_and_speed_networks_brussels",
		metric = "custom"
	},
    {
        name = "relaxed",
        function_name = "factor_and_speed_relaxed",
        metric = "custom"
    }
}

-- Processes relation and adds the attributes_to_keep to the child ways for use in routing
function relation_tag_processor (attributes, result)
	result.attributes_to_keep = {}
	if attributes.network == "lcn" then
		result.attributes_to_keep.lcn = "yes"
	end
	if attributes.network == "rcn" then
		result.attributes_to_keep.rcn = "yes"
	end
	if (attributes.network == "lcn" or attributes.network == "rcn") and
		attributes.operator == "Brussels Mobility" then
		result.attributes_to_keep.brussels = "yes"
	end
	if attributes.colour ~= nil and result.attributes_to_keep.brussels == "yes" then
		result.attributes_to_keep.cyclecolour = attributes.colour
	end
	if attributes.ref ~= nil and result.attributes_to_keep.brussels == "yes" then
		result.attributes_to_keep.cycleref = attributes.ref
	end
	if attributes.type == "route" and
	   attributes.route == "bicycle" then
		result.attributes_to_keep.cyclenetwork = "yes"
	end
end

-- processes node and adds the attributes to keep to the vertex meta collection in the routerdb.
function node_tag_processor (attributes, results)
	if attributes.rcn_ref then
		results.attributes_to_keep = {
			rcn_ref = attributes.rcn_ref
		}
	end
end

function can_access (attributes, result)
	local last_access = nil
	local access = access_values[attributes.access]
	if access ~= nil then
		result.attributes_to_keep.access = true
		last_access = access
	end
	for i=0, 10 do
		local access_key_key = vehicle_types[i]
		local access_key = attributes[access_key_key]
		if access_key then
			access = access_values[access_key]
			if access ~= nil then
				result.attributes_to_keep[access_key_key] = true
				last_access = access
			end
		end
	end
	return last_access
end

-- turns a oneway tag value into a direction
function is_oneway (attributes, name)
	local oneway = attributes[name]
	if oneway ~= nil then
		if oneway == "yes" or
		   oneway == "true" or
		   oneway == "1" then
			return 1
		end
		if oneway == "-1" then
			return 2
		end
		if oneway == "no" then
			return 0
		end
	end
	return nil
end

function factor_and_speed (attributes, result)
	 local highway = attributes.highway

	 result.speed = 0
	 result.direction = 0
	 result.canstop = true
	 result.attributes_to_keep = {}

	 -- set highway to ferry when ferry.
	 local route = attributes.route;
	 if route == "ferry" then
		highway = "ferry"
		result.attributes_to_keep.route = highway
	 end

	 local highway_speed = speed_profile[highway]
	 if highway_speed then
        result.speed = highway_speed.speed
		result.access = highway_speed.access
        result.direction = 0
		result.canstop = true
		result.attributes_to_keep.highway = highway
	 else
		return
	 end

	 local access = can_access (attributes, result)
	 if access ~= nil then
		result.access = access
	 end

	 if result.access then
	 else
		result.speed = 0
		result.direction = 0
		result.canstop = true
		return
	 end

	-- get directional information
	local junction = attributes.junction
	if junction == "roundabout" then
		result.direction = 1
		result.attributes_to_keep.junction = true
	end
	local direction = is_oneway (attributes, "oneway")
	if direction ~= nil then
		result.direction = direction
		result.attributes_to_keep.oneway = true
	end
	direction = is_oneway (attributes, "oneway:bicycle")
	if direction ~= nil then
		result.direction = direction
		result.attributes_to_keep["oneway:bicycle"] = true
	end
end

highest_avoid_factor = 0.8
avoid_factor = 0.9
prefer_factor = 1.1
highest_prefer_factor = 1.2

-- multiplication factors per classification (balanced)
bicycle_balanced_factors = {
	["primary"] = highest_avoid_factor,
	["primary_link"] = highest_avoid_factor,
	["secondary"] = avoid_factor,
	["secondary_link"] = avoid_factor,
	["tertiary"] = 1,
	["tertiary_link"] = 1,
	["residential"] = 1,
	["path"] = prefer_factor,
	["cycleway"] = prefer_factor,
	["footway"] = prefer_factor,
	["pedestrian"] = avoid_factor,
	["steps"] = avoid_factor
}

bicycle_balanced_factors_cycleway = {
    ["lane"] = prefer_factor,
    ["track"] = prefer_factor,
    ["shared_lane"] = prefer_factor,
    ["opposite_lane"] = prefer_factor,
    ["share_busway"] = prefer_factor,
    ["opposite_track"] = prefer_factor,
    ["opposite"] = prefer_factor
}

-- the factor function for the factor profile
function factor_and_speed_balanced (attributes, result)

	factor_and_speed (attributes, result)

	if result.speed == 0 then
		return
	end

	result.factor = 1.0 / (result.speed / 3.6)
	local balanced_factor = bicycle_balanced_factors[attributes.highway]
	if balanced_factor ~= nil then
		result.factor = result.factor / balanced_factor
	else
		balanced_factor = 1
	end
-- considers the cycleway key and its tag weights
    local cycleway_factor = bicycle_balanced_factors_cycleway[attributes["cycleway"]]
    if cycleway_factor ~= nil then
        balanced_factor = balanced_factor * cycleway_factor
    end
-- considers the cycleway:left and cycleway:right keys together and the cycleway tag weights
    local cycleway_left_factor = bicycle_balanced_factors_cycleway[attributes["cycleway:left"]]
    local cycleway_right_factor = bicycle_balanced_factors_cycleway[attributes["cycleway:right"]]
    if cycleway_left_factor ~= nil and cycleway_right_factor ~= nil then
        balanced_factor = balanced_factor * (cycleway_right_factor - 0.1) * (cycleway_left_factor - 0.1)
    end

end

-- multiplication factors per classification (relaxed)
bicycle_relaxed_factors_highway = {
    ["primary"] = highest_avoid_factor,
    ["primary_link"] = highest_avoid_factor,
    ["secondary"] = highest_avoid_factor,
    ["secondary_link"] = highest_avoid_factor,
    ["tertiary"] = avoid_factor,
    ["tertiary_link"] = avoid_factor,
    ["residential"] = 1,
    ["path"] = highest_prefer_factor,
    ["cycleway"] = highest_prefer_factor,
    ["footway"] = highest_prefer_factor,
    ["pedestrian"] = 1,
    ["steps"] = 1,
    ["track"] = 1,
    ["living_street"] = 1
}

bicycle_relaxed_factors_cycleway = {
    ["lane"] = prefer_factor,
    ["track"] = highest_prefer_factor,
    ["shared_lane"] = prefer_factor,
    ["opposite_lane"] = prefer_factor,
    ["share_busway"] = highest_prefer_factor,
    ["opposite_track"] = highest_prefer_factor,
    ["opposite"] = prefer_factor
}

bicycle_relaxed_factors_surface = {
    ["paving_stones"] = avoid_factor,
    ["sett"] = avoid_factor,
    ["cobblestone"] = avoid_factor,
    ["gravel"] = avoid_factor,
    ["pebblestone"] = avoid_factor,
    ["unpaved"] = avoid_factor,
    ["ground"] = avoid_factor,
    ["dirt"] = highest_avoid_factor,
    ["grass"] = highest_avoid_factor,
    ["sand"] = highest_avoid_factor,
    ["metal"] = 1,
    ["paved"] = 1,
    ["asphalt"] = 1,
    ["concrete"] = 1,
    ["fine_gravel"] = 1,
    ["compacted"] = 1
}

bicycle_relaxed_factors_parking = {
    ["parallel"] = avoid_factor
}

--[[
bicycle_relaxed_factors_bicycle = {
    ["yes"] = prefer_factor,
    ["no"] = 0
}
--]]
-- the factor function for the factor profile (relaxed)
function factor_and_speed_relaxed (attributes, result)

    factor_and_speed (attributes, result)

    if result.speed == 0 then
        return
    end
-- considers the highway key and its tag weights
    result.factor = 1.0 / (result.speed / 3.6)
    local relaxed_factor = bicycle_relaxed_factors_highway[attributes.highway]
    if relaxed_factor == nil then
        relaxed_factor = 1;
    end
-- considers the surface key and its tag weights
    local surface_factor = bicycle_relaxed_factors_surface[attributes.surface]
    if surface_factor ~= nil then
        relaxed_factor = relaxed_factor * surface_factor
    end
-- considers the parking:lane:both key and its tag weights
    local parking_factor = bicycle_relaxed_factors_parking[attributes["parking:lane:both"]]
    if parking_factor ~= nil then
        relaxed_factor = relaxed_factor * parking_factor
    end
-- considers the cycleway key and its tag weights
    local cycleway_factor = bicycle_relaxed_factors_cycleway[attributes["cycleway"]]
    if cycleway_factor ~= nil then
        relaxed_factor = relaxed_factor * cycleway_factor
    end
-- considers the cycleway:left and cycleway:right keys together and the cycleway tag weights
    local cycleway_left_factor = bicycle_relaxed_factors_cycleway[attributes["cycleway:left"]]
    local cycleway_right_factor = bicycle_relaxed_factors_cycleway[attributes["cycleway:right"]]
    if cycleway_left_factor ~= nil and cycleway_right_factor ~= nil then
        relaxed_factor = relaxed_factor * (cycleway_right_factor - 0.1) * (cycleway_left_factor - 0.1)
    end

    result.factor = result.factor / relaxed_factor

end


--[[
function factor_and_speed_networks (attributes, result)

	factor_and_speed_balanced (attributes, result)

	if result.speed == 0 then
		return
	end

	if attributes.cyclenetwork then
		result.factor = result.factor / 3
	end

end
--]]
--[[
	Function to calculate the factor of an edge in the graph when routing.
	If the edge is part of the brussels mobility network, favor it by a factor of 3.
--]]
function factor_and_speed_networks_brussels (attributes, result)
	factor_and_speed_balanced(attributes, result)
	if result.speed == 0 then
		return
	end

	if attributes.brussels then
		result.factor = result.factor / 3
	end
end

-- instruction generators
instruction_generators = {
	{
		applies_to = "", -- applies to all profiles when empty
		generators = {
			{
				name = "start",
				function_name = "get_start"
			},
			{
				name = "stop",
				function_name = "get_stop"
			},
			{
				name = "roundabout",
				function_name = "get_roundabout"
			},
			{
				name = "turn",
				function_name = "get_turn"
			}
		}
	}
}

-- gets the first instruction
function get_start (route_position, language_reference, instruction)
	if route_position.is_first() then
		local direction = route_position.direction()
		instruction.text = itinero.format(language_reference.get("Start {0}."), language_reference.get(direction));
		instruction.shape = route_position.shape
		return 1
	end
	return 0
end

-- gets the last instruction
function get_stop (route_position, language_reference, instruction)
	if route_position.is_last() then
		instruction.text = language_reference.get("Arrived at destination.");
		instruction.shape = route_position.shape
		return 1
	end
	return 0
end

function contains (attributes, key, value)
	if attributes then
		return localvalue == attributes[key];
	end
end

-- gets a roundabout instruction
function get_roundabout (route_position, language_reference, instruction)
	if route_position.attributes.junction == "roundabout" and
		(not route_position.is_last()) then
		local attributes = route_position.next().attributes
		if attributes.junction then
		else
			local exit = 1
			local count = 1
			local previous = route_position.previous()
			while previous and previous.attributes.junction == "roundabout" do
				local branches = previous.branches
				if branches then
					branches = branches.get_traversable()
					if branches.count > 0 then
						exit = exit + 1
					end
				end
				count = count + 1
				previous = previous.previous()
			end

			instruction.text = itinero.format(language_reference.get("Take the {0}th exit at the next roundabout."), "" .. exit)
			if exit == 1 then
				instruction.text = itinero.format(language_reference.get("Take the first exit at the next roundabout."))
			elseif exit == 2 then
				instruction.text = itinero.format(language_reference.get("Take the second exit at the next roundabout."))
			elseif exit == 3 then
				instruction.text = itinero.format(language_reference.get("Take the third exit at the next roundabout."))
			end
			instruction.type = "roundabout"
			instruction.shape = route_position.shape
			return count
		end
	end
	return 0
end

-- gets a turn
function get_turn (route_position, language_reference, instruction)
	local relative_direction = route_position.relative_direction().direction

	local turn_relevant = false
	local branches = route_position.branches
	if branches then
		branches = branches.get_traversable()
		if relative_direction == "straighton" and
			branches.count >= 2 then
			turn_relevant = true -- straight on at cross road
		end
		if  relative_direction != "straighton" and
			branches.count > 0 then
			turn_relevant = true -- an actual normal turn
		end
	end

	if relative_direction == "unknown" then
		turn_relevant = false -- turn could not be calculated.
	end

	if turn_relevant then
		local next = route_position.next()
		local name = nil
		if next then
			name = next.attributes.name
		end
		if name then
			instruction.text = itinero.format(language_reference.get("Go {0} on {1}."),
				language_reference.get(relative_direction), name)
			instruction.shape = route_position.shape
		else
			instruction.text = itinero.format(language_reference.get("Go {0}."),
				language_reference.get(relative_direction))
			instruction.shape = route_position.shape
		end

		return 1
	end
	return 0
end

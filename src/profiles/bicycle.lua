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
	["footway"] = { speed = 15, access = false },
	["pedestrian"] = { speed = 15, access = false },
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
	"cyclenetwork",
	"brussels",
	"oneway:bicycle",
	"operator",
	"cycleref",
	"cyclecolour"
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
	{ 
		name = "networks",
		function_name = "factor_and_speed_networks",
		metric = "custom"
	},
	{ 
		name = "brussels",
		function_name = "factor_and_speed_networks_brussels",
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
	if attributes.colour != nil and result.attributes_to_keep.brussels == "yes" then
		result.attributes_to_keep.cyclecolour = attributes.colour
	end
	if attributes.ref != nil and result.attributes_to_keep.brussels == "yes" then
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
	if access != nil then
		result.attributes_to_keep.access = true
		last_access = access
	end
	for i=0, 10 do
		local access_key_key = vehicle_types[i]
		local access_key = attributes[access_key_key]
		if access_key then
			access = access_values[access_key]
			if access != nil then
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
	if oneway != nil then
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
	 if access != nil then
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
	if direction != nil then
		result.direction = direction
		result.attributes_to_keep.oneway = true
	end
	direction = is_oneway (attributes, "oneway:bicycle")
	if direction != nil then
		result.direction = direction
		result.attributes_to_keep["oneway:bicycle"] = true
	end
end

highest_avoid_factor = 0.8
avoid_factor = 0.9
prefer_factor = 1.1
highest_prefer_factor = 1.2

-- multiplication factors per classification
bicycle_balanced_factors = {
	["primary"] = highest_avoid_factor,
	["primary_link"] = highest_avoid_factor,
	["secondary"] = highest_avoid_factor,
	["secondary_link"] = highest_avoid_factor,
	["tertiary"] = avoid_factor,
	["tertiary_link"] = avoid_factor,
	["residential"] = 1,
	["path"] = highest_prefer_factor,
	["cycleway"] = highest_prefer_factor,
	["footway"] = prefer_factor,
	["pedestrian"] = prefer_factor,
	["steps"] = prefer_factor
}

-- the factor function for the factor profile
function factor_and_speed_balanced (attributes, result)

	factor_and_speed (attributes, result)

	if result.speed == 0 then
		return
	end

	result.factor = 1.0 / (result.speed / 3.6)
	local balanced_factor = bicycle_balanced_factors[attributes.highway]
	if balanced_factor != nil then
		result.factor = result.factor / balanced_factor
	end

end

function factor_and_speed_networks (attributes, result)
	
	factor_and_speed_balanced (attributes, result)

	if result.speed == 0 then
		return
	end

	if attributes.cyclenetwork then
		result.factor = result.factor / 3
	end

end

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

--[[
	Generates an instrutions every time the reference of the road changes and
	every time you leave or enter the cyclenetwork.
--]]
function get_turn (route_position, language_reference, instruction) 
	local relative_direction = route_position.relative_direction().direction

	local turn_relevant = false
	local branches = route_position.branches

	local ref = route_position.attributes.cycleref
	local cyclenetwork = route_position.attributes.brussels
	local next_cyclenetwork = nil
	local next_ref;
	local next = route_position.next()
	
	if next then
		 next_cyclenetwork = next.attributes.brussels
		 next_ref = next.attributes.cycleref
	end

	-- Checks if an instruction has to be generated.
	if branches and cyclenetwork and next_cyclenetwork and ref ~= next_ref then
		turn_relevant = true
	end
	if branches then
		if cyclenetwork and not next_cyclenetwork then
			turn_relevant = true
		elseif not cyclenetwork and next_cyclenetwork then
			turn_relevant = true
		end	
	end

	-- Set the properties of the instruction
	if turn_relevant then
		local name = nil
		
		if next then
			name = next.attributes.name
		end
		if cyclenetwork then 
			if next_cyclenetwork then
				if next_ref then
					instruction.text = itinero.format(language_reference.get("Go {0} on the {1} route."), 
					language_reference.get(relative_direction), next_ref)
				else
					instruction.text = itinero.format(language_reference.get("Go {0}."), 
					language_reference.get(relative_direction))
				end
				
			else
				if name then
					instruction.text = itinero.format(language_reference.get("Go {0} and leave the cyclenetwork on {1}."), 
					language_reference.get(relative_direction), name)
				else
					instruction.text = itinero.format(language_reference.get("Go {0} and leave the cyclenetwork."), 
					language_reference.get(relative_direction))
				end
				
			end
		else
			if next_cyclenetwork then
				if next_ref then
					instruction.text = itinero.format(language_reference.get("Go {0} and enter the cyclenetwork on the {1} route."), 
					language_reference.get(relative_direction), next_ref)
				else
					instruction.text = itinero.format(language_reference.get("Go {0} and enter the cyclenetwork."), 
					language_reference.get(relative_direction))
				end
				
			else
				if name then
					instruction.text = itinero.format(language_reference.get("Go {0} on {1}."), 
						language_reference.get(relative_direction), name)
				else
					instruction.text = itinero.format(language_reference.get("Go {0}."), 
						language_reference.get(relative_direction))
				end
			end
		end
		instruction.shape = route_position.shape

		return 1
	end
	return 0
end	
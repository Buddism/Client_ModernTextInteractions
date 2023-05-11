package modernTextInteractions
{
	function GuiTextEditCtrl::onWake(%this)
	{
		//newChatHud_AddLine("imAwake[" @ $modernTextInteractions::smNumAwake + 1 @ "]" SPC %this.getName());

		if($typeControl == 0 || $typeControl.getGroup() != %this.getGroup())
			$typeControl = %this;
		//newChatHud_AddLine("typeControl: "@ %this.getName() SPC %this);

		//echo("ENTERING [", $modernTextInteractions::smNumAwake @"]", %this.getName());
		$modernTextInteractions::textInputQueue[$modernTextInteractions::smNumAwake] = %this;
		$modernTextInteractions::smNumAwake++;
		parent::onWake(%this);
	}
	function GuiTextEditCtrl::onSleep(%this)
	{
		//newChatHud_AddLine("imSleep[" @ $modernTextInteractions::smNumAwake - 1 @"]" SPC %this.getName());

		if($modernTextInteractions::smNumAwake <= 1)
		{
			$typeControl = 0;
			//newChatHud_AddLine("typeControl: "@ 0);
		} else {
			$typeControl = $modernTextInteractions::textInputQueue[$modernTextInteractions::smNumAwake - 2];
			//newChatHud_AddLine("typeControl: "@ $typeControl.getName() SPC $typeControl);
		}

		//echo("EXITING [", $modernTextInteractions::smNumAwake @"]", %this.getName());
		$modernTextInteractions::textInputQueue[$modernTextInteractions::smNumAwake] = "";
		$modernTextInteractions::smNumAwake--;
		parent::onSleep(%this);
	}
};

activatePackage(modernTextInteractions);
resetAllOpCallFunc();

//if(!isFunction("GuiTextEditCtrl", "onSleep"))
	eval("function GuiTextEditCtrl::onSleep(%this){}");
//if(!isFunction("GuiTextEditCtrl", "onWake" ))
	eval("function GuiTextEditCtrl::onWake(%this) {}");

if($Pref::ModernText::charList $= "")
	$Pref::ModernText::charList = "abcdefghijklmnopqrstuvwxyz" @ "ABCDEFGHIJKLMNOPQRSTUVWXYZ" @ "0123456789";

GlobalActionMap.bind(keyboard, "ctrl backspace", action_backspace);
GlobalActionMap.bind(keyboard, "ctrl delete", action_delete);
GlobalActionMap.bind(keyboard, "ctrl left", action_leftShift);
GlobalActionMap.bind(keyboard, "ctrl right", action_rightShift);
//GlobalActionMap.bind(keyboard, "ctrl-shift left", action_leftShift); //no text selection functions available

function action_handleRepeat(%val, %repeatCallback)
{
	cancel($actionRepeat[%repeatCallback]);
	switch(%val)
	{
		case 2:
			$actionRepeat[%repeatCallback] = schedule(25, 0, call, %repeatCallback, 2);

		case 1:
			$actionRepeat[%repeatCallback] = schedule(500, 0, call, %repeatCallback, 2);

		case 0:
			cancel($actionRepeat[%repeatCallback]);
	}
}
function action_leftShift(%val)
{
	action_handleRepeat(%val, "action_leftShift");
	if(!%val || !isObject($typeControl) || !isFunction($typeControl.getClassName(), "getCursorPos"))
		return;

	%value = $typeControl.getValue();
	%pos = $typeControl.getCursorPos();

	if(%value $= "" || %pos == 0)
		return;

	%charList = $Pref::ModernText::charList;
	%lastChar = getSubStr(%value, %pos - 1, 1);
	%antiAlphabet = strpos(%charList, %lastChar) == -1;
	%skipWhitespace = (getSubStr(%value, %pos - 1, 1) $= " ");

	for(%I = %pos - 1; %I >= 0; %I--)
	{
		%cChar = getSubStr(%value, %I, 1);
		if(%cChar $= " ")
		{
			if(%skipWhitespace)
				continue;
			else {
				%I++;
				break;
			}
		}
		%skipWhitespace = false;

		if(%antiAlphabet && strpos(%charList, %cChar) != -1)
		{
			if(%pos - %i > 1)
			{
				//ignore current char
				%i++;
				break;
			}
			%antiAlphabet = 0;
		}

		if(!%antiAlphabet && strpos(%charList, %cChar) == -1)
		{
			//echo("break at " @ %cChar TAB %i);
			//ignore current char
			%i++;
			break;
		}
	}

	%i = getMax(%i, 0);
	$typeControl.setCursorPos(%i);
}

function action_rightShift(%val)
{
	action_handleRepeat(%val, "action_rightShift");
	if(!%val || !isObject($typeControl) || !isFunction($typeControl.getClassName(), "getCursorPos"))
		return;

	%value = $typeControl.getValue();
	%pos = $typeControl.getCursorPos();
	%length = strLen(%value);

	if(%value $= "" || %pos == %length)
		return;

	%charList = $Pref::ModernText::charList;
	%lastChar = getSubStr(%value, %pos, 1);
	%antiAlphabet = strpos(%charList, %lastChar) == -1;
	%skipWhitespace = (getSubStr(%value, %pos, 1) $= " ");

	for(%I = %pos; %I <= %length; %I++)
	{
		%cChar = getSubStr(%value, %I, 1);

		if(%cChar $= " ")
		{
			if(%skipWhitespace)
				continue;
			else break;
		}
		%skipWhitespace = false;

		if(%antiAlphabet && strpos(%charList, %cChar) != -1)
		{
			if(%pos - %i < 0)
			{
				break;
			}
			%antiAlphabet = 0;
		}

		if(!%antiAlphabet && strpos(%charList, %cChar) == -1)
		{
			break;
		}
	}

	$typeControl.setCursorPos(%i);
}


function setSubStr(%string,%start,%end,%value)
{
	return getSubStr(%string, 0, %start) @ %value @ getSubStr(%string, %start + %end, strLen(%string));
}

function action_delete(%val)
{
	action_handleRepeat(%val, "action_delete");
	if(!%val || !isObject($typeControl) || !isFunction($typeControl.getClassName(), "getCursorPos"))
		return;

	%value = $typeControl.getValue();
	%valueLength = strlen(%value);
	%pos = $typeControl.getCursorPos();

	if(%valueLength == 0 || %pos == %valueLength)
		return;

	%charList = $Pref::ModernText::charList;
	%curChar = getSubStr(%value, %pos, 1);
	%antiAlphabet = strpos(%charList, %curChar) == -1;
	%deleteWhitespace = %curChar $= " ";

	for(%I = %pos; %I <= %valueLength; %I++)
	{
		%nextChar = getSubStr(%value, %I, 1);

		if(%nextChar $= " ")
		{
			if(%deleteWhitespace)
				continue;
			else
				break;
		} else if(%deleteWhitespace)
			break;

		if(%antiAlphabet && strpos(%charList, %nextChar) != -1)
		{
			if(%pos - %i < 0)
				break;

			%antiAlphabet = 0;
		}

		if(!%antiAlphabet && strpos(%charList, %nextChar) == -1)
			break;
	}

	$typeControl.setValue(setSubStr(%value, %pos, %i - %pos, ""));
	$typeControl.setCursorPos(%pos);
	if($typeControl.command !$= "")
		eval($typeControl.command);
}


function action_backspace(%val)
{
	action_handleRepeat(%val, "action_backspace");
	if(!%val || !isObject($typeControl) || !isFunction($typeControl.getClassName(), "getCursorPos"))
		return;

	%value = $typeControl.getValue();
	%pos = $typeControl.getCursorPos();

	if(%value $= "" || %pos == 0)
		return;

	%charList = $Pref::ModernText::charList;
	%lastChar = getSubStr(%value, %pos - 1, 1);
	%antiAlphabet = strpos(%charList, %lastChar) == -1;
	%deleteWhitespace = (getSubStr(%value, getMax(%pos - 1, 0), 1) $= " ");

	for(%I = %pos - 1; %I >= 0; %I--)
	{
		%cChar = getSubStr(%value, %I, 1);

		if(%cChar $= " ")
		{
			if(%deleteWhitespace)
				continue;
			else {
				%I++;
				break;
			}
		} else if(%deleteWhitespace)
		{
			%I++;
			break;
		}

		if(%antiAlphabet && strpos(%charList, %cChar) != -1)
		{
			if(%pos - %i > 1)
			{
				//ignore current char
				%i++;
				break;
			}
			%antiAlphabet = 0;
		}

		if(!%antiAlphabet && strpos(%charList, %cChar) == -1)
		{
			//echo("break at " @ %cChar TAB %i);
			//ignore current char
			%i++;
			break;
		}
	}

	%i = getMax(%i, 0);
	$typeControl.setValue(setSubStr(%value, %i, %pos - %i, ""));
	if($typeControl.command !$= "")
		eval($typeControl.command);
		
	if(%pos != strLen(%value))
		$typeControl.setCursorPos(%i);
}

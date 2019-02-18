$Server::MIDIRecAddress = "127.0.0.1:27069";
$Server::MIDIRec::Connected = false;

function initMIDIRecConnection() {
	if(!isObject(MIDIRecTCPObject)) {
		new TCPObject(MIDIRecTCPObject);
	} else {
		MIDIRecTCPObject.disconnect();
	}

	%obj = MIDIRecTCPObject;
	%obj.connect($Server::MIDIRecAddress);
}
initMIDIRecConnection();

function MIDIRecTCPObject::onConnected(%this) {
	cancel($MIDIRecConnectRetryLoop);

	echo("Connected to the MIDIRec server.");
	MIDIRecTCPObject.send("connect\r\n");
}

function MIDIRecTCPObject::onConnectFailed(%this) {
	cancel($MIDIRecConnectRetryLoop);
	echo("Trying to connect to the MIDIRec server again (failed to connect)...");
	$Server::MIDIRec::Connected = false;
	$MIDIRecConnectRetryLoop = %this.schedule(1000, connect, $Server::MIDIRecAddress);
}

function MIDIRecTCPObject::onDisconnect(%this) {
	cancel($MIDIRecConnectRetryLoop);
	echo("Trying to connect to the MIDIRec server again (disconnected)...");
	$Server::MIDIRec::Connected = false;
	$MIDIRecConnectRetryLoop = %this.schedule(1000, connect, $Server::MIDIRecAddress);
}

function MIDIRecTCPObject::onLine(%this, %line) {
	%line = trim(%line);
	echo("\c4[RECV]\c0" SPC %line);
	
	// if you host this to the outside world (WHICH YOU HAVE TO RECONFIGURE TO MAKE HAPPEN), don't come complaining to me when your server inevitably crashes.
	// i am WELL AWARE of the security risks here, leave it on 127.0.0.1 and stick to trying to crash yourself thanks
	%cmd = getField(%line, 0);
	call("_MIDINote_", %line);
}

function createCoolMIDIGrid() {
	%notes = "C\tCs\tD\tDs\tE\tF\tFs\tG\tGs\tA\tAs\tB";
	BrickGroup_999999.deleteAll();
	%x = 0;
	%z = 0;

	for(%channel = 0; %channel < 16; %channel++) {
		for(%note = 0; %note < 128; %note++) {
			%octave = mFloor(%note / 12);

			//%x = (%channel * 12) + %octave;
			//%z = (%note % 12);

			%x = ((%channel * 12) + %octave) % 48;
			%z = (mFloor(%channel / 4) * 12) + (%note % 12);		

			%name = "_MIDIRec_Ch" @ %channel @ "_" @ getField(%notes, %note % 12) @ %octave;
			%brick = new fxDTSBrick() {
				angleID = 0;
				colorFxID = 0;
				shapeFxID = 0;
				colorID = %channel;
				oColor = %channel;
				dataBlock = "brick1x1Data";
				position = %x * 0.5 SPC 0 SPC (%z * 0.6) + 0.3;
				rotation = "1 0 0 0";
				isPlanted = 1;
				scale = "1 1 1";
				stackBL_ID = -1;
			};
			if(!isObject(%brick)) {
				warn("Failed to create brick" SPC %brick);
				return;
			}
			%brick.setNTObjectName(%name);

			BrickGroup_999999.add(%brick);
			%brick.plant();
			%brick.setTrusted(1);
			BrickGroup_999999.addNTName(%name);
		}
	}
}

function _MIDINote_(%fields) {
	%note = getField(%fields, 0);
	%channel = getField(%fields, 1);
	%toggle = getField(%fields, 2);

	%brick = "_MIDIRec_Ch" @ %channel @ "_" @ %note;
	%brick.setColor(%toggle ? 16 : %brick.oColor);
	%brick.setColorFX(%toggle ? 3 : 0);
}
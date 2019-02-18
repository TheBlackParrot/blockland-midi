const net = require("net");
var emidi = require("easymidi");
var input = new emidi.Input("LoopBe Internal MIDI 2");

function parseNote(note) {
	let notes = ["C", "Cs", "D", "Ds", "E", "F", "Fs", "G", "Gs", "A", "As", "B"];
	let octave = Math.floor(note/12);

	return `${notes[note % 12]}${octave}`;
}
function broadcast(what) {
	TCPClients.forEach(function(socket) {
		socket.write(`${what.join("\t")}\r\n`);
	});
}
input.on('noteon', function(msg) {
	let note = parseNote(msg.note);
	console.log(`[MIDI] ${note} on ${msg.channel}`);
	broadcast([note, msg.channel, 1]);
});
input.on('noteoff', function(msg) {
	let note = parseNote(msg.note);
	console.log(`[MIDI] ${note} off ${msg.channel}`);
	broadcast([note, msg.channel, 0]);
});

var TCPClients = [];
net.createServer(function(socket) {
	TCPClients.push(socket);
	socket.write("OK\r\n");

	socket.on("error", function(err) {
		TCPClients.splice(TCPClients.indexOf(socket), 1);
	});

	socket.on("end", function(err) {
		TCPClients.splice(TCPClients.indexOf(socket), 1);
	});
}).listen(27069, "127.0.0.1");
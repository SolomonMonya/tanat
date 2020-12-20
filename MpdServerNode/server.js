var net = require('net');
const libamf = require('libamf');
const fs = require('fs');
const bufferReverse = require('buffer-reverse');
const io = require('socket.io')(7777);

io.on('connection', socket => {
    console.log('new client');
});

// io.listen(7777);
// var server = net.createServer(function (connection) {
//     console.log('client connected');

//     connection.on('end', function () {
//         console.log('client disconnected');
//     });

//     connection.on('data', (data) => {
//         // const amfData = libamf.deserialize(data, libamf.ENCODING.AMF3);
//         // console.log(data.toString(''));
//         const reversed = bufferReverse(data);
//         console.log(reversed.toString())
//         // const amfData = libamf.deserialize(Buffer.from(data.toString(), 'utf-8'), libamf.ENCODING.AMF3);
//         // console.log(amfData);
//         // fs.writeFileSync('test555.amf', amfData);
//     });
// });
// server.listen(7777, function () {
//     console.log('server is listening');
// });
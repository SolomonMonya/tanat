var net = require('net');
const fs = require('fs');
var Decoder = require('node-amf3').Decoder;
const { decode } = require('punycode');
const amf = require('amf');

var server = net.createServer(function (connection) {
    console.log('client connected');

    connection.on('end', function () {
        console.log('client disconnected');
    });

    connection.on('data', function (data) {
        // var test_data = new Buffer('03 00 03 66 6f 6f 02 00 03 62 61 72 00 00 09'.replace(/ /g, ''), 'hex');
        console.log(data.toString())
        // const test = amf.read(data, 0);
        // console.log(test);
        // console.log(data.toString());
        // const decoder = new Decoder(data);
        // const result = decoder.decode();

        fs.writeFileSync("test.amf", data);
        // console.log(result);
    });
    connection.write('Hello World!\r\n');
    connection.pipe(connection);
    // connection.end()
});
server.listen(7777, function () {
    console.log('server is listening');
});
const express = require('express')
const bodyParser = require('body-parser');
const fs = require('fs');
const amf = require('amf');
const amfDeSerializer = require('@jscad/amf-deserializer')
const libamf = require('libamf');

const app = express()
const port = 7776;

app.use(bodyParser.urlencoded({ extended: true }));

app.post('/entry_point.php', (req, res) => {
    for (let data in req.body) {
        const amfData = libamf.deserialize(Buffer.from(data, 'utf8'), libamf.ENCODING.AMF3);
        const object = amfData.get('object');
        const action = amfData.get('action');

        switch (action) {
            case 'login': {
                // console.log(amfData);
                const sess_uid = amfData.get('sess_uid');
                const sess_key = amfData.get('sess_key');
                const counter = amfData.get('counter');
                const params = amfData.get('params');
                const email = params.get('email');
                const passwd = params.get('passwd');
                const version = params.get('version');

                // login etc

                let response = new Map();
                let arguments = new Map();

                response.set('user|login', arguments);
                
                arguments.set('status', 100);
                arguments.set('error', -123);
                arguments.set('id', 1);
                arguments.set('username', 'dailycode');
                arguments.set('sess_key', 'dailycode_0');
                arguments.set('flags', 1);
                


                const encodedAmf = libamf.serialize(response, libamf.ENCODING.AMF3);


                let responseHero = new Map();
                let argumentsHero = new Map();
                let heroCreate = new Map();

                responseHero.set('common|hero_conf', argumentsHero);
                
                argumentsHero.set('status', 100);
                argumentsHero.set('error', -1);
                argumentsHero.set('load', heroCreate);
                heroCreate.set('id', 1);
                heroCreate.set('race', 1);
                heroCreate.set('gender', true);
                heroCreate.set('face', 1);
                heroCreate.set('hair', 1);
                heroCreate.set('dist_mark', 1);
                heroCreate.set('skin_color', 1);
                heroCreate.set('hair_color', 1);

                const encodedAmfHero = libamf.serialize(responseHero, libamf.ENCODING.AMF3);


                let mpdConnection = new Map();
                let mpdConnectionArgs = new Map();

                mpdConnection.set('chat|conf', mpdConnectionArgs);
                mpdConnectionArgs.set('status', 100);
                mpdConnectionArgs.set('error', -1);
                mpdConnectionArgs.set('chat_server_host', '127.0.0.1');
                mpdConnectionArgs.set('chat_server_port', [7777]);
                mpdConnectionArgs.set('chat_server_uid', 1);
                mpdConnectionArgs.set('chat_server_sid', 'dailycode_0');

                const encodedAmfMpdConnection = libamf.serialize(mpdConnection, libamf.ENCODING.AMF3);

                res.write(encodedAmf);
                res.write(encodedAmfHero);
                res.write(encodedAmfMpdConnection);
                res.end();
                break;
            }
            case 'create': {
                let response = new Map();
                let arguments = new Map();

                response.set('hero|create', arguments);
                
                arguments.set('status', 100);
                arguments.set('error', -1);

                const encodedAmf = libamf.serialize(response, libamf.ENCODING.AMF3);

                res.send(encodedAmf)
                break;
            }
            case 'get_global_buffs': {
                let response = new Map();
                let arguments = new Map();

                response.set('user|get_global_buffs', arguments);
                arguments.set('status', 100);
                arguments.set('error', -1);

                const encodedAmf = libamf.serialize(response, libamf.ENCODING.AMF3);

                res.send(encodedAmf);
                break;
            }
            case 'can_reconnect': {
                let response = new Map();
                let arguments = new Map();

                response.set('common|can_reconnect', arguments);
                arguments.set('status', 100);
                arguments.set('error', -1);
                arguments.set('answer', false);
                arguments.set('timer', parseFloat('4f'));

                const encodedAmf = libamf.serialize(response, libamf.ENCODING.AMF3);
                res.send(encodedAmf);

                break;
            }
            default: {
                // console.log(amfData);
                console.log(data);
                // fs.writeFileSync('test.amf', data);
                break;
            }
        }
    }
})

app.listen(port, () => {
    console.log(`Example app listening at http://localhost:${port}`)
})
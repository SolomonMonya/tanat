const express = require('express')
const app = express()
const port = 7776;

app.get('/entry_point.php', (req, res) => {
    console.log(req.body);
    res.send('Hello World!');
})

app.listen(port, () => {
    console.log(`Example app listening at http://localhost:${port}`)
})
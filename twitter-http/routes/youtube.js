var express = require('express');
var router = express.Router();
const asyncErrors = require('express-async-errors');
const rp = require('request-promise');

const youtube = require('../Crawlers/社交/youtube');

/* GET home page. */
router.get('/', function(req, res, next) {
    res.render('youtube', { title: 'youtube-demo', version: 'v0.0.1' });
});

//爬关键字
router.post('/search/', async function(req, res) {
    var t = new youtube(req.body, req.body.token);
    await t.startCollect();
    await t.startParse();
    // var downloadMsg = await Download(Date.now(), req.body.token);
    res.send(t.parseData);
    // res.send(downloadMsg);
});

async function Download(unixTime, keyWord) {
    const options = {
        method: 'POST',
        uri: 'http://localhost:5000/Twitter/Download?unixTime=' + unixTime + '&keyWord=' + encodeURIComponent(keyWord),
        json: true
    };
    try {
        return await rp(options);
    } catch (error) {
        return '开启下载失败：' + error.message;
    }
}

module.exports = router;
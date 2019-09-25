const fs = require('fs');
const path = require('path');
const os = require('os');
const mkdirp = require('mkdirp');
const moment = require('moment');
const helper = require('./helper');

const mkDirAsync = helper.promisify(mkdirp);
const writeFileAsync = helper.promisify(fs.writeFile);

class Logger {
    constructor(token) {
        this.token = token; //一次抓取任务的token
        this.directory = './logs/' + moment().format('YYYYMMDD') + '/' + this.token;
        this.logFile = path.join(this.directory, 'log.log');
        this.Initialized = false;
    }

    static deleteOldDirs() {
        // 当前日期减去一个月，以此时间为界限，之前的都删了
        const dateLine = moment().subtract(1, 'months');
        const logDirs = fs.readdirSync('./logs/')
        for (let i = 0, len = logDirs.length; i < len; i++) {
            const fullPath = path.join('./logs/', logDirs[i]);
            if (fs.lstatSync(fullPath).isDirectory() && moment(logDirs[i], "YYYYMMDD").diff(dateLine) < 0) {
                helper.deleteFolderRecursive(fullPath)
            }
        }
    }

    async init() {
        await mkDirAsync(this.directory);
    }

    async log(data) {
        if (!this.Initialized) {
            await this.init();
            this.Initialized = true;
        }

        if (data instanceof Error) {
            data = data.message + data.stack;
        }
        fs.appendFile(this.logFile,
            '[' + new Date().toISOString() + '] ' + data + os.EOL,
            err => console.error(err));
    }

    async logHtml(html, fileName) {
        if (!this.Initialized) {
            await this.init();
            this.Initialized = true;
        }

        if (fileName === undefined) {
            fileName = moment().format('mmssSSS');
		}
        await writeFileAsync(this.directory + '/' + fileName + '.html', html);
    }
}

module.exports = Logger;
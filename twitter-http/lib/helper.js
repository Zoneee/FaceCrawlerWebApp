const rp = require('request-promise');
const fs = require('fs');
const p = require("path")

class Helper {
    static promisify(nodeFunction, thisArg) {
        function promisified(...args) {
            return new Promise((resolve, reject) => {
                function callback(err, ...result) {
                    if (err)
                        return reject(err);
                    if (result.length === 1)
                        return resolve(result[0]);
                    return resolve(result);
                }
                nodeFunction.call(thisArg, ...args, callback);
            });
        }
        return promisified;
    }

    static deleteFolderRecursive(path) {
        if (fs.existsSync(path)) {
            fs.readdirSync(path).forEach(function (file, index) {
                var curPath = p.join(path, file);
                if (fs.lstatSync(curPath).isDirectory()) { // recurse
                    Helper.deleteFolderRecursive(curPath);
                } else { // delete file
                    fs.unlinkSync(curPath);
                }
            });
            fs.rmdirSync(path);
        }
    }

    static async recognizeCaptcha(imageBase64, imageName) {
        const options = {
            method: 'POST',
            uri: 'https://nmd-ai.juxinli.com/ocr_captcha',
            body: {
                image_base64: imageBase64,
                image_name: imageName,
                app_id: 'juxinli'
            },
            json: true
        };
        const response = await rp(options);
        if (response.errorcode === 0) {
            return response.string;
        }
        else {
            throw new Error("识别失败" + response.errormsg);
        }
    }
}

module.exports = Helper;
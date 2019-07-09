import * as fs from 'fs';
import * as nconf from 'nconf';

export class DataProvider {

    /**
     * This method is used to read data from Json file using language and dataname filters
     *
     * @author Pavani
     * @param dataProviderFile
     * @param dataName
     * @returns result object
     */
    static getJsonData(dataProviderFile: string, dataName: string) {
        let data = null, result = null;
        data = JSON.parse(fs.readFileSync(dataProviderFile, 'utf8'));
        result = data[dataName];
        return result;
    }


    /**
     * This method is used to write Json data to Json file using language and dataname
     * filters with the help of key & value pair.
     *
     * @author Pavani
     * @param dataProviderFile
     * @param dataName
     * @param key
     * @param value
     */
    writeDataProvider(dataProviderFile, dataName, key, value) {
        nconf.argv()
            .env()
            .file({file: dataProviderFile});

        nconf.set(dataName + ':' + key, value);


        nconf.save(function (err) {
            fs.readFile(dataProviderFile, 'utf-8', function (err, data) {
                if (err) throw err;
            });
        });
    };
}

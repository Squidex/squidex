import * as fs from 'fs';

export class DataProvider {

    /**
     * This method is used to read data from Json file using language and dataname filters
     *
     * @author Pavani
     * @param dataProviderFile
     * @param dataName
     * @returns result object
     */
    public static getJsonData(dataProviderFile: string, dataName: string) {
        let data = null, result = null;
        data = JSON.parse(fs.readFileSync(dataProviderFile, 'utf8'));
        result = data[dataName];
        return result;
    }

    /*

    /
     * This method is used to write Json data to Json file using language and dataname
     * filters with the help of key & value pair.
     *
     * @author Pavani
     * @param dataProviderFile
     * @param dataName
     * @param key
     * @param value
     *
    public writeDataProvider(dataProviderFile: nconf.IFileOptions.file, dataName, key, value) {
        nconf.argv()
            .env()
            .file({ file: dataProviderFile });

        nconf.set(dataName + ':' + key, value);


        nconf.save(() => {
            fs.readFile(dataProviderFile, 'utf-8', (err2) => {
                if (err2) {
                    throw err2;
                }
            });
        });
    }
    */
}

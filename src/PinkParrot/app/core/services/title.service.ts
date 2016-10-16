/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class TitlesConfig {
    constructor(public readonly value: { [key: string]: string }) { }
}

export const TitleServiceFactory = (titles: TitlesConfig) => {
    return new TitleService(titles);
};

export class TitleService {
    constructor(private readonly titles: TitlesConfig) { }

    public setTitle(key: string, parameters: { [key: string]: string }) {
        let title = this.titles.value[key] || '';

        if (!title) {
            return;
        }

        for (let parameter in parameters) {
            if (parameters.hasOwnProperty(parameter)) {
                title = title.replace(`{${parameter}}`, parameters[parameter]);
            }
        }

        document.title = title;
    }
}
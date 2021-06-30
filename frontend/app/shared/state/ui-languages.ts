/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

type UILanguage = { iso2Code: string; localName: string };

export module UILanguages {
    export const ALL: ReadonlyArray<UILanguage> = [{
        iso2Code: 'en',
        localName: 'English',
    }, {
        iso2Code: 'nl',
        localName: 'Nederlands',
    }, {
        iso2Code: 'it',
        localName: 'Italiano',
    }, {
        iso2Code: 'zh',
        localName: '简体中文',
    }];
}

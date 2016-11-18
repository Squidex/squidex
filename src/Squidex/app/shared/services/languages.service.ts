/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import { Observable } from 'rxjs';

import { ApiUrlConfig } from 'framework';

import { AuthService } from './auth.service';

export class Language {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string
    ) {
    }
}

@Ng2.Injectable()
export class LanguageService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(): Observable<Language[]> {
        return this.authService.authGet(this.apiUrl.buildUrl('api/languages'))
                .map(response => {                    
                    const body: any[] = response.json();

                    return body.map(item => {
                        return new Language(
                            item.iso2Code, 
                            item.englishName);
                    });
                });
    }
}
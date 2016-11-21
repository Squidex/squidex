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
import { LanguageDto } from './languages.service';

@Ng2.Injectable()
export class AppLanguagesService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(appName: string): Observable<LanguageDto[]> {
        return this.authService.authGet(this.apiUrl.buildUrl(`api/apps/${appName}/languages`))
                .map(response => {                    
                    const body: any[] = response.json();

                    return body.map(item => {
                        return new LanguageDto(
                            item.iso2Code, 
                            item.englishName);
                    });
                });
    }

    public postLanguages(appName: string, languageCodes: string[]): Observable<any> {
        return this.authService.authPost(this.apiUrl.buildUrl(`api/apps/${appName}/languages`), { languages: languageCodes });
    }
}
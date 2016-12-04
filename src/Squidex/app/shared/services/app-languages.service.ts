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

import { handleError } from './errors';

export class AppLanguageDto {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string,
        public isMasterLanguage: boolean
    ) {
    }
}

@Ng2.Injectable()
export class AppLanguagesService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(appName: string): Observable<AppLanguageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {                    
                    const items: any[] = response;

                    return items.map(item => {
                        return new AppLanguageDto(
                            item.iso2Code, 
                            item.englishName,
                            item.isMasterLanguage === true);
                    });
                })
                .catch(response => handleError('Failed to load languages. Please reload', response));
    }

    public postLanguages(appName: string, languageCode: string): Observable<AppLanguageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages`);

        return this.authService.authPost(url, { language: languageCode })
                .map(response => response.json())
                .map(response => {               
                    return new AppLanguageDto(
                        response.iso2Code, 
                        response.englishName,
                        response.isMasterLanguage === true);
                })
                .catch(response => handleError('Failed to add language. Please reload.', response));
    }

    public makeMasterLanguage(appName: string, languageCode: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return this.authService.authPut(url, { isMasterLanguage: true })
                .catch(response => handleError('Failed to change language. Please reload.', response));
    } 

    public deleteLanguage(appName: string, languageCode: string): Observable<AppLanguageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/languages/${languageCode}`);

        return this.authService.authDelete(url)
                .catch(response => handleError('Failed to add language. Please reload.', response));
    }
}
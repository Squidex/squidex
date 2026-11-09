/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, pretifyError } from '@app/framework';
import { ScriptLogsDto } from './../model';

@Service()
export class ScriptLogsService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public getScriptLogs(appName: string, name?: string, take = 100): Observable<ScriptLogsDto> {
        let url = `api/apps/${appName}/script-logs?take=${take}`;

        if (name) {
            url += `&name=${encodeURIComponent(name)}`;
        }

        return this.http.get<any>(this.apiUrl.buildUrl(url)).pipe(
            map(body => {
                return ScriptLogsDto.fromJSON(body);
            }),
            pretifyError('i18n:scriptLogs.loadFailed'));
    }
}

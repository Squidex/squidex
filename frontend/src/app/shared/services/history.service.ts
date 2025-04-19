/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom, from, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, escapeHTML, pretifyError, StringHelper } from '@app/framework';
import { HistoryEventDto } from './../model';
import { UsersProviderService } from './users-provider.service';

export function formatHistoryMessage(message: string, users: UsersProviderService): Observable<string> {
    async function getUserName(id: string): Promise<string> {
        const user = await firstValueFrom(users.getUser(id, null));

        return user.displayName;
    }

    async function format(message: string): Promise<string> {
        const placeholderMatches = message.matchAll(/{(?<type>[^\s:]*):(?<id>[^}]*)}/g) || [];
        const placeholderValues: string[] = [];

        for (const match of placeholderMatches) {
            const { id, type } = match.groups!;

            if (type !== 'user') {
                placeholderValues.push(id);
                continue;
            }

            const parts = id.split(':');

            if (parts.length === 1) {
                placeholderValues.push(await getUserName(parts[0]));
            } else if (parts[0] === 'subject') {
                placeholderValues.push(await getUserName(parts[1]));
            } else if (parts[1].toLowerCase().endsWith('client')) {
                placeholderValues.push(parts[1]);
            } else {
                placeholderValues.push(`${parts[1]}-client`);
            }
        }

        message = message.replace(/{([^\s:]*):([^}]*)}/, () => {
            return `<span class="user-ref">${escapeHTML(placeholderValues.shift() || '')}</span>`;
        });

        message = message.replace(/{([^}]*)}/g, (match: string, marker: string) => {
            return `<span class="marker-ref">${escapeHTML(marker)}</span>`;
        });

        return message;
    }

    return from(format(message));
}

@Injectable({
    providedIn: 'root',
})
export class HistoryService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getHistory(appName: string, channel: string): Observable<ReadonlyArray<HistoryEventDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/history${StringHelper.buildQuery({ channel })}`);

        const options = {
            headers: new HttpHeaders({
                'X-Silent': '1',
            }),
        };

        return this.http.get<any[]>(url, options).pipe(
            map(body => {
                return body.map(HistoryEventDto.fromJSON);
            }),
            pretifyError('i18n:history.loadFailed'));
    }

    public getHistoryForTeam(teamId: string, channel: string): Observable<ReadonlyArray<HistoryEventDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/history?channel=${channel}`);

        const options = {
            headers: new HttpHeaders({
                'X-Silent': '1',
            }),
        };

        return this.http.get<any[]>(url, options).pipe(
            map(body => {
                return body.map(HistoryEventDto.fromJSON);
            }),
            pretifyError('i18n:history.loadFailed'));
    }
}
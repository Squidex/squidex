/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { computed, inject, Service, Signal } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, escapeHTML, pretifyError, StringHelper } from '@app/framework';
import { HistoryEventDto } from './../model';
import { UsersProviderService } from './users-provider.service';

export function formatHistoryMessage(message: string, users: UsersProviderService): Signal<string | null> {
    return computed(() => {
        const placeholderMatches = message.matchAll(/{(?<type>[^\s:]*):(?<id>[^}]*)}/g) || [];
        const placeholderValues: string[] = [];

        for (const match of placeholderMatches) {
            const { id, type } = match.groups!;

            if (type !== 'user') {
                placeholderValues.push(id);
                continue;
            }

            const parts = id.split(':');

            if (parts.length === 1 || parts[0] === 'subject') {
                const user = users.user(parts.length === 1 ? parts[0] : parts[1], null)();
                if (!user) {
                    // Wait until all users are loaded.
                    return null;
                }

                placeholderValues.push(user.displayName);
            } else if (parts[1].toLowerCase().endsWith('client')) {
                placeholderValues.push(parts[1]);
            } else {
                placeholderValues.push(`${parts[1]}-client`);
            }
        }

        let result = message.replace(/{([^\s:]*):([^}]*)}/, () => {
            return `<span class="user-ref">${escapeHTML(placeholderValues.shift() || '')}</span>`;
        });

        result = result.replace(/{([^}]*)}/g, (match: string, marker: string) => {
            return `<span class="marker-ref">${escapeHTML(marker)}</span>`;
        });

        return result;
    });
}

@Service()
export class HistoryService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

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
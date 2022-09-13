/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, DateTime, pretifyError, Version } from '@app/framework';
import { UsersProviderService } from './users-provider.service';

export class HistoryEventDto {
    constructor(
        public readonly eventId: string,
        public readonly actor: string,
        public readonly eventType: string,
        public readonly message: string,
        public readonly created: DateTime,
        public readonly version: Version,
    ) {
    }
}

const REPLACEMENT_TEMP = '$TEMP$';

export function formatHistoryMessage(message: string, users: UsersProviderService): Observable<string> {
    const userName = (userId: string) => {
        const parts = userId.split(':');

        if (parts.length === 1) {
            return users.getUser(parts[0], null).pipe(map(u => u.displayName));
        } else if (parts[0] === 'subject') {
            return users.getUser(parts[1], null).pipe(map(u => u.displayName));
        } else if (parts[1].endsWith('client')) {
            return of(parts[1]);
        } else {
            return of(`${parts[1]}-client`);
        }
    };

    let foundUserId: string | null = null;

    message = message.replace(/{([^\s:]*):([^}]*)}/, (match: string, type: string, id: string) => {
        if (type === 'user') {
            foundUserId = id;
            return REPLACEMENT_TEMP;
        } else {
            return id;
        }
    });

    message = message.replace(/{([^}]*)}/g, (match: string, marker: string) => {
        return `<span class="marker-ref">${marker}</span>`;
    });

    if (foundUserId) {
        return userName(foundUserId).pipe(map(t => message.replace(REPLACEMENT_TEMP, `<span class="user-ref">${t}</span>`)));
    }

    return of(message);
}

@Injectable()
export class HistoryService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getHistory(appName: string, channel: string): Observable<ReadonlyArray<HistoryEventDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/history?channel=${channel}`);

        const options = {
            headers: new HttpHeaders({
                'X-Silent': '1',
            }),
        };

        return this.http.get<any[]>(url, options).pipe(
            map(body => {
                return parseHistoryEvents(body);
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
                return parseHistoryEvents(body);
            }),
            pretifyError('i18n:history.loadFailed'));
    }
}

function parseHistoryEvents(response: any[]) {
    return response.map(parseHistoryEvent);
}

function parseHistoryEvent(response: any) {
    return new HistoryEventDto(
        response.eventId,
        response.actor,
        response.eventType,
        response.message,
        DateTime.parseISO(response.created),
        new Version(response.version.toString()));
}

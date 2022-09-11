/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export class StockPhotoDto {
    constructor(
        public readonly url: string,
        public readonly thumbUrl: string,
        public readonly user: string,
        public readonly userProfileUrl: string,
    ) {
    }
}

@Injectable()
export class StockPhotoService {
    constructor(
        private readonly http: HttpClient,
    ) {
    }

    public getImages(query: string, page = 1): Observable<ReadonlyArray<StockPhotoDto>> {
        const url = `https://stockphoto.squidex.io/?query=${query}&page=${page}`;

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return parseImages(body);
            }),
            catchError(() => of([])));
    }
}
function parseImages(response: any[]) {
    return response.map(parseImage);
}

function parseImage(response: any) {
    return new StockPhotoDto(
        response.url,
        response.thumbUrl,
        response.user,
        response.userProfileUrl);
}


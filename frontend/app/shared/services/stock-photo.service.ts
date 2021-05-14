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

    public getImages(query: string): Observable<ReadonlyArray<StockPhotoDto>> {
        const url = `https://stockphoto.squidex.io/?query=${query}&pageSize=100`;

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(x =>
                    new StockPhotoDto(
                        x.url,
                        x.thumbUrl,
                        x.user,
                        x.userProfileUrl,
                    ));
            }),
            catchError(() => of([])));
    }
}

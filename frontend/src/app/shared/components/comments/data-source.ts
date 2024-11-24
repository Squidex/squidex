/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { map, Observable, of } from 'rxjs';
import { AutocompleteSource } from '@app/framework';
import { ContributorsState } from '@app/shared/internal';

@Injectable()
export class ContributorsDataSource implements AutocompleteSource {
    constructor(
        private readonly contributorsState: ContributorsState,
    ) {
    }

    public loadIfNotLoaded() {
        this.contributorsState.loadIfNotLoaded();
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of([]);
        }

        return this.contributorsState.contributors.pipe(
            map(contributors => contributors.filter(c => c.contributorEmail.indexOf(query) >= 0)),
        );
    }
}
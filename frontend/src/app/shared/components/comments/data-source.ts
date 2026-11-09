/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { Observable, of } from 'rxjs';
import { AutocompleteSource } from '@app/framework';
import { ContributorsState } from '@app/shared/internal';

@Service({ autoProvided: false })
export class ContributorsDataSource implements AutocompleteSource {
    private readonly contributorsState = inject(ContributorsState);

    public loadIfNotLoaded() {
        this.contributorsState.loadIfNotLoaded();
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of([]);
        }

        return of(this.contributorsState.contributors().filter(c => c.contributorEmail.indexOf(query) >= 0));
    }
}
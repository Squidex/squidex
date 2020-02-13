/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Injectable, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    AppsState,
    AutocompleteComponent,
    AutocompleteSource,
    SearchResultDto,
    SearchService,
    Types
} from '@app/shared/internal';

@Injectable()
export class SearchSource implements AutocompleteSource {
    public selectedAppOrNull = this.appsState.selectedAppOrNull;

    constructor(
        private readonly appsState: AppsState,
        private readonly searchService: SearchService
    ) {
    }

    public find(query: string): Observable<readonly any[]> {
        return this.searchService.getResults(this.appsState.appName, query);
    }
}

@Component({
    selector: 'sqx-search-menu',
    styleUrls: ['./search-menu.component.scss'],
    templateUrl: './search-menu.component.html',
    providers: [
        SearchSource
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchMenuComponent {
    @ViewChild(AutocompleteComponent, { static: false })
    public searchControl: AutocompleteComponent;

    public selection: SearchResultDto;

    constructor(
        private readonly router: Router,
        private readonly apiUrl: ApiUrlConfig,
        public readonly searchSource: SearchSource
    ) {
    }

    public selectResult(result: SearchResultDto) {
        if (Types.is(result, SearchResultDto)) {
            const relativeUrl = result.url.substr(this.apiUrl.value.length);

            this.router.navigateByUrl(relativeUrl);

            this.searchControl.reset();
        }
    }
}
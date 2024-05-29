/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Injectable, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { ApiUrlConfig, AppsState, AutocompleteComponent, AutocompleteSource, SearchResultDto, SearchService, ShortcutComponent, ShortcutDirective, TooltipDirective, TranslatePipe, Types } from '@app/shared';
import { AutocompleteComponent as AutocompleteComponent_1 } from '../../../framework/angular/forms/editors/autocomplete.component';

@Injectable()
export class SearchSource implements AutocompleteSource {
    public selectedApp = this.appsState.selectedApp;

    constructor(
        private readonly appsState: AppsState,
        private readonly searchService: SearchService,
    ) {
    }

    public find(query: string): Observable<readonly any[]> {
        if (!query || query.length < 3) {
            return of([]);
        }

        return this.searchService.getResults(this.appsState.appName, query);
    }
}

@Component({
    standalone: true,
    selector: 'sqx-search-menu',
    styleUrls: ['./search-menu.component.scss'],
    templateUrl: './search-menu.component.html',
    providers: [
        SearchSource,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        AutocompleteComponent_1,
        FormsModule,
        ShortcutComponent,
        ShortcutDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class SearchMenuComponent {
    @ViewChild(AutocompleteComponent, { static: false })
    public searchControl!: AutocompleteComponent;

    public searchResult?: SearchResultDto;

    constructor(
        private readonly router: Router,
        private readonly apiUrl: ApiUrlConfig,
        public readonly searchSource: SearchSource,
    ) {
    }

    public selectResult(result: SearchResultDto) {
        if (Types.is(result, SearchResultDto)) {
            const relativeUrl = result.url.substring(this.apiUrl.value.length);

            this.router.navigateByUrl(relativeUrl);

            this.searchControl.reset();
        }
    }
}

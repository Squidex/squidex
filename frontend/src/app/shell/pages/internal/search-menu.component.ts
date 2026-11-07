/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Injectable, viewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { ApiUrlConfig, AppsState, AutocompleteComponent, AutocompleteSource, SearchResultDto, SearchService, ShortcutComponent, ShortcutDirective, TooltipDirective, TranslatePipe, Types } from '@app/shared';
import { AutocompleteComponent as AutocompleteComponent_1 } from '../../../framework/angular/forms/editors/autocomplete.component';

@Injectable()
export class SearchSource implements AutocompleteSource {
    private readonly appsState = inject(AppsState);
    private readonly searchService = inject(SearchService);

    public readonly selectedApp = this.appsState.selectedApp;

    public find(query: string): Observable<readonly any[]> {
        if (!query || query.length < 3) {
            return of([]);
        }

        return this.searchService.getResults(this.appsState.appName, query);
    }
}

@Component({
    selector: 'sqx-search-menu',
    styleUrls: ['./search-menu.component.scss'],
    templateUrl: './search-menu.component.html',
    providers: [
        SearchSource,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AutocompleteComponent_1,
        FormsModule,
        ShortcutComponent,
        ShortcutDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class SearchMenuComponent {
    private readonly router = inject(Router);
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly searchControl = viewChild(AutocompleteComponent);

    protected readonly searchSource = inject(SearchSource);

    protected readonly searchResult?: SearchResultDto;

    protected selectResult(result: SearchResultDto) {
        if (Types.is(result, SearchResultDto)) {
            const relativeUrl = result.url.substring(this.apiUrl.value.length);

            this.router.navigateByUrl(relativeUrl);

            this.searchControl()?.reset();
        }
    }
}

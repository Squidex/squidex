/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AppsState,
    AssetsState,
    Queries,
    UIState
} from '@app/shared';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html'
})
export class AssetsPageComponent implements OnInit {
    public assetsFilter = new FormControl();

    public queries = new Queries(this.uiState, 'assets');

    constructor(
        public readonly appsState: AppsState,
        public readonly assetsState: AssetsState,
        private readonly uiState: UIState
    ) {
    }

    public ngOnInit() {
        this.assetsState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.assetsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public search(query: string) {
        this.assetsState.search(query).pipe(onErrorResumeNext()).subscribe();
    }

    public toggleTag(tag: string) {
        this.assetsState.toggleTag(tag).pipe(onErrorResumeNext()).subscribe();
    }

    public resetTags() {
        this.assetsState.resetTags().pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.assetsState.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.assetsState.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public isSelectedQuery(query: string) {
        return query === this.assetsState.snapshot.assetsQuery || (!query && !this.assetsState.assetsQuery);
    }
}


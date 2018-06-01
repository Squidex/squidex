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

import { AppsState, AssetsState } from '@app/shared';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html'
})
export class AssetsPageComponent implements OnInit {
    public assetsFilter = new FormControl();

    constructor(
        public readonly appsState: AppsState,
        public readonly assetsState: AssetsState
    ) {
    }

    public ngOnInit() {
        this.assetsState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.assetsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public search() {
        this.assetsState.search(this.assetsFilter.value).pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.assetsState.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.assetsState.goPrev().pipe(onErrorResumeNext()).subscribe();
    }
}


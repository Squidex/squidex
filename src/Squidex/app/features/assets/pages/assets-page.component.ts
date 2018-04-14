/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

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
        this.load();
    }

    public load(notify = false) {
        this.assetsState.loadAssets(notify).subscribe();

    }

    public search() {
        this.assetsState.search(this.assetsFilter.value).subscribe();
    }

    public goNext() {
        this.assetsState.goNext().subscribe();
    }

    public goPrev() {
        this.assetsState.goPrev().subscribe();
    }
}


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppsState,
    ContributorDto,
    ContributorsState,
    DialogModel,
    RolesState
} from '@app/shared';

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html'
})
export class ContributorsPageComponent implements OnInit {
    public importDialog = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly contributorsState: ContributorsState,
        public readonly rolesState: RolesState
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.contributorsState.load();
    }

    public reload() {
        this.contributorsState.load(true);
    }

    public goPrev() {
        this.contributorsState.goPrev();
    }

    public goNext() {
        this.contributorsState.goNext();
    }

    public search(query: string) {
        this.contributorsState.search(query);
    }

    public trackByContributor(contributor: ContributorDto) {
        return contributor.contributorId;
    }
}

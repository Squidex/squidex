/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ContributorDto, ContributorsState, DialogModel, RolesState, Router2State } from '@app/shared';

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html',
    providers: [
        Router2State
    ]
})
export class ContributorsPageComponent implements OnInit {
    public importDialog = new DialogModel();

    constructor(
        public readonly contributorsRoute: Router2State,
        public readonly contributorsState: ContributorsState,
        public readonly rolesState: RolesState
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.contributorsState.loadAndListen(this.contributorsRoute);
    }

    public reload() {
        this.contributorsState.load(true);
    }

    public search(query: string) {
        this.contributorsState.search(query);
    }

    public trackByContributor(_index: number, contributor: ContributorDto) {
        return contributor.contributorId;
    }
}

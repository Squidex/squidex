/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { TeamContributorsState } from '@app/features/teams/internal';
import { ContributorDto, Router2State } from '@app/shared';

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html',
    providers: [
        Router2State,
    ],
})
export class ContributorsPageComponent implements OnInit {
    constructor(
        public readonly contributorsRoute: Router2State,
        public readonly contributorsState: TeamContributorsState,
    ) {
    }

    public ngOnInit() {
        const initial =
            this.contributorsRoute.mapTo(this.contributorsState)
                .withPaging('contributors', 10)
                .withString('query')
                .getInitial();

        this.contributorsState.load(false, initial);
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

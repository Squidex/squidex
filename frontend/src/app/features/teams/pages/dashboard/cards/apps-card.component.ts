/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { AppDto, AppsService, StatefulComponent, TeamDto } from '@app/shared';

interface State {
    // The apps for this team.
    apps?: ReadonlyArray<AppDto>;
}

@Component({
    selector: 'sqx-apps-card',
    styleUrls: ['./apps-card.component.scss'],
    templateUrl: './apps-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppsCardComponent extends StatefulComponent<State> implements OnInit {
    @Input({ required: true })
    public team!: TeamDto;

    constructor(
        private readonly appsService: AppsService,
    ) {
        super({});
    }

    public ngOnInit() {
        this.appsService.getTeamApps(this.team.id)
            .subscribe(apps => {
                this.next({ apps });
            });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppDto, AppsService, StatefulComponent, StopClickDirective, TeamDto, TranslatePipe } from '@app/shared';

interface State {
    // The apps for this team.
    apps?: ReadonlyArray<AppDto>;
}

@Component({
    standalone: true,
    selector: 'sqx-apps-card',
    styleUrls: ['./apps-card.component.scss'],
    templateUrl: './apps-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RouterLink,
        StopClickDirective,
        TranslatePipe,
    ],
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

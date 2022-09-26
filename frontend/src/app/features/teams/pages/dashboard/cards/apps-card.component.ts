/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { AppDto, AppsService, StatefulComponent, TeamDto } from '@app/shared';

interface State {
    // The apps for this team.
    apps?: ReadonlyArray<AppDto>;
}

@Component({
    selector: 'sqx-apps-card[team]',
    styleUrls: ['./apps-card.component.scss'],
    templateUrl: './apps-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppsCardComponent extends StatefulComponent<State> implements OnInit {
    @Input()
    public team!: TeamDto;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsService: AppsService,
    ) {
        super(changeDetector, {});
    }

    public ngOnInit() {
        this.appsService.getTeamApps(this.team.id)
            .subscribe(apps => {
                this.next({ apps });
            });
    }
}

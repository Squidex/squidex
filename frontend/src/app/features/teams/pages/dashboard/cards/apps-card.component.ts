/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AppsService, StopClickDirective, TeamDto, TranslatePipe } from '@app/shared';

@Component({
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
export class AppsCardComponent {
    private readonly appsService = inject(AppsService);

    /** The team to show the apps for. */
    public readonly team = input.required<TeamDto>();

    protected readonly apps = rxResource({
        params: () => ({ team: this.team() }),
        stream: ({ params }) => this.appsService.getTeamApps(params.team.id),
    });
}

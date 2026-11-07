/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { switchMap } from 'rxjs/operators';
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
    /** The team to show the apps for. */
    public readonly team = input.required<TeamDto>();

    protected readonly apps;

    constructor() {
        const appsService = inject(AppsService);

        this.apps = toSignal(
            toObservable(this.team).pipe(
                switchMap(team => appsService.getTeamApps(team.id))));
    }
}

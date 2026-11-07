/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
import { AppFormComponent, AppsState, DialogModel, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, TeamFormComponent, TeamsState, TemplatesState, TitleService, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AppFormComponent,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        RouterLinkActive,
        TeamFormComponent,
        TranslatePipe,
    ],
})
export class AppsMenuComponent {
    private readonly templatesState = inject(TemplatesState);
    private readonly titleService = inject(TitleService);

    protected readonly appsState = inject(AppsState);

    protected readonly route = inject(ActivatedRoute);

    protected readonly teamsState = inject(TeamsState);

    protected readonly uiState = inject(UIState);

    protected readonly appPath = computed(() => {
        return this.titleService.path().slice(1);
    });

    protected readonly addAppDialog = new DialogModel();

    protected readonly addTeamDialog = new DialogModel();

    protected readonly appsMenu = new ModalModel();

    protected readonly starters = this.templatesState.starters;
}

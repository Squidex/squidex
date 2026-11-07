/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed } from '@angular/core';
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
    protected readonly addAppDialog = new DialogModel();
    protected readonly addTeamDialog = new DialogModel();

    protected readonly appsMenu = new ModalModel();

    protected readonly appPath = computed(() => {
        return this.titleService.path().slice(1);
    });

    protected readonly starters = this.templatesState.starters;

    constructor(
        private readonly titleService: TitleService,
        protected readonly appsState: AppsState,
        protected readonly route: ActivatedRoute,
        protected readonly teamsState: TeamsState,
        protected readonly uiState: UIState,
        private readonly templatesState: TemplatesState,
    ) {
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppDto, AppFormComponent, AppsState, DialogModel, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, TeamDto, TeamFormComponent, TeamsState, Title, TitleService, TranslatePipe, UIState } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AppFormComponent,
        AsyncPipe,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        NgFor,
        NgIf,
        RouterLink,
        RouterLinkActive,
        TeamFormComponent,
        TranslatePipe,
    ],
})
export class AppsMenuComponent {
    public addAppDialog = new DialogModel();
    public addTeamDialog = new DialogModel();

    public appsMenu = new ModalModel();
    public appPath: Observable<ReadonlyArray<Title>>;

    constructor(titleService: TitleService,
        public readonly appsState: AppsState,
        public readonly route: ActivatedRoute,
        public readonly teamsState: TeamsState,
        public readonly uiState: UIState,
    ) {
        this.appPath = titleService.pathChanges.pipe(map(x => x.slice(1)));
    }

    public trackByApp(_index: number, app: AppDto) {
        return app.id;
    }

    public trackByTeam(_index: number, team: TeamDto) {
        return team.id;
    }

    public trackByTitle(_index: number, title: Title) {
        return title.value;
    }
}
